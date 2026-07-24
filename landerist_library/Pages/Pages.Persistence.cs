using landerist_library.Application;
using landerist_library.Application.Listings;
using landerist_library.Application.Pages;
using landerist_library.Application.Persistence;
using landerist_library.Application.Websites;
using landerist_orels.ES;

namespace landerist_library.Pages;

public partial class Pages
{
    private static IPagePersistenceService Persistence =>
        LanderistApplication.Services.PagePersistence;

    private static IPageQueryService Queries =>
        LanderistApplication.Services.PageQueries;

    private static IPageMaintenanceService Maintenance =>
        LanderistApplication.Services.PageMaintenance;

    private static IWebsiteCatalog WebsiteCatalog =>
        LanderistApplication.Services.WebsiteCatalog;

    private static IListingQueryService ListingQueries =>
        LanderistApplication.Services.ListingQueries;

    private static IListingMaintenanceService ListingMaintenance =>
        LanderistApplication.Services.ListingMaintenance;

    public static bool Insert(Page page) => Persistence.Insert(page);

    public static bool Update(Page page) => Persistence.Update(page);

    public static bool UpdateNextScrape(Page page) => Persistence.UpdateNextScrape(page);

    public static bool Delete(Page page)
    {
        ArgumentNullException.ThrowIfNull(page);
        bool success = Persistence.Delete(page);
        return success && ListingMaintenance.Delete(page.UriHash);
    }

    public static bool DeleteListing(Page page)
    {
        ArgumentNullException.ThrowIfNull(page);
        Listing? listing = ListingQueries.Get(page, loadMedia: false, loadSources: false);
        return listing is not null && ListingMaintenance.Delete(listing.guid);
    }

    public static bool ListingParserInputExistsOnAnotherListing(Page page) =>
        Persistence.ListingParserInputExistsOnAnotherListing(page);
}