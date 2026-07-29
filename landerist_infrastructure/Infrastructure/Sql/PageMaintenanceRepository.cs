using landerist_library.Database;
using landerist_library.Pages;
using System.Data;

namespace landerist_library.Infrastructure.Sql;

public sealed class PageMaintenanceRepository
{
    private readonly IDatabase _database;
    public PageMaintenanceRepository(IDatabase database)
    {
        ArgumentNullException.ThrowIfNull(database);
        _database = database;
    }

    private IDatabase Database => _database;

    public DataTable SelectWaitingStatus(
        int topRows,
        WaitingStatus waitingStatusFrom,
        WaitingStatus waitingStatusTo,
        int tokenCount,
        bool isMaxTokenCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(topRows);
        ArgumentOutOfRangeException.ThrowIfNegative(tokenCount);

        string comparisonOperator = isMaxTokenCount ? "<=" : ">";
        string query =
            "BEGIN TRANSACTION; " +
            "WITH PagesToUpdate AS ( " +
            "SELECT TOP (@TopRows) " + Pages.Pages.PAGES + ".[UriHash] " +
            "FROM " + Pages.Pages.PAGES + " " +
            "INNER JOIN " + Websites.Websites.WEBSITES + " ON " + Pages.Pages.PAGES + ".[Host] = " + Websites.Websites.WEBSITES + ".[Host] " +
            "WHERE " + Pages.Pages.PAGES + ".[WaitingStatus] = @WaitingStatusFrom AND [TokenCount] " + comparisonOperator + " @TokenCount " +
            "ORDER BY " + Pages.Pages.PAGES + ".[LastScrape] ASC ) " +
            "UPDATE " + Pages.Pages.PAGES + " " +
            "SET [WaitingStatus] = @WaitingStatusTo " +
            "OUTPUT " + PageQueryRepository.SelectColumns("INSERTED") + " " +
            "FROM " + Pages.Pages.PAGES + " " +
            "INNER JOIN PagesToUpdate ON " + Pages.Pages.PAGES + ".[UriHash] = PagesToUpdate.[UriHash] " +
            "INNER JOIN " + Websites.Websites.WEBSITES + " ON " + Pages.Pages.PAGES + ".[Host] = " + Websites.Websites.WEBSITES + ".[Host] " +
            "COMMIT TRANSACTION;";

        return Database.QueryTable(query, new Dictionary<string, object?>
        {
            ["TopRows"] = topRows,
            ["WaitingStatusFrom"] = waitingStatusFrom.ToString(),
            ["WaitingStatusTo"] = waitingStatusTo.ToString(),
            ["TokenCount"] = tokenCount
        });
    }

    public bool UpdateWaitingStatus(string uriHash, WaitingStatus waitingStatus)
    {
        const string query =
            "UPDATE " + Pages.Pages.PAGES + " " +
            "SET [WaitingStatus] = @WaitingStatus " +
            "WHERE [UriHash] = @UriHash";

        return Database.Query(query, new Dictionary<string, object?>
        {
            ["WaitingStatus"] = waitingStatus.ToString(),
            ["UriHash"] = uriHash
        });
    }

    public bool UpdateWaitingStatus(WaitingStatus waitingStatusFrom, WaitingStatus waitingStatusTo)
    {
        const string query =
            "UPDATE " + Pages.Pages.PAGES + " " +
            "SET [WaitingStatus] = @WaitingStatusTo " +
            "WHERE [WaitingStatus] = @WaitingStatusFrom";

        return Database.Query(query, new Dictionary<string, object?>
        {
            ["WaitingStatusFrom"] = waitingStatusFrom.ToString(),
            ["WaitingStatusTo"] = waitingStatusTo.ToString()
        });
    }

    public bool CleanLockedBy(string lockedBy)
    {
        const string query =
            "UPDATE " + Pages.Pages.PAGES + " " +
            "SET [LockedBy] = NULL " +
            "WHERE [LockedBy] = @LockedBy";

        return Database.Query(query, new Dictionary<string, object?>
        {
            ["LockedBy"] = lockedBy
        });
    }

    public Task<bool> CleanLockedByAsync(
        string lockedBy,
        CancellationToken cancellationToken = default)
    {
        const string query =
            "UPDATE " + Pages.Pages.PAGES + " " +
            "SET [LockedBy] = NULL " +
            "WHERE [LockedBy] = @LockedBy";

        return Database.QueryAsync(
            query,
            new Dictionary<string, object?> { ["LockedBy"] = lockedBy },
            cancellationToken);
    }
    public bool DeleteByHost(string host)
    {
        const string query =
            "DELETE FROM " + Pages.Pages.PAGES + " " +
            "WHERE [Host] = @Host";

        return Database.Query(query, new Dictionary<string, object?>
        {
            ["Host"] = host
        });
    }

    public bool DeleteAll()
    {
        const string query = "DELETE FROM " + Pages.Pages.PAGES;
        return Database.Query(query);
    }

    public bool RemoveListingParserInputHash(PageType? pageType = null)
    {
        string where = pageType is null ? string.Empty : " WHERE [PageType] = @PageType";
        string query =
            "UPDATE " + Pages.Pages.PAGES + " " +
            "SET [ListingParserInputHash] = NULL" +
            where;

        Dictionary<string, object?>? parameters = pageType is null
            ? null
            : new Dictionary<string, object?> { ["PageType"] = pageType.ToString() };

        return Database.Query(query, parameters);
    }
}
