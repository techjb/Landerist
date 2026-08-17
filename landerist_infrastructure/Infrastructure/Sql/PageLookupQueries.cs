using landerist_library.Database;
using landerist_library.Pages;
using System.Data;
using PageTable = landerist_library.Pages.Pages;

namespace landerist_library.Infrastructure.Sql;

internal sealed class PageLookupQueries
{
    private readonly IDatabase _database;

    public PageLookupQueries(IDatabase database) => _database = database;

    public DataTable GetPagesByHost(string host) =>
        _database.QueryTable(
            "SELECT * FROM " + PageTable.PAGES + " WHERE [Host] = @Host",
            new Dictionary<string, object?> { ["Host"] = host });

    public DataTable GetNonScrapedPages(string host) =>
        _database.QueryTable(
            "SELECT * FROM " + PageTable.PAGES +
            " WHERE [Host] = @Host AND [LastScrape] IS NULL",
            new Dictionary<string, object?> { ["Host"] = host });

    public DataTable GetUnknownPageType(string host) =>
        _database.QueryTable(
            "SELECT * FROM " + PageTable.PAGES +
            " WHERE [Host] = @Host AND [PageType] IS NULL",
            new Dictionary<string, object?> { ["Host"] = host });

    public List<string> GetUris(bool isListing) =>
        _database.QueryListString(
            "SELECT [Uri] FROM " + PageTable.PAGES + " WHERE IsListing = @IsListing",
            new Dictionary<string, object?> { ["IsListing"] = isListing });

    public List<string> GetUris() =>
        _database.QueryListString("SELECT [Uri] FROM " + PageTable.PAGES);

    public DataTable GetHostPagesDataTable(string host) =>
        _database.QueryTable(
            "SELECT [Host], [Uri], [UriHash], [Inserted], [LastScrape], " +
            "[LastParseListing], [NextScrape], [HttpStatusCode], [Etag], " +
            "[LastModified], [PageType], [PageTypeCounter], [LockedBy], " +
            "[WaitingStatus], [ListingParserInputNotChangedCounter], " +
            "[TransientErrorCounter], [TokenCount] FROM " + PageTable.PAGES +
            " WHERE [Host] = @Host ORDER BY [Uri]",
            new Dictionary<string, object?> { ["Host"] = host });

    public int CountPages() =>
        _database.QueryInt("SELECT COUNT(*) FROM " + PageTable.PAGES);

    public DataTable GetPagesBatch(string? lastUriHash, int batchSize)
    {
        string where = lastUriHash is null
            ? string.Empty
            : "WHERE " + PageTable.PAGES + ".[UriHash] > @LastUriHash ";
        string query = PageSqlQueryBuilder.Select(batchSize) + where +
            "ORDER BY " + PageTable.PAGES + ".[UriHash] ASC";
        Dictionary<string, object?> parameters = [];
        if (lastUriHash is not null)
        {
            parameters["LastUriHash"] = lastUriHash;
        }

        return QueryPages(query, parameters);
    }

    public DataTable QueryPages(string query) => QueryPages(query, []);

    public DataTable QueryPages(
        string query,
        Dictionary<string, object?> parameters) =>
        _database.QueryTable(query, parameters);

    public DataTable GetPageByUriHash(string uriHash) =>
        QueryPages(
            PageSqlQueryBuilder.Select() +
            "WHERE " + PageTable.PAGES + ".[UriHash] = @UriHash",
            new Dictionary<string, object?> { ["UriHash"] = uriHash });

    public DataTable GetPagesByPageType(PageType pageType) =>
        QueryPages(
            PageSqlQueryBuilder.Select() +
            "WHERE " + PageTable.PAGES + ".[PageType] = @PageType",
            new Dictionary<string, object?> { ["PageType"] = pageType.ToString() });

    public DataTable GetUnknownPageType() =>
        QueryPages(
            PageSqlQueryBuilder.Select() +
            "WHERE " + PageTable.PAGES + ".[PageType] IS NULL " +
            "AND " + PageTable.PAGES + ".[WaitingStatus] IS NULL");

    public DataTable GetUnknownHttpStatusCode() =>
        QueryPages(
            PageSqlQueryBuilder.Select() +
            "WHERE " + PageTable.PAGES + ".[HttpStatusCode] IS NULL");

    public DataTable GetAllUrisDataTable() =>
        _database.QueryTable("SELECT [Uri] FROM " + PageTable.PAGES);

    public DataTable GetUrisLikePrint() =>
        QueryPages(
            PageSqlQueryBuilder.Select() +
            "WHERE " + PageTable.PAGES + ".[Uri] LIKE '%print%' OR " +
            PageTable.PAGES + ".[Uri] LIKE '%imprimi%'");

    public DataTable GetPagesWithProhibitedUris(
        IEnumerable<string> prohibitedUriFragments)
    {
        ArgumentNullException.ThrowIfNull(prohibitedUriFragments);
        string[] fragments = prohibitedUriFragments
            .Where(fragment => !string.IsNullOrWhiteSpace(fragment))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (fragments.Length == 0)
        {
            return new DataTable();
        }

        Dictionary<string, object?> parameters = [];
        string[] conditions = new string[fragments.Length];
        for (int index = 0; index < fragments.Length; index++)
        {
            string parameterName = "UriFragment" + index;
            conditions[index] = PageTable.PAGES + ".[Uri] LIKE @" + parameterName;
            parameters[parameterName] = "%" + fragments[index] + "%";
        }

        return QueryPages(
            PageSqlQueryBuilder.Select() + "WHERE " + string.Join(" OR ", conditions),
            parameters);
    }
}
