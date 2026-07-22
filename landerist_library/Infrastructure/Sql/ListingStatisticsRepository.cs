using landerist_library.Database;
using landerist_orels;
using landerist_orels.ES;

namespace landerist_library.Infrastructure.Sql;

public sealed class ListingStatisticsRepository
{
    private readonly IDatabase? _database;

    public ListingStatisticsRepository()
    {
    }

    public ListingStatisticsRepository(IDatabase database)
    {
        ArgumentNullException.ThrowIfNull(database);
        _database = database;
    }

    private IDatabase Database => _database ?? new DataBase();

    public int Count(string host)
    {
        string query =
            "SELECT COUNT(*) " +
            "FROM " + ES_Listings.TABLE_ES_LISTINGS + " " +
            "WHERE [host] = @Host";

        return Database.QueryInt(query, new Dictionary<string, object?> { { "Host", host } });
    }

    public int Count(string host, ListingStatus listingStatus)
    {
        string query =
            "SELECT COUNT(*) " +
            "FROM " + ES_Listings.TABLE_ES_LISTINGS + " " +
            "WHERE [host] = @Host AND [listingStatus] = @ListingStatus";

        return Database.QueryInt(query, new Dictionary<string, object?>
            {
                { "Host", host },
                { "ListingStatus", listingStatus.ToString() }
            });
    }

    public int CountSinceListingDate(string host, DateTime listingDateFrom)
    {
        string query =
            "SELECT COUNT(*) " +
            "FROM " + ES_Listings.TABLE_ES_LISTINGS + " " +
            "WHERE [host] = @Host " +
            "AND [listingDate] >= @ListingDateFrom";

        return Database.QueryInt(query, new Dictionary<string, object?>
            {
                { "Host", host },
                { "ListingDateFrom", listingDateFrom }
            });
    }

    public int CountWithAddress(string host, ListingStatus listingStatus)
    {
        string query =
            "SELECT COUNT(*) " +
            "FROM " + ES_Listings.TABLE_ES_LISTINGS + " " +
            "WHERE [host] = @Host " +
            "AND [listingStatus] = @ListingStatus " +
            "AND NULLIF(LTRIM(RTRIM([address])), '') IS NOT NULL";

        return Database.QueryInt(query, new Dictionary<string, object?>
            {
                { "Host", host },
                { "ListingStatus", listingStatus.ToString() }
            });
    }

    public int CountWithCoordinates(string host, ListingStatus listingStatus)
    {
        string query =
            "SELECT COUNT(*) " +
            "FROM " + ES_Listings.TABLE_ES_LISTINGS + " " +
            "WHERE [host] = @Host " +
            "AND [listingStatus] = @ListingStatus " +
            "AND [latitude] IS NOT NULL " +
            "AND [longitude] IS NOT NULL";

        return Database.QueryInt(query, new Dictionary<string, object?>
            {
                { "Host", host },
                { "ListingStatus", listingStatus.ToString() }
            });
    }

    public int CountWithImages(string host, ListingStatus listingStatus)
    {
        string query =
            "SELECT COUNT(*) " +
            "FROM " + ES_Listings.TABLE_ES_LISTINGS + " AS L " +
            "WHERE L.[host] = @Host " +
            "AND L.[listingStatus] = @ListingStatus " +
            "AND EXISTS (" +
            "   SELECT 1 " +
            "   FROM " + ES_Media.TABLE_ES_MEDIA + " AS M " +
            "   WHERE M.[listingGuid] = L.[guid] " +
            "   AND M.[mediaType] = @MediaType" +
            ")";

        return Database.QueryInt(query, new Dictionary<string, object?>
            {
                { "Host", host },
                { "ListingStatus", listingStatus.ToString() },
                { "MediaType", MediaType.image.ToString() }
            });
    }

    public int Count(ListingStatus listingStatus, Operation operation, PropertyType propertyType)
    {
        string query =
            "SELECT COUNT(*) " +
            "FROM " + ES_Listings.TABLE_ES_LISTINGS + " AS L " +
            "WHERE L.[listingStatus] = @ListingStatus AND " +
            "L.[operation] = @Operation AND " +
            "L.[propertyType] = @PropertyType";

        return Database.QueryInt(query, new Dictionary<string, object?>
            {
                { "ListingStatus", listingStatus.ToString() },
                { "Operation", operation.ToString() },
                { "PropertyType", propertyType.ToString() }
            });
    }
}
