using landerist_library.Pages;
using landerist_library.Websites;

namespace landerist_library.Application.Pages;

public interface IPageQueryService
{
    Page? GetByHash(string uriHash);
    IEnumerable<IReadOnlyList<Page>> GetBatches(int batchSize);
    IReadOnlyList<Page> GetByType(PageType pageType);
    IReadOnlyList<Page> GetUnknown();
    IReadOnlyList<Page> GetUnknown(int topRows);
    IReadOnlyList<Page> GetNextScrape(int topRows, bool extendToFillTopRows);
    IReadOnlyList<Page> GetNextScrapeFuture(int topRows);
    IReadOnlyList<Page> GetRecentlyUnpublishedListings(int topRows);
    IReadOnlyList<Page> GetScrapePages(int topRows);
    IReadOnlyList<Page> GetNonScraped(Website website);
    IReadOnlyList<Page> GetUnknown(Website website);
    IReadOnlyList<Page> GetUnknownHttpStatusCode();
    IReadOnlyList<string> GetUris();
    IReadOnlyList<string> GetUris(bool isListing);
    IReadOnlyList<Page> GetListingsWithHttpStatusCodeError();
    IReadOnlyList<Page> GetListingsWithParserInputHash();
    IReadOnlyList<Page> GetUrisLikePrint();
    IReadOnlyList<Page> GetPagesWithProhibitedUris(
        IEnumerable<string> prohibitedUriFragments);
    int Count();
}

public interface IPageMaintenanceService
{
    bool DeleteAll();
    bool RemoveListingParserInputHash(PageType? pageType = null);
}

