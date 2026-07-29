using landerist_library.Database;
using landerist_library.Pages;
using landerist_orels.ES;

namespace landerist_library.Infrastructure.Sql;

public sealed class PageStatisticsRepository
{
    private readonly IDatabase _database;
    public PageStatisticsRepository(IDatabase database)
    {
        ArgumentNullException.ThrowIfNull(database);
        _database = database;
    }

    private IDatabase Database => _database;

    public Dictionary<string, object?> GroupByPageType(ListingStatus? listingStatus = null)
    {
        string where = listingStatus is null
            ? string.Empty
            : "WHERE L.[listingStatus] = @ListingStatus ";
        string query =
            "SELECT P.[PageType], COUNT(*) " +
            "FROM " + Pages.Pages.PAGES + " AS P " +
            "LEFT JOIN " + SqlTableNames.Listings + " AS L ON L.[guid] = P.[UriHash] " +
            where +
            "GROUP BY P.[PageType] " +
            "ORDER BY COUNT(*) DESC";

        return Database.QueryDictionary(query, GetListingStatusParameters(listingStatus));
    }

    public Dictionary<string, object?> GroupByHttpStatusCode(ListingStatus? listingStatus = null)
    {
        string where = listingStatus is null
            ? string.Empty
            : "WHERE L.[listingStatus] = @ListingStatus ";
        string query =
            "SELECT CONVERT(VARCHAR, P.[HttpStatusCode], 23), COUNT(*) " +
            "FROM " + Pages.Pages.PAGES + " AS P " +
            "LEFT JOIN " + SqlTableNames.Listings + " AS L ON L.[guid] = P.[UriHash] " +
            where +
            "GROUP BY CONVERT(VARCHAR, P.[HttpStatusCode], 23) " +
            "ORDER BY COUNT(*) DESC";

        return Database.QueryDictionary(query, GetListingStatusParameters(listingStatus));
    }

    public Dictionary<string, object?> GroupByNextScrape()
    {
        const string query =
            "SELECT CONVERT(VARCHAR, [NextScrape], 23) AS [DateWhithoutTime], COUNT(*) AS [Total] " +
            "FROM " + Pages.Pages.PAGES + " " +
            "GROUP BY CONVERT(VARCHAR, [NextScrape], 23) " +
            "ORDER BY [DateWhithoutTime] ASC";

        return Database.QueryDictionary(query);
    }

    public Dictionary<string, object?> CountByHttpStatusCode()
    {
        const string query =
            "SELECT CAST([HttpStatusCode] AS VARCHAR), COUNT(*) " +
            "FROM " + Pages.Pages.PAGES + " " +
            "GROUP BY [HttpStatusCode] " +
            "ORDER BY COUNT(*) DESC";

        return Database.QueryDictionary(query);
    }

    private static Dictionary<string, object?>? GetListingStatusParameters(ListingStatus? listingStatus)
    {
        return listingStatus is null
            ? null
            : new Dictionary<string, object?> { ["ListingStatus"] = listingStatus.ToString() };
    }
}
