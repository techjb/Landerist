using landerist_library.Application.Listings;
using landerist_library.Application.Logging;
using landerist_library.Application.Persistence;
using landerist_library.Application.Statistics;
using landerist_library.Infrastructure.Listings;
using landerist_library.Infrastructure.PageServices;
using landerist_library.Infrastructure.Runtime;
using landerist_library.Infrastructure.Sql;
using landerist_library.Infrastructure.Sql.Statistics;
using landerist_library.Infrastructure.WebsiteServices;
using Microsoft.Extensions.DependencyInjection;

namespace landerist_console;

internal static class LanderistPersistenceAdapterServiceCollectionExtensions
{
    public static IServiceCollection AddLanderistPersistenceServices(
        this IServiceCollection services,
        LanderistRuntimeOptions runtimeOptions)
    {
        services.AddSingleton<PagePersistenceService>();
        services.AddSingleton<WebsitePersistenceService>();
        services.AddSingleton<SqlListingQueryService>();
        services.AddSingleton<IListingAdministrationService,
            SqlListingAdministrationService>();
        services.AddSingleton<SqlPageCatalog>();
        services.AddSingleton<SqlPageWaitingStatusService>();
        services.AddSingleton<SqlWebsiteCatalog>();
        services.AddSingleton<SqlListingStore>(serviceProvider =>
            serviceProvider.GetRequiredService<LanderistDatabaseAdapterFactory>()
                .CreateListingStore(
                    serviceProvider.GetRequiredService<GlobalStatisticsRepository>(),
                    serviceProvider.GetRequiredService<IApplicationLogger>()));
        services.AddSingleton<SqlNotListingCacheService>(serviceProvider =>
            serviceProvider.GetRequiredService<LanderistDatabaseAdapterFactory>()
                .CreateNotListingCache(
                    runtimeOptions.Scraping.NotListingCacheEnabled));
        services.AddSingleton<WebsiteMetricsService>(serviceProvider => new(
            serviceProvider.GetRequiredService<WebsitePageMetricsRepository>(),
            serviceProvider.GetRequiredService<ListingStatisticsRepository>(),
            runtimeOptions.Scraping.MaxPagesPerWebsite));
        services.AddSingleton<GlobalStatistics>(serviceProvider => new(
            serviceProvider.GetRequiredService<GlobalStatisticsRepository>(),
            persistenceEnabled: !runtimeOptions.Execution.IsLocal));
        services.AddSingleton<HostStatistics>(serviceProvider => new(
            serviceProvider.GetRequiredService<HostStatisticsRepository>(),
            serviceProvider.GetRequiredService<SqlWebsiteCatalog>(),
            persistenceEnabled: !runtimeOptions.Execution.IsLocal));
        return services;
    }
}
