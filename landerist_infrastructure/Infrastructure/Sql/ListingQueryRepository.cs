using landerist_library.Database;
using landerist_orels;
using landerist_orels.ES;
using System.Data;

namespace landerist_library.Infrastructure.Sql;

public sealed class ListingQueryRepository
{
    private readonly IDatabase _database;
    public ListingQueryRepository(IDatabase database)
    {
        ArgumentNullException.ThrowIfNull(database);
        _database = database;
    }

    private IDatabase Database => _database;

    public DataTable GetAll()
    {
        string query = "SELECT * FROM " + SqlTableNames.Listings;
        return Database.QueryTable(query);
    }

    public DataTable GetListings(ListingStatus listingStatus)
    {
        string query =
            "SELECT * " +
            "FROM " + SqlTableNames.Listings + " " +
            "WHERE [listingStatus] = @listingStatus";

        return Database.QueryTable(query, new Dictionary<string, object?> {
                {"listingStatus", listingStatus.ToString() },
            });
    }

    public DataTable GetListings(ListingStatus listingStatus, Operation operation, PropertyType propertyType)
    {
        string query =
            "SELECT L.* " +
            "FROM " + SqlTableNames.Listings + " AS L " +
            "WHERE L.[listingStatus] = @listingStatus AND " +
            "L.[operation] = @operation AND " +
            "L.[propertyType] = @propertyType";

        return Database.QueryTable(query, new Dictionary<string, object?> {
                {"listingStatus", listingStatus.ToString() },
                {"operation", operation.ToString() },
                {"propertyType", propertyType.ToString() },
            });
    }

    public DataTable GetListings(string host, ListingStatus? listingStatus = null)
    {
        string query =
            "SELECT * " +
            "FROM " + SqlTableNames.Listings + " " +
            "WHERE [Host] = @Host " +
            (listingStatus is null ? string.Empty : "AND [ListingStatus] = @ListingStatus");

        return Database.QueryTable(query, new Dictionary<string, object?>
            {
                { "Host", host },
                { "ListingStatus", listingStatus?.ToString() }
            });
    }

    public DataTable GetListingWithCatastralReference()
    {
        return Database.QueryTable("SELECT * FROM " + SqlTableNames.Listings + " WHERE [cadastralReference] IS NOT NULL");
    }

    public DataTable GetListingsWithoutLauName()
    {
        string query =
            "SELECT * " +
            "FROM " + SqlTableNames.Listings + " " +
            "WHERE [lauName] IS NULL AND " +
            "[latitude] IS NOT NULL AND " +
            "[longitude] IS NOT NULL";

        return Database.QueryTable(query);
    }

    public DataTable GetListingWithCatastralReferenceAndNoAddress()
    {
        string query =
            "SELECT * " +
            "FROM " + SqlTableNames.Listings + " " +
            "WHERE [cadastralReference] IS NOT NULL " +
            "AND [address] IS NULL";

        return Database.QueryTable(query);
    }

    public DataTable GetListingsWithoutCatastralReferenceAndLocationIsAccurate()
    {
        string query =
            "SELECT * " +
            "FROM " + SqlTableNames.Listings + " " +
            "WHERE [cadastralReference] IS NULL " +
            "AND [locationIsAccurate] = 1";

        return Database.QueryTable(query);
    }

    public DataTable GetListingsLocationIsAccurateNoCadastralReference()
    {
        string query =
            "SELECT * " +
            "FROM " + SqlTableNames.Listings + " " +
            "WHERE [locationIsAccurate] = 1 AND " +
            " [cadastralReference] IS NULL";

        return Database.QueryTable(query);
    }

    public DataTable GetUnpublishedListings(DateTime unlistingDate)
    {
        string query =
            "SELECT * " +
            "FROM " + SqlTableNames.Listings + " " +
            "WHERE [listingStatus] = @listingStatus AND " +
            "[unlistingDate] < @unlistingDate";

        return Database.QueryTable(query, new Dictionary<string, object?> {
                {"listingStatus", ListingStatus.unpublished.ToString() },
                {"unlistingDate", unlistingDate }
            });
    }

    public DataTable GetListings(DateOnly dateFrom, DateOnly dateTo)
    {
        string query =
            "SELECT * " +
            "FROM " + SqlTableNames.Listings + " " +
            "WHERE " +
            "   CAST([updated] AS DATE) >= CAST(@DateFrom AS DATE) AND " +
            "   CAST([updated] AS DATE) <= CAST(@DateTo AS DATE)";

        return Database.QueryTable(query, new Dictionary<string, object?>()
            {
                { "DateFrom", dateFrom },
                { "DateTo", dateTo },
            });
    }

    public DataTable GetListings(ListingStatus listingStatus, DateOnly dateFrom, DateOnly dateTo)
    {
        string query =
            "SELECT * " +
            "FROM " + SqlTableNames.Listings + " " +
            "WHERE " +
            "   [listingStatus] = @ListingStatus AND " +
            "   CAST([updated] AS DATE) >= CAST(@DateFrom AS DATE) AND " +
            "   CAST([updated] AS DATE) <= CAST(@DateTo AS DATE)";

        return Database.QueryTable(query, new Dictionary<string, object?>()
            {
                { "ListingStatus", listingStatus.ToString() },
                { "DateFrom", dateFrom },
                { "DateTo", dateTo },
            });
    }

    public Task<DataTable> GetListingAsync(
        string guid,
        CancellationToken cancellationToken = default)
    {
        const string query =
            "SELECT * " +
            "FROM " + SqlTableNames.Listings + " " +
            "WHERE [Guid] = @Guid";
        return Database.QueryTableAsync(
            query,
            new Dictionary<string, object?> { ["Guid"] = guid },
            cancellationToken);
    }
    public DataTable GetListing(string guid)
    {
        string query =
            "SELECT * " +
            "FROM " + SqlTableNames.Listings + " " +
            "WHERE [Guid] = @Guid";

        return Database.QueryTable(query, new Dictionary<string, object?> {
                {"Guid", guid }
            });
    }
}
