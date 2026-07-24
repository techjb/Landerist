using landerist_library.Pages;
using landerist_library.Websites;

namespace landerist_library.Infrastructure.Administration;

public sealed partial class PageAdministrationService
{
    public Page LoadOrCreate(Uri uri)
    {
        ArgumentNullException.ThrowIfNull(uri);
        Page? existing = GetPage(Tools.Strings.GetHash(uri.ToString()));
        if (existing is not null)
        {
            return existing;
        }

        Website website = WebsiteCatalog.Get(uri.Host);
        return new Page(website, uri);
    }

    public Page? GetPage(string uriHash) => Queries.GetByHash(uriHash);

    public IReadOnlyList<Page> GetPages()
    {
        Console.WriteLine("Reading all pages");
        List<Page> pages = [];
        int batchNumber = 0;
        foreach (List<Page> batch in GetPageBatches())
        {
            batchNumber++;
            pages.AddRange(batch);
            Console.WriteLine("Read batch " + batchNumber + ": " + batch.Count +
                " pages. Total: " + pages.Count);
        }
        return pages;
    }

    public IReadOnlyList<Page> GetPages(PageType pageType) =>
        [.. Queries.GetByType(pageType)];

    public List<Page> GetUnknownPageType() => [.. Queries.GetUnknown()];

    public List<Page> GetUnknownPageType(int topRows) =>
        [.. Queries.GetUnknown(topRows)];

    public List<Page> GetNextScrape(int topRows, bool extendToFillTopRows) =>
        [.. Queries.GetNextScrape(topRows, extendToFillTopRows)];

    public List<Page> GetNextScrapeFuture(int topRows) =>
        [.. Queries.GetNextScrapeFuture(topRows)];

    public List<Page> GetRecentlyUnpublishedListingsPages(int topRows) =>
        [.. Queries.GetRecentlyUnpublishedListings(topRows)];

    public List<Page> GetScrapePages(int topRows) =>
        [.. Queries.GetScrapePages(topRows)];

    public List<Page> GetNonScrapedPages(Website website) =>
        [.. Queries.GetNonScraped(website)];

    public List<Page> GetUnknowPageType(Website website) =>
        [.. Queries.GetUnknown(website)];

    public List<Page> GetUnknowHttpStatusCode() =>
        [.. Queries.GetUnknownHttpStatusCode()];

    public List<string> GetUris(bool isListing) =>
        [.. Queries.GetUris(isListing)];

    public IReadOnlyList<string> GetUris() => [.. Queries.GetUris()];

    private int CountPages() => Queries.Count();

    private IEnumerable<List<Page>> GetPageBatches(
        int batchSize = GET_ALL_PAGES_BATCH_SIZE)
    {
        foreach (IReadOnlyList<Page> batch in Queries.GetBatches(batchSize))
        {
            yield return [.. batch];
        }
    }
}

