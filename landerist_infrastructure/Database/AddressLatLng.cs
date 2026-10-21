using System.Data;

namespace landerist_library.Database;

public sealed class AddressLatLng
{
    private const string TableName = "[ADDRESS_LAT_LNG]";
    private readonly IDatabase _database;

    public AddressLatLng(IDatabase database)
    {
        ArgumentNullException.ThrowIfNull(database);
        _database = database;
    }

    public bool Insert(string address, string region, double lat, double lng, bool isAccurate)
    {
        const string query =
            "INSERT INTO " + TableName + " " +
            "([DateInsert], [Address], [Region], [Lat], [Lng], [IsAccurate]) " +
            "VALUES (GETDATE(), @Address, @Region, @Lat, @Lng, @IsAccurate)";
        return _database.Query(query, new Dictionary<string, object?>
        {
            { "Address", address },
            { "Region", region },
            { "Lat", lat },
            { "Lng", lng },
            { "IsAccurate", isAccurate }
        });
    }

    public (double lat, double lng, bool isAccurate)? Select(string address, string region)
    {
        const string query =
            "SELECT Lat, Lng, IsAccurate " +
            "FROM " + TableName + " " +
            "WHERE Address = @Address AND Region = @Region";
        DataTable rows = _database.QueryTable(query, new Dictionary<string, object?>
        {
            { "Address", address },
            { "Region", region }
        });
        if (rows.Rows.Count == 0)
        {
            return null;
        }
        return (
            (double)rows.Rows[0]["Lat"],
            (double)rows.Rows[0]["Lng"],
            (bool)rows.Rows[0]["IsAccurate"]);
    }

    public bool IsNotFound(string address, string region) => _database.QueryExists(
        "SELECT 1 FROM [ADDRESS_LAT_LNG_NOT_FOUND] WHERE Address = @Address AND Region = @Region " +
        "AND ExpiresAt > GETUTCDATE()",
        new Dictionary<string, object?> { { "Address", address }, { "Region", region } });

    public bool InsertNotFound(string address, string region, int lifetimeDays) => _database.Query(
        "SET XACT_ABORT ON; BEGIN TRANSACTION; UPDATE [ADDRESS_LAT_LNG_NOT_FOUND] WITH (UPDLOCK, SERIALIZABLE) " +
        "SET ExpiresAt = DATEADD(DAY, @Days, GETUTCDATE()) WHERE Address = @Address AND Region = @Region; " +
        "IF @@ROWCOUNT = 0 INSERT INTO [ADDRESS_LAT_LNG_NOT_FOUND] (Address, Region, ExpiresAt) " +
        "VALUES (@Address, @Region, DATEADD(DAY, @Days, GETUTCDATE())); COMMIT;",
        new Dictionary<string, object?> { { "Address", address }, { "Region", region }, { "Days", lifetimeDays } });

    public bool Clean()
    {
        bool negativesCleaned = _database.Query("DELETE FROM [ADDRESS_LAT_LNG_NOT_FOUND] WHERE ExpiresAt <= GETUTCDATE()");
        return _database.Query(
        "DELETE FROM " + TableName + " " +
        "WHERE [DateInsert] < DATEADD(YEAR, -1, GETDATE())") && negativesCleaned;
    }
}