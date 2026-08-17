using landerist_library.Database;
using landerist_library.Pages;
using landerist_library.Websites;
using System.Data;
using PageTable = landerist_library.Pages.Pages;
using WebsiteTable = landerist_library.Websites.Websites;

namespace landerist_library.Infrastructure.Sql;

internal sealed class PageScrapingQueries
{
    private readonly IDatabase _database;
    private readonly PageQueryOptions _options;

    public PageScrapingQueries(IDatabase database, PageQueryOptions options)
    {
        _database = database;
        _options = options;
    }

    public DataTable GetScrapePages(int topRows)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(topRows);
        string query =
            "WITH CandidatePages AS (" +
            "   SELECT P.[UriHash], P.[NextScrape], " +
            "       CASE WHEN P.[PageType] IS NULL THEN 0 ELSE 1 END AS SelectionPriority, " +
            "       ROW_NUMBER() OVER (PARTITION BY P.[Host] " +
            "       ORDER BY CASE WHEN P.[PageType] IS NULL THEN 0 ELSE 1 END ASC, P.[NextScrape] ASC, P.[UriHash] ASC) AS HostPageRank " +
            "   FROM " + PageTable.PAGES + " AS P " +
            "   INNER JOIN " + WebsiteTable.WEBSITES + " AS W ON P.[Host] = W.[Host] " +
            "   WHERE P.[LockedBy] IS NULL AND P.[WaitingStatus] IS NULL " +
            "   AND (P.[PageType] IS NULL OR P.[NextScrape] < GETDATE()) " +
            "   AND NOT EXISTS (SELECT 1 FROM " + SqlTableNames.WebsiteThrottle +
            " AS WB WHERE WB.[Host] = P.[Host] AND WB.[BlockUntil] > GETDATE())" +
            "), TopPages AS (" +
            "   SELECT TOP " + topRows + " [UriHash] FROM CandidatePages " +
            "   WHERE HostPageRank <= @MaxPagesPerHost " +
            "   ORDER BY SelectionPriority ASC, [NextScrape] ASC, [UriHash] ASC" +
            ") UPDATE P SET LockedBy = @LockedBy " +
            PageSqlQueryBuilder.OutputColumns("INSERTED") +
            "FROM " + PageTable.PAGES + " AS P " +
            "INNER JOIN " + WebsiteTable.WEBSITES + " AS W ON P.[Host] = W.[Host] " +
            "INNER JOIN TopPages AS TP ON P.[UriHash] = TP.[UriHash]";

        return _database.QueryTable(query, new Dictionary<string, object?>
        {
            ["LockedBy"] = _options.LockedBy,
            ["MaxPagesPerHost"] = _options.MaxPagesPerHost
        });
    }

    public DataTable GetPagesForUpdate(int topRows, string where)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(topRows);
        string query =
            "WITH TopPages AS (" +
            "   SELECT TOP " + topRows + " P.[UriHash] " +
            "   FROM " + PageTable.PAGES + " AS P " +
            "   INNER JOIN " + WebsiteTable.WEBSITES + " AS W ON P.[Host] = W.[Host] " +
            "   WHERE P.[LockedBy] IS NULL AND P.[WaitingStatus] IS NULL " +
            "   AND NOT EXISTS (SELECT 1 FROM " + SqlTableNames.WebsiteThrottle +
            " AS WB WHERE WB.[Host] = P.[Host] AND WB.[BlockUntil] > GETDATE()) " +
            (string.IsNullOrEmpty(where) ? string.Empty : " AND " + where) + " " +
            "   ORDER BY P.[NextScrape] ASC" +
            ") UPDATE P SET LockedBy = @LockedBy " +
            PageSqlQueryBuilder.OutputColumns("INSERTED") +
            "FROM " + PageTable.PAGES + " AS P " +
            "INNER JOIN " + WebsiteTable.WEBSITES + " AS W ON P.[Host] = W.[Host] " +
            "INNER JOIN TopPages AS TP ON P.[UriHash] = TP.[UriHash]";

        return _database.QueryTable(query, new Dictionary<string, object?>
        {
            ["LockedBy"] = _options.LockedBy
        });
    }

    public DataTable GetUnknownPageTypeForUpdate(int topRows) =>
        GetPagesForUpdate(topRows, "P.[PageType] IS NULL");

    public DataTable GetNextScrapeForUpdate(int topRows, bool extendToFillTopRows) =>
        GetPagesForUpdate(
            topRows,
            extendToFillTopRows ? string.Empty : "P.[NextScrape] < GETDATE()");

    public DataTable GetNextScrapeFutureForUpdate(int topRows) =>
        GetPagesForUpdate(topRows, "P.[NextScrape] >= GETDATE()");

    public DataTable GetRecentlyUnpublishedListingsPages(int topRows) =>
        GetPagesForUpdate(
            topRows,
            "P.[UriHash] IN (SELECT [Guid] FROM " + SqlTableNames.Listings +
            " WHERE [ListingStatus] = 'unpublished' " +
            "AND [UnlistingDate] > DATEADD(day, -2, GETDATE()))");
}
