using landerist_library.Application.Administration;
using landerist_library.Websites;
using landerist_library.Pages;
using landerist_library.Application;
using landerist_library.Application.Listings;
using landerist_library.Application.Pages;
using landerist_library.Application.Persistence;
using landerist_library.Application.Websites;
using landerist_library.Infrastructure.Runtime;

namespace landerist_library.Infrastructure.Administration;

public sealed partial class WebsiteAdministrationService : IWebsiteAdministrationService
{
    private readonly IWebsitePersistenceService Persistence;
    private readonly IWebsiteDeletionService Deletion;
    private readonly IWebsiteCatalog Catalog;
    private readonly IWebsiteMaintenanceService Maintenance;
    private readonly IWebsiteMetricsService Metrics;
    private readonly IPageQueryService PageQueries;
    private readonly IListingMaintenanceService ListingMaintenance;
    private readonly IPagePersistenceService PagePersistence;
    private readonly IPageMaintenanceService PageMaintenance;

    public WebsiteAdministrationService(
        IWebsitePersistenceService persistence,
        IWebsiteDeletionService deletion,
        IWebsiteCatalog catalog,
        IWebsiteMaintenanceService maintenance,
        IWebsiteMetricsService metrics,
        IPageQueryService pageQueries,
        IListingMaintenanceService listingMaintenance,
        IPagePersistenceService pagePersistence,
        IPageMaintenanceService pageMaintenance,
        IWebsiteNetworkService network,
        IWebsiteSitemapService sitemaps,
        IWebsiteRobotsPolicy robotsPolicy,
        AdministrationOptions options)
    {
        Persistence = persistence;
        Deletion = deletion;
        Catalog = catalog;
        Maintenance = maintenance;
        Metrics = metrics;
        PageQueries = pageQueries;
        ListingMaintenance = listingMaintenance;
        PagePersistence = pagePersistence;
        PageMaintenance = pageMaintenance;
        RefreshOperations = new WebsiteRefreshOperations(catalog, persistence, network, sitemaps);
        Reporting = new WebsiteAdministrationReporting(catalog, robotsPolicy, pagePersistence);
        ArgumentNullException.ThrowIfNull(options);
        options.Validate();
        FileCleanup = new WebsiteFileCleanup(
            catalog,
            metrics,
            deletion,
            options,
            new WebsiteCleanupCsvReader());
    }

    public bool Insert(Website website) => Persistence.Insert(website);

    public bool Update(Website website) => Persistence.Update(website);

    public bool DeleteWithRelations(Website website) =>
        Deletion.DeleteWithRelations(website);
}

