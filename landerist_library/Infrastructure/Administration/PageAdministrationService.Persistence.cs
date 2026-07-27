using landerist_library.Websites;
using landerist_library.Pages;
using landerist_library.Application.Administration;
using landerist_library.Application.Listings;
using landerist_library.Application.Pages;
using landerist_library.Application.Persistence;
using landerist_library.Application.Websites;
using landerist_orels.ES;

namespace landerist_library.Infrastructure.Administration;

public sealed partial class PageAdministrationService : IPageAdministrationService
{
    private const int GET_ALL_PAGES_BATCH_SIZE = 3000;
    private readonly IPagePersistenceService Persistence;
    private readonly IPageQueryService Queries;
    private readonly IPageMaintenanceService Maintenance;
    private readonly IWebsiteCatalog WebsiteCatalog;
    private readonly IListingQueryService ListingQueries;
    private readonly IListingMaintenanceService ListingMaintenance;
    private readonly IWebsiteRobotsPolicy RobotsPolicy;

    public PageAdministrationService(
        IPagePersistenceService persistence,
        IPageQueryService queries,
        IPageMaintenanceService maintenance,
        IWebsiteCatalog websiteCatalog,
        IListingQueryService listingQueries,
        IListingMaintenanceService listingMaintenance,
        IWebsiteRobotsPolicy robotsPolicy)
    {
        Persistence = persistence;
        Queries = queries;
        Maintenance = maintenance;
        WebsiteCatalog = websiteCatalog;
        ListingQueries = listingQueries;
        ListingMaintenance = listingMaintenance;
        RobotsPolicy = robotsPolicy;
    }

    public bool Insert(Page page) => Persistence.Insert(page);

    public bool Update(Page page) => Persistence.Update(page);

    public bool UpdateNextScrape(Page page) => Persistence.UpdateNextScrape(page);

    public bool Delete(Page page)
    {
        ArgumentNullException.ThrowIfNull(page);
        bool success = Persistence.Delete(page);
        return success && ListingMaintenance.Delete(page.UriHash);
    }

    public bool DeleteListing(Page page)
    {
        ArgumentNullException.ThrowIfNull(page);
        Listing? listing = ListingQueries.Get(page, loadMedia: false, loadSources: false);
        return listing is not null && ListingMaintenance.Delete(listing.guid);
    }

    public bool ListingParserInputExistsOnAnotherListing(Page page) =>
        Persistence.ListingParserInputExistsOnAnotherListing(page);
}
