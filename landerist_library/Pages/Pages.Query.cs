using landerist_library.Websites;

namespace landerist_library.Pages;

public partial class Pages
{
    public static Page LoadOrCreate(Uri uri)
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

    public static Page? GetPage(string uriHash) => Queries.GetByHash(uriHash);

    public static List<Page> GetPages()
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

    public static List<Page> GetPages(PageType pageType) =>
        [.. Queries.GetByType(pageType)];

    public static List<Page> GetUnknownPageType() => [.. Queries.GetUnknown()];

    public static List<Page> GetUnknownPageType(int topRows) =>
        [.. Queries.GetUnknown(topRows)];

    public static List<Page> GetNextScrape(int topRows, bool extendToFillTopRows) =>
        [.. Queries.GetNextScrape(topRows, extendToFillTopRows)];

    public static List<Page> GetNextScrapeFuture(int topRows) =>
        [.. Queries.GetNextScrapeFuture(topRows)];

    public static List<Page> GetRecentlyUnpublishedListingsPages(int topRows) =>
        [.. Queries.GetRecentlyUnpublishedListings(topRows)];

    public static List<Page> GetScrapePages(int topRows) =>
        [.. Queries.GetScrapePages(topRows)];

    public static List<Page> GetNonScrapedPages(Website website) =>
        [.. Queries.GetNonScraped(website)];

    public static List<Page> GetUnknowPageType(Website website) =>
        [.. Queries.GetUnknown(website)];

    public static List<Page> GetUnknowHttpStatusCode() =>
        [.. Queries.GetUnknownHttpStatusCode()];

    public static List<string> GetUris(bool isListing) =>
        [.. Queries.GetUris(isListing)];

    public static List<string> GetUris() => [.. Queries.GetUris()];

    private static int CountPages() => Queries.Count();

    private static IEnumerable<List<Page>> GetPageBatches(
        int batchSize = GET_ALL_PAGES_BATCH_SIZE)
    {
        foreach (IReadOnlyList<Page> batch in Queries.GetBatches(batchSize))
        {
            yield return [.. batch];
        }
    }
}
