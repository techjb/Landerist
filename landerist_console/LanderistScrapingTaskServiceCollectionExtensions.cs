using landerist_library.Application.Listings;
using landerist_library.Application.Persistence;
using landerist_library.Application.Scraping;
using landerist_library.Application.Statistics;
using landerist_library.Application.Tasks;
using landerist_library.Infrastructure.Listings;
using landerist_library.Infrastructure.PageServices;
using landerist_library.Infrastructure.Parsing;
using landerist_library.Infrastructure.Sql;
using landerist_library.Infrastructure.Sql.Statistics;
using landerist_library.Infrastructure.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace landerist_console;

internal static class LanderistScrapingTaskServiceCollectionExtensions
{
    public static IServiceCollection AddLanderistScrapingTasks(
        this IServiceCollection services)
    {
        services.AddSingleton<LanderistBatchProviderComposition>();
        services.AddSingleton<LanderistBatchComposition>();
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
        return services;
    }
}
