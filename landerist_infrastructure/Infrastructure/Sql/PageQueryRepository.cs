using landerist_library.Database;
using landerist_library.Pages;
using System.Data;

namespace landerist_library.Infrastructure.Sql;

public class PageQueryRepository
{
    private readonly PageLookupQueries _lookups;
    private readonly PageScrapingQueries _scraping;
    private readonly PageListingQueries _listings;

    public PageQueryRepository(IDatabase database)
        : this(database, PageQueryOptions.Default)
    {
    }

    public PageQueryRepository(IDatabase database, PageQueryOptions options)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(options);
        _lookups = new PageLookupQueries(database);
        _scraping = new PageScrapingQueries(database, options);
        _listings = new PageListingQueries(database);
    }

    public DataTable GetPagesByHost(string host) =>
        _lookups.GetPagesByHost(host);

    public DataTable GetScrapePages(int topRows) =>
        _scraping.GetScrapePages(topRows);

    public DataTable GetPagesForUpdate(int topRows, string where) =>
        _scraping.GetPagesForUpdate(topRows, where);

    public DataTable GetNonScrapedPages(string host) =>
        _lookups.GetNonScrapedPages(host);

    public DataTable GetUnknownPageType(string host) =>
        _lookups.GetUnknownPageType(host);

    public List<string> GetUris(bool isListing) =>
        _lookups.GetUris(isListing);

    public List<string> GetUris() => _lookups.GetUris();

    public DataTable GetHostPagesDataTable(string host) =>
        _lookups.GetHostPagesDataTable(host);

    public int CountPages() => _lookups.CountPages();

    public DataTable GetPagesBatch(string? lastUriHash, int batchSize) =>
        _lookups.GetPagesBatch(lastUriHash, batchSize);

    public DataTable QueryPages(string query) =>
        _lookups.QueryPages(query);

    public DataTable QueryPages(
        string query,
        Dictionary<string, object?> parameters) =>
        _lookups.QueryPages(query, parameters);

    public DataTable GetPageByUriHash(string uriHash) =>
        _lookups.GetPageByUriHash(uriHash);

    public DataTable GetPagesByPageType(PageType pageType) =>
        _lookups.GetPagesByPageType(pageType);

    public DataTable GetUnknownPageType() =>
        _lookups.GetUnknownPageType();

    public DataTable GetUnknownPageTypeForUpdate(int topRows) =>
        _scraping.GetUnknownPageTypeForUpdate(topRows);

    public DataTable GetNextScrapeForUpdate(
        int topRows,
        bool extendToFillTopRows) =>
        _scraping.GetNextScrapeForUpdate(topRows, extendToFillTopRows);

    public DataTable GetNextScrapeFutureForUpdate(int topRows) =>
        _scraping.GetNextScrapeFutureForUpdate(topRows);

    public DataTable GetRecentlyUnpublishedListingsPages(int topRows) =>
        _scraping.GetRecentlyUnpublishedListingsPages(topRows);

    public DataTable GetUnknownHttpStatusCode() =>
        _lookups.GetUnknownHttpStatusCode();

    public DataTable GetAllUrisDataTable() =>
        _lookups.GetAllUrisDataTable();

    public DataTable GetListingsWithHttpStatusCodeError() =>
        _listings.GetListingsWithHttpStatusCodeError();

    public DataTable GetListingsWithParserInputHash() =>
        _listings.GetListingsWithParserInputHash();

    public DataTable GetUrisLikePrint() =>
        _lookups.GetUrisLikePrint();

    public DataTable GetPagesWithProhibitedUris(
        IEnumerable<string> prohibitedUriFragments) =>
        _lookups.GetPagesWithProhibitedUris(prohibitedUriFragments);

    public string SelectQuery(int? topRows = null) =>
        PageSqlQueryBuilder.Select(topRows);

    public static string SelectColumns(string pagesTableName = "") =>
        PageSqlQueryBuilder.SelectColumns(pagesTableName);
}
