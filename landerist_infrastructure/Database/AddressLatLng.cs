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

    public bool Clean() => _database.Query(
        "DELETE FROM " + TableName + " " +
        "WHERE [DateInsert] < DATEADD(YEAR, -1, GETDATE())");
}