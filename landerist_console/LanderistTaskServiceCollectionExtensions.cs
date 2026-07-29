using landerist_domain.Parsing.Tokenization;
using landerist_library.Application.Listings;
using landerist_library.Application.Logging;
using landerist_library.Application.Persistence;
using landerist_library.Application.Scraping;
using landerist_library.Application.Statistics;
using landerist_library.Application.Tasks;
using landerist_library.Application.Websites;
using landerist_library.Infrastructure.Listings;
using landerist_library.Infrastructure.PageServices;
using landerist_library.Infrastructure.Parsing;
using landerist_library.Infrastructure.Parsing.Tokenization;
using landerist_library.Infrastructure.Parsing.UserInput;
using landerist_library.Infrastructure.Runtime;
using landerist_library.Infrastructure.Scraping;
using landerist_library.Infrastructure.Sql;
using landerist_library.Infrastructure.Statistics;
using landerist_library.Infrastructure.Tasks;
using landerist_library.Infrastructure.WebsiteServices;
using landerist_library.Parsing;
using Microsoft.Extensions.DependencyInjection;

namespace landerist_console;

internal static class LanderistTaskServiceCollectionExtensions
{
    public static IServiceCollection AddLanderistTasks(
        this IServiceCollection services,
        LanderistRuntimeOptions runtimeOptions)
    {
        services.AddSingleton<LanderistAiComposition>();
        services.AddSingleton<ParseListing>(serviceProvider =>
            serviceProvider.GetRequiredService<LanderistAiComposition>()
                .CreateListingParser());
        services.AddSingleton<LanderistBatchComposition>();
        services.AddSingleton<LanderistDistributionComposition>();
        services.AddSingleton<LanderistScrapingPipeline>(serviceProvider =>
            serviceProvider.GetRequiredService<LanderistScrapingPipelineFactory>()
                .Create(
                    serviceProvider.GetRequiredService<PagePersistenceService>(),
                    serviceProvider.GetRequiredService<ListingLifecycleService>(),
                    serviceProvider.GetRequiredService<SqlNotListingCacheService>(),
                    serviceProvider.GetRequiredService<HostStatistics>(),
                    serviceProvider.GetRequiredService<ParseListing>(),
                    serviceProvider.GetRequiredService<SqlPageLinkService>(),
                    serviceProvider.GetRequiredService<SqlListingStore>(),
                    serviceProvider.GetRequiredService<PageQueryOptions>()));
        services.AddSingleton<LanderistBatchTasks>(serviceProvider =>
        {
            LanderistScrapingPipeline pipeline =
                serviceProvider.GetRequiredService<LanderistScrapingPipeline>();
            return serviceProvider.GetRequiredService<LanderistBatchComposition>()
                .Create(
                    pipeline.ParsedClassification,
                    serviceProvider.GetRequiredService<GlobalStatistics>(),
                    serviceProvider.GetRequiredService<SqlPageCatalog>(),
                    serviceProvider.GetRequiredService<PagePersistenceService>(),
                    serviceProvider.GetRequiredService<SqlPageWaitingStatusService>(),
                    serviceProvider.GetRequiredService<ParseListing>());
        });
        services.AddSingleton<ScrapeTaskJob>(serviceProvider =>
        {
            LanderistScrapingPipeline pipeline =
                serviceProvider.GetRequiredService<LanderistScrapingPipeline>();
            return new ScrapeTaskJob(
                pipeline.Scraper,
                pipeline.BatchServices.Browser);
        });
        services.AddSingleton<LocalAiTaskJob>(serviceProvider => new(() =>
        {
            LanderistScrapingPipeline pipeline =
                serviceProvider.GetRequiredService<LanderistScrapingPipeline>();
            IApplicationLogger logger =
                serviceProvider.GetRequiredService<IApplicationLogger>();
            return new TaskLocalAIParsing(
                pipeline.ParsedClassification,
                serviceProvider.GetRequiredService<GlobalStatistics>(),
                serviceProvider.GetRequiredService<SqlPageWaitingStatusService>(),
                serviceProvider.GetRequiredService<SqlPageCatalog>(),
                serviceProvider.GetRequiredService<PagePersistenceService>(),
                new LegacyLocalAiListingParser(
                    serviceProvider.GetRequiredService<ParseListing>(),
                    serviceProvider.GetRequiredService<HostStatistics>()),
                new PageListingInputPreparer(logger),
                new LocalAiParsingTaskOptions(
                    modelMaxTokens: runtimeOptions.Execution.LocalAiMaxModelLength,
                    runSequentially: runtimeOptions.Execution.IsLocal,
                    updateWaitingStatusOnStart: runtimeOptions.Execution.IsProduction),
                new LegacyLocalAiTokenBudget(
                    new Tokenizer(TokenizerOptions.ForProvider(LLMProvider.LocalAI))),
                logger);
        }));
        services.AddSingleton<HourlyTaskJob>(serviceProvider => new(
            new WebsiteRefreshService(
                serviceProvider.GetRequiredService<SqlWebsiteCatalog>(),
                serviceProvider.GetRequiredService<WebsitePersistenceService>(),
                serviceProvider.GetRequiredService<WebsiteNetworkService>(),
                serviceProvider.GetRequiredService<WebsiteSitemapService>()),
            serviceProvider.GetRequiredService<LanderistBatchTasks>().Cleaner));
        services.AddSingleton<DailyTaskJob>(serviceProvider =>
            serviceProvider.GetRequiredService<LanderistDistributionComposition>()
                .CreateDailyJob(
                    serviceProvider.GetRequiredService<SqlNotListingCacheService>(),
                    serviceProvider.GetRequiredService<GlobalStatistics>(),
                    serviceProvider.GetRequiredService<HostStatistics>(),
                    serviceProvider.GetRequiredService<PageStatisticsRepository>(),
                    serviceProvider.GetRequiredService<WebsiteMetricsService>(),
                    serviceProvider.GetRequiredService<SqlWebsiteCatalog>(),
                    serviceProvider.GetRequiredService<WebsiteQueryRepository>()));
        services.AddSingleton<TasksService>(serviceProvider => new(
            new TasksServiceOptions(GetExecutionMode(runtimeOptions.Role)),
            new SystemRecurringTaskScheduler(),
            serviceProvider.GetRequiredService<IApplicationLogger>(),
            serviceProvider.GetRequiredService<ScrapeTaskJob>(),
            serviceProvider.GetRequiredService<LocalAiTaskJob>(),
            serviceProvider.GetRequiredService<LanderistBatchTasks>().TenMinute,
            serviceProvider.GetRequiredService<HourlyTaskJob>(),
            serviceProvider.GetRequiredService<DailyTaskJob>(),
            TimeProvider.System));
        return services;
    }

    private static TasksExecutionMode GetExecutionMode(
        LanderistExecutionRole role) => role switch
        {
            LanderistExecutionRole.LocalAi => TasksExecutionMode.LocalAi,
            LanderistExecutionRole.Principal => TasksExecutionMode.Principal,
            LanderistExecutionRole.Scraper => TasksExecutionMode.Scraper,
            _ => throw new ArgumentOutOfRangeException(
                nameof(role), role, "Unknown execution role.")
        };
}