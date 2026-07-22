using landerist_library.Configuration;
using landerist_library.Database;
using landerist_orels.ES;

namespace landerist_library.Pages;

public partial class Pages
{
    public static bool IsNotListingCache(Page page)
    {
        ArgumentNullException.ThrowIfNull(page);
        return Config.NOT_LISTING_CACHE_ENABLED &&
            !string.IsNullOrEmpty(page.ListingParserInputHash) &&
            NotListingsCache.IsNotListing(page.Host, page.ListingParserInputHash);
    }

    public static bool InsertToNotListingCache(Page page)
    {
        ArgumentNullException.ThrowIfNull(page);
        return page.ListingParserInputHash is not null &&
            NotListingsCache.Insert(page.Host, page.ListingParserInputHash);
    }

    public static Listing? GetListing(Page page, bool loadMedia, bool loadSources)
    {
        ArgumentNullException.ThrowIfNull(page);
        return ES_Listings.GetListing(page, loadMedia, loadSources);
    }

    public static bool ContainsListing(Page page) => GetListing(page, false, false) is not null;

    public static bool IsListingStatusPublished(Page page) =>
        GetListing(page, false, false)?.listingStatus == ListingStatus.published;

    public static bool IsListingStatusUnpublished(Page page) =>
        GetListing(page, false, false)?.listingStatus == ListingStatus.unpublished;

    public static ListingUnpublishDecision GetListingUnpublishDecision(Page page) =>
        new ListingUnpublishEvaluator(page).Evaluate();

    public static ListingStatus? GetListingStatus(Page page) =>
        GetListing(page, false, false)?.listingStatus;

    public static void SetNextScrape(Page page) =>
        page.SetNextScrape(GetListingStatus(page));

    public static void SetNextScrapeFromNow(Page page) =>
        page.SetNextScrapeFromNow(GetListingStatus(page));

    public static void SetPageTypeAndNextScrape(Page page, PageType pageType)
    {
        page.SetPageType(pageType);
        SetNextScrapeFromNow(page);
    }
    public static bool IsNotCanonicalListing(Page page) =>
        page.IsNotCanonical() && ContainsListing(page);

    public static bool IsRedirectToAnotherUrlListing(Page page) =>
        page.IsRedirectToAnotherUrl() && ContainsListing(page);
}