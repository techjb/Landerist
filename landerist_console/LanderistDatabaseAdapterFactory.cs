using landerist_library.Application.Listings;
using landerist_library.Application.Logging;
using landerist_library.Application.Statistics;
using landerist_library.Application.Websites;
using landerist_library.Database;
using landerist_library.Infrastructure.Administration;
using landerist_library.Infrastructure.Backup;
using landerist_library.Infrastructure.Listings;
using landerist_library.Infrastructure.Parsing;
using landerist_library.Infrastructure.Scraping;
using landerist_library.Infrastructure.Sql;
using landerist_library.Infrastructure.Statistics;
using landerist_library.Infrastructure.Location.Providers.Goolzoom;
using landerist_library.Infrastructure.Location.Providers.GoogleMaps;
using landerist_library.Websites;

namespace landerist_console;

internal sealed class LanderistDatabaseAdapterFactory(
    IDatabaseFactory databaseFactory)
{
    public SqlListingStore CreateListingStore(
        IGlobalStatisticsRepository statistics,
        IApplicationLogger logger) =>
        new(databaseFactory.Create(), statistics, logger);

    public SqlNotListingCacheService CreateNotListingCache(bool enabled) =>
        new(databaseFactory.Create(), enabled);

    public SqlListingEnricher CreateListingEnricher(
        IGoolzoomClient goolzoom,
        string googleMapsApiKey,
        IApplicationLogger logger) =>
        new(
            databaseFactory.Create(),
            new LegacyListingLocationEnricher(
                databaseFactory.Create(),
                goolzoom,
                new GoogleMapsApi(databaseFactory.Create(), googleMapsApiKey, logger),
                new GoolzoomCadastralReferenceProvider(
                    new AddressCadastralReference(databaseFactory.Create()),
                    new GlobalStatisticsRepository(databaseFactory.Create()),
                    goolzoom,
                    new LegacyAddressCandidateSelector(),
                    logger)));

    public SqlScrapeMetrics CreateScrapeMetrics() =>
        new(databaseFactory.Create());

    public SqlPageClassificationMetrics CreatePageClassificationMetrics() =>
        new(databaseFactory.Create());

    public SqlPageSelectionRepository CreatePageSelectionRepository(
        string machineName,
        PageQueryOptions options) =>
        new(databaseFactory.Create(), machineName, options);

    public SqlWebsiteThrottleService CreateWebsiteThrottle(
        IWebsiteRobotsPolicy robotsPolicy) =>
        new(databaseFactory.Create(), robotsPolicy);

    public SqlPageLockManager CreatePageLockManager(string machineName) =>
        new(databaseFactory.Create(), machineName);

    public SqlScrapeBatchMetrics CreateScrapeBatchMetrics() =>
        new(databaseFactory.Create());

    public SqlScrapePageSource CreateScrapePageSource(
        IListingStore listingStore) =>
        new(databaseFactory.Create(), listingStore);

    public SqlBatchStore CreateBatchStore() =>
        new(databaseFactory.Create());

    public SqlBatchRegistrationStore CreateBatchRegistrationStore() =>
        new(databaseFactory.Create());

    public LegacyAddressDataMaintenance CreateAddressDataMaintenance() =>
        new(databaseFactory.Create());

    public SqlDatabaseBackupService CreateDatabaseBackupService() =>
        new(databaseFactory.Create());
}
