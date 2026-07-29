using landerist_library.Application.Listings;
using landerist_library.Application.Logging;
using landerist_library.Application.Statistics;
using landerist_library.Infrastructure.Distribution;
using landerist_library.Infrastructure.Listings;
using landerist_library.Infrastructure.Sql;
using landerist_library.Infrastructure.Statistics;
using landerist_library.Infrastructure.Tasks;
using landerist_library.Infrastructure.WebsiteServices;

namespace landerist_console;

internal sealed class LanderistDistributionComposition(
    LanderistDatabaseAdapterFactory databaseAdapters,
    IListingAdministrationService listingAdministration,
    IApplicationLogger logger)
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
                listingAdministration),
            logger);
}