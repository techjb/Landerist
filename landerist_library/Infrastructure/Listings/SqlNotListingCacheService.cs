using landerist_library.Application.Listings;
using landerist_library.Database;
using landerist_library.Pages;

namespace landerist_library.Infrastructure.Listings;

public sealed class SqlNotListingCacheService : INotListingCacheService, INotListingCacheMaintenance
{
    private const string TableName = "NOT_LISTINGS_CACHE";
    private readonly IDatabase _database;
    private readonly bool _enabled;

    public SqlNotListingCacheService(IDatabase database, bool enabled)
    {
        ArgumentNullException.ThrowIfNull(database);
        _database = database;
        _enabled = enabled;
    }

    public bool Insert(Page page)
    {
        ArgumentNullException.ThrowIfNull(page);
        if (!_enabled || string.IsNullOrEmpty(page.ListingParserInputHash))
        {
            return false;
        }

        const string query =
            "INSERT INTO " + TableName + " " +
            "([Inserted], [Host], [ListingParserInputHash]) " +
            "VALUES (GETDATE(), @Host, @ListingParserInputHash)";
        return _database.Query(query, new Dictionary<string, object?>
        {
            { "Host", page.Host },
            { "ListingParserInputHash", page.ListingParserInputHash }
        });
    }

    public bool Contains(Page page)
    {
        ArgumentNullException.ThrowIfNull(page);
        if (!_enabled || string.IsNullOrEmpty(page.ListingParserInputHash))
        {
            return false;
        }

        const string query =
            "SELECT 1 FROM " + TableName + " " +
            "WHERE [Host] = @Host " +
            "AND [ListingParserInputHash] = @ListingParserInputHash";
        return _database.QueryExists(query, new Dictionary<string, object?>
        {
            { "Host", page.Host },
            { "ListingParserInputHash", page.ListingParserInputHash }
        });
    }

    public bool Clean() => _database.Query(
        "DELETE FROM " + TableName + " " +
        "WHERE [Inserted] < DATEADD(DAY, -30, GETDATE())");
}