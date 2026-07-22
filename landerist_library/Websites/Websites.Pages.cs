using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Infrastructure.Sql;
using landerist_library.Pages;
using landerist_orels.ES;

namespace landerist_library.Websites;

public partial class Websites
{
    private static readonly WebsitePageMetricsRepository PageMetrics = new();

    public static bool DeleteWithRelations(Website website)
    {
        ArgumentNullException.ThrowIfNull(website);
        DeleteListings(website);
        Pages.Pages.Delete(website);
        return DeleteRecord(website);
    }

    public static void DeleteListings(Website website)
    {
        foreach (Page page in GetPages(website))
        {
            Pages.Pages.DeleteListing(page);
        }
    }

    public static bool InsertMainPage(Website website)
    {
        ArgumentNullException.ThrowIfNull(website);
        return Pages.Pages.Insert(new Page(website));
    }

    public static List<Page> GetPages(Website website) => Pages.Pages.GetPages(website);

    public static List<Page> GetUnknownPageType(Website website) => Pages.Pages.GetUnknowPageType(website);

    public static List<Page> GetNonScrapedPages(Website website) => Pages.Pages.GetNonScrapedPages(website);

    public static int GetNumPages(Website website) => PageMetrics.CountPages(website.Host);

    public static int GetNumPagesScrapedSince(Website website, DateTime dateFrom) =>
        PageMetrics.CountPagesSince(website.Host, "LastScrape", dateFrom);

    public static int GetNumPagesInsertedSince(Website website, DateTime dateFrom) =>
        PageMetrics.CountPagesSince(website.Host, "Inserted", dateFrom);

    public static int GetNumPagesParseListingSince(Website website, DateTime dateFrom) =>
        PageMetrics.CountPagesSince(website.Host, "LastParseListing", dateFrom);

    public static bool AchievedMaxNumberOfPages(Website website) =>
        GetNumPages(website) >= Config.MAX_PAGES_PER_WEBSITE;

    public static int GetNumListings(Website website) => ES_Listings.Count(website.Host);

    public static int GetNumListingsSinceListingDate(Website website, DateTime dateFrom) =>
        ES_Listings.CountSinceListingDate(website.Host, dateFrom);

    public static int GetNumPublishedListings(Website website) =>
        ES_Listings.Count(website.Host, ListingStatus.published);

    public static int GetNumPublishedListingsWithAddress(Website website) =>
        ES_Listings.CountWithAddress(website.Host, ListingStatus.published);

    public static int GetNumPublishedListingsWithCoordinates(Website website) =>
        ES_Listings.CountWithCoordinates(website.Host, ListingStatus.published);

    public static int GetNumPublishedListingsWithImages(Website website) =>
        ES_Listings.CountWithImages(website.Host, ListingStatus.published);

    public static int GetNumUnpublishedListings(Website website) =>
        ES_Listings.Count(website.Host, ListingStatus.unpublished);
}