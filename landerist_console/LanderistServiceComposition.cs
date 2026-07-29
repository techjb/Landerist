using landerist_library.Infrastructure.Parsing.Tokenization;
using landerist_domain.Parsing.Tokenization;
using landerist_domain.Parsing.Prompt;
using landerist_library.Infrastructure.ListingMedia;
using landerist_library.Infrastructure.Ai.StructuredOutputs;
using landerist_domain.Parsing.Materialization;
using landerist_domain.Parsing.UserInput;
using landerist_library.Infrastructure.Ai.LocalAI;
using landerist_library.Infrastructure.Ai.OpenAI;
using landerist_library.Parsing;
using landerist_library.Application.Parsing;
using landerist_domain.Parsing.StructuredOutputs;
using landerist_library.Infrastructure.Ai.Vertex;
using landerist_library.Infrastructure.Browser;
using landerist_library.Infrastructure.Downloaders.Puppeteer;
using landerist_library.Infrastructure.Downloaders.Multiple;
using landerist_library.Infrastructure.Location.Providers.Goolzoom;
using landerist_library.Websites;
using landerist_library.Infrastructure.Statistics;
using landerist_library.Infrastructure.Ai.OpenAI.Batch;
using landerist_library.Infrastructure.Parsing;
using landerist_library.Infrastructure.Parsing.UserInput;
using landerist_library.Application.Listings;
using landerist_library.Application.Logging;
using landerist_library.Application.Persistence;
using landerist_library.Application.Scraping;
using landerist_library.Application.Tasks;
using landerist_library.Application.Websites;
using landerist_library.Infrastructure.Administration;
using landerist_library.Infrastructure.Ai;
using landerist_library.Infrastructure.Ai.Batch;
using landerist_library.Infrastructure.Backup;
using landerist_library.Infrastructure.Distribution;
using landerist_library.Infrastructure.Downloaders;
using landerist_library.Infrastructure.Logging;
using landerist_library.Infrastructure.Http;
using landerist_library.Infrastructure.Indexing;
using landerist_library.Infrastructure.PageServices;
using landerist_library.Infrastructure.Listings;
using landerist_library.Infrastructure.Sql;
using landerist_library.Infrastructure.Scraping;
using landerist_library.Infrastructure.Tasks;
using landerist_library.Infrastructure.WebsiteServices;
using landerist_library.Logs;
using landerist_library.Application.Statistics;
using landerist_library.Infrastructure.Runtime;
using Microsoft.Extensions.DependencyInjection;

namespace landerist_console;

internal static class LanderistServiceComposition
{
    public static TasksService CreateTasksService(
        LanderistRuntimeOptions runtimeOptions,
        IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(runtimeOptions);
        ArgumentNullException.ThrowIfNull(services);
        runtimeOptions.Validate();
        LanderistDatabaseAdapterFactory databaseAdapters =
            services.GetRequiredService<LanderistDatabaseAdapterFactory>();
        HttpClientTransportFactory httpClients =
            services.GetRequiredService<HttpClientTransportFactory>();
        landerist_library.Tools.ScrapingBee.Configure(
            runtimeOptions.Integrations.ScrapingBeeApiKey,
            httpClients);
        landerist_library.Export.S3.Configure(new landerist_library.Export.S3Options(
            runtimeOptions.Integrations.AwsAccessKeyId,
            runtimeOptions.Integrations.AwsSecretAccessKey,
            runtimeOptions.Integrations.AwsDownloadsBucket,
            runtimeOptions.Integrations.AwsWebsiteBucket));
        IApplicationLogger logger =
            services.GetRequiredService<IApplicationLogger>();
        GoolzoomApi goolzoom = services.GetRequiredService<GoolzoomApi>();
        WebsiteNetworkService websiteNetwork =
            services.GetRequiredService<WebsiteNetworkService>();
PagePersistenceService pagePersistence =
            services.GetRequiredService<PagePersistenceService>();
        WebsitePersistenceService websitePersistence =
            services.GetRequiredService<WebsitePersistenceService>();
        SqlListingStore listingStore =
            services.GetRequiredService<SqlListingStore>();
        SqlListingQueryService listingQueries =
            services.GetRequiredService<SqlListingQueryService>();
        SqlNotListingCacheService notListingCache =
            services.GetRequiredService<SqlNotListingCacheService>();
        PageQueryOptions pageQueryOptions =
            services.GetRequiredService<PageQueryOptions>();
        SqlPageCatalog pageCatalog =
            services.GetRequiredService<SqlPageCatalog>();
        SqlPageWaitingStatusService waitingStatus =
            services.GetRequiredService<SqlPageWaitingStatusService>();
        PageStatisticsRepository pageStatistics =
            services.GetRequiredService<PageStatisticsRepository>();
        WebsiteQueryRepository websiteQueries =
            services.GetRequiredService<WebsiteQueryRepository>();
        SqlWebsiteCatalog websiteCatalog =
            services.GetRequiredService<SqlWebsiteCatalog>();
        WebsiteMetricsService websiteMetrics =
            services.GetRequiredService<WebsiteMetricsService>();        WebsiteRobotsPolicy robotsPolicy =
            services.GetRequiredService<WebsiteRobotsPolicy>();
        WebsiteAccessServices websiteAccess =
            services.GetRequiredService<WebsiteAccessServices>();
        WebsiteSitemapService websiteSitemaps = new(
            runtimeOptions.Scraping.IndexerEnabled,
            robotsPolicy,
            TimeProvider.System,
            new LegacyWebsiteSitemapIndexerFactory(
                robotsPolicy,
                httpClients,
                pagePersistence,
                websiteMetrics),
            logger);
        ParseListing listingParser = services.GetRequiredService<ParseListing>();
        GlobalStatistics globalStatistics =
            services.GetRequiredService<GlobalStatistics>();
        HostStatistics hostStatistics =
            services.GetRequiredService<HostStatistics>();
        SqlPageLinkService pageLinks =
            services.GetRequiredService<SqlPageLinkService>();
        ListingLifecycleService listingLifecycle =
            services.GetRequiredService<ListingLifecycleService>();
        LanderistScrapingPipeline scrapingPipeline = services
            .GetRequiredService<LanderistScrapingPipelineFactory>()
            .Create(
                pagePersistence,
                listingLifecycle,
                notListingCache,
                hostStatistics,
                listingParser,
                pageLinks,
                listingStore,
                pageQueryOptions);
        Scraper scraper = scrapingPipeline.Scraper;
        ScrapeBatchServices batchScraping = scrapingPipeline.BatchServices;
        ParsedPageClassificationService parsedClassification =
            scrapingPipeline.ParsedClassification;
        TasksExecutionMode executionMode = runtimeOptions.Role switch
        {
            LanderistExecutionRole.LocalAi => TasksExecutionMode.LocalAi,
            LanderistExecutionRole.Principal => TasksExecutionMode.Principal,
            LanderistExecutionRole.Scraper => TasksExecutionMode.Scraper,
            _ => throw new ArgumentOutOfRangeException(
                nameof(runtimeOptions),
                runtimeOptions.Role,
                "Unknown execution role.")
        };

        LanderistBatchTasks batchTasks = services
            .GetRequiredService<LanderistBatchComposition>()
            .Create(
            parsedClassification,
            globalStatistics,
            pageCatalog,
            pagePersistence,
            waitingStatus,
            listingParser);
        return new TasksService(
            new TasksServiceOptions(executionMode),
            new SystemRecurringTaskScheduler(),
            logger,
            new ScrapeTaskJob(scraper, batchScraping.Browser),
            new LocalAiTaskJob(() => new TaskLocalAIParsing(
                parsedClassification,
                globalStatistics,
                waitingStatus,
                pageCatalog,
                pagePersistence,
                new LegacyLocalAiListingParser(listingParser, hostStatistics),
                new PageListingInputPreparer(logger),
                new LocalAiParsingTaskOptions(
                    modelMaxTokens: runtimeOptions.Execution.LocalAiMaxModelLength,
                    runSequentially: runtimeOptions.Execution.IsLocal,
                    updateWaitingStatusOnStart: runtimeOptions.Execution.IsProduction),
                new LegacyLocalAiTokenBudget(
                    new Tokenizer(TokenizerOptions.ForProvider(LLMProvider.LocalAI))),
                logger)),
            batchTasks.TenMinute,
            new HourlyTaskJob(
                new WebsiteRefreshService(
                    websiteCatalog,
                    websitePersistence,
                    websiteNetwork,
                    websiteSitemaps),
                batchTasks.Cleaner),
            services.GetRequiredService<LanderistDistributionComposition>()
                .CreateDailyJob(
                notListingCache,
                globalStatistics,
                hostStatistics,
                pageStatistics,
                websiteMetrics,
                websiteCatalog,
                websiteQueries),
            TimeProvider.System);
    }
}
