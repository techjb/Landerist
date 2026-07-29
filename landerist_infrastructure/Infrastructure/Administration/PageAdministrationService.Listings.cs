using landerist_library.Websites;
using landerist_library.Pages;
using landerist_orels.ES;

namespace landerist_library.Infrastructure.Administration;

public sealed partial class PageAdministrationService
{

    public Listing? GetListing(Page page, bool loadMedia, bool loadSources)
    {
        ArgumentNullException.ThrowIfNull(page);
        return ListingQueries.Get(page, loadMedia, loadSources);
    }

    public bool ContainsListing(Page page) => GetListing(page, false, false) is not null;

    public bool IsListingStatusPublished(Page page) =>
        GetListing(page, false, false)?.listingStatus == ListingStatus.published;

    public bool IsListingStatusUnpublished(Page page) =>
        GetListing(page, false, false)?.listingStatus == ListingStatus.unpublished;

    public ListingUnpublishDecision GetListingUnpublishDecision(Page page) =>
        new ListingUnpublishEvaluator(page, ContainsListing).Evaluate();

    public ListingStatus? GetListingStatus(Page page) =>
        GetListing(page, false, false)?.listingStatus;

    public void SetNextScrape(Page page) =>
        page.SetNextScrape(GetListingStatus(page));

    public void SetNextScrapeFromNow(Page page) =>
        page.SetNextScrapeFromNow(GetListingStatus(page));

    public void SetPageTypeAndNextScrape(Page page, PageType pageType)
    {
        page.SetPageType(pageType);
        SetNextScrapeFromNow(page);
    }
    public bool IsNotCanonicalListing(Page page) =>
        page.IsNotCanonical() && ContainsListing(page);

    public bool IsRedirectToAnotherUrlListing(Page page) =>
        page.IsRedirectToAnotherUrl() && ContainsListing(page);
}
