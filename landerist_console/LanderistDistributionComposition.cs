using landerist_library.Application.Logging;
using landerist_library.Application.Statistics;
using landerist_library.Infrastructure.Administration;
using landerist_library.Infrastructure.Distribution;
using landerist_library.Infrastructure.Listings;
using landerist_library.Infrastructure.Sql;
using landerist_library.Infrastructure.Statistics;
using landerist_library.Infrastructure.Tasks;
using landerist_library.Infrastructure.WebsiteServices;
using Microsoft.Extensions.DependencyInjection;

namespace landerist_console;

internal static class LanderistDistributionComposition
{
    public static DailyTaskJob CreateDailyJob(
        LanderistDatabaseAdapterFactory databaseAdapters,
        SqlNotListingCacheService notListingCache,
        GlobalStatistics globalStatistics,
        HostStatistics hostStatistics,
        PageStatisticsRepository pageStatistics,
        WebsiteMetricsService websiteMetrics,
        SqlWebsiteCatalog websiteCatalog,
        WebsiteQueryRepository websiteQueries,
        IServiceProvider services,
        IApplicationLogger logger) => new(
            databaseAdapters.CreateAddressDataMaintenance(),
            notListingCache,
            databaseAdapters.CreateDatabaseBackupService(),
            globalStatistics,
            hostStatistics,
            new DistributionPublisher(
                globalStatistics,
                hostStatistics,
                pageStatistics,
                websiteMetrics,
                websiteCatalog,
                websiteQueries,
                new SqlListingAdministrationService(
                    services.GetRequiredService<ListingRepository>(),
                    services.GetRequiredService<ListingQueryRepository>(),
                    services.GetRequiredService<ListingStatisticsRepository>(),
                    services.GetRequiredService<MediaRepository>(),
                    services.GetRequiredService<SourceRepository>(),
                    logger)),
            logger);
}