using landerist_library.Application.Listings;
using landerist_library.Application.Scraping;
using landerist_library.Pages;

namespace landerist_library.Infrastructure.Sql.Scraping;

public sealed class SqlPageSchedulingService : IPageSchedulingService
{
    private readonly IListingStore _listings;

    public SqlPageSchedulingService(IListingStore listings)
    {
        ArgumentNullException.ThrowIfNull(listings);
        _listings = listings;
    }

    public void SetNextScrape(Page page) =>
        page.SetNextScrape(GetListingStatus(page));

    public void SetNextScrapeFromNow(Page page) =>
        page.SetNextScrapeFromNow(GetListingStatus(page));

    private landerist_orels.ES.ListingStatus? GetListingStatus(Page page) =>
        _listings.Get(page, loadMedia: false, loadSources: false)?.listingStatus;
}
