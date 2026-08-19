using landerist_library.Application.Listings;
using landerist_library.Application.Logging;
using landerist_library.Application.Statistics;
using landerist_library.Infrastructure.Distribution;
using landerist_library.Infrastructure.Listings;
using landerist_library.Infrastructure.Sql;
using landerist_library.Infrastructure.Sql.Statistics;
using landerist_library.Infrastructure.Tasks;
using landerist_library.Infrastructure.WebsiteServices;
using landerist_library.Infrastructure.Runtime;
using landerist_library.Infrastructure.Logging;
using landerist_library.Database;

namespace landerist_console;

internal sealed class LanderistDistributionComposition(
    LanderistDatabaseAdapterFactory databaseAdapters,
    IDatabaseFactory databaseFactory,
    IListingAdministrationService listingAdministration,
    IApplicationLogger logger,
    LanderistRuntimeOptions runtimeOptions)
{
    public DailyTaskJob CreateDailyJob(
        SqlNotListingCacheService notListingCache,
        GlobalStatistics globalStatistics,
        HostStatistics hostStatistics,
        PageStatisticsRepository pageStatistics,
        WebsiteMetricsService websiteMetrics,
        SqlWebsiteCatalog websiteCatalog,
        WebsiteQueryRepository websiteQueries) => new(
            databaseAdapters.CreateAddressDataMaintenance(),
            notListingCache,
            databaseAdapters.CreateDatabaseBackupService(
                runtimeOptions.Backup,
                logger),
            globalStatistics,
            hostStatistics,
            new DistributionPublisher(
                globalStatistics,
                hostStatistics,
                pageStatistics,
                websiteMetrics,
                websiteCatalog,
                websiteQueries,
                listingAdministration,
                runtimeOptions.Distribution,
                logger),
            new SqlLogRetentionService(
                databaseFactory,
                runtimeOptions.LogRetention,
                TimeProvider.System),
            logger);
}
