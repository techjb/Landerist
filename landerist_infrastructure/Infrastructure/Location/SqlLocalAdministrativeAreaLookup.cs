using System.Data;
using landerist_library.Application.Parsing;
using landerist_library.Database;
using landerist_library.Websites;

namespace landerist_library.Infrastructure.Location;

public sealed class SqlLocalAdministrativeAreaLookup(
    IDatabase database) : ILocalAdministrativeAreaLookup
{
    public LocalAdministrativeArea? Find(
        CountryCode countryCode,
        double latitude,
        double longitude)
    {
        if (!IsValidCoordinate(latitude, longitude))
        {
            return null;
        }

        return countryCode == CountryCode.ES
            ? FindSpanishMunicipality(latitude, longitude)
            : FindLau(latitude, longitude);
    }

    private LocalAdministrativeArea? FindSpanishMunicipality(
        double latitude,
        double longitude)
    {
        DataRow? row = FindContaining(
            "[CNIG]",
            "natcode AS AreaId, nameunit AS AreaName",
            latitude,
            longitude);
        if (row is null)
        {
            return null;
        }

        string id = Read(row, "AreaId");
        string name = Read(row, "AreaName");
        if (id.Length != 11 || string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        return new LocalAdministrativeArea(id[6..], name);
    }

    private LocalAdministrativeArea? FindLau(
        double latitude,
        double longitude)
    {
        DataRow? row = FindContaining(
            "[LAU]",
            "lau_id AS AreaId, lau_name AS AreaName",
            latitude,
            longitude);
        if (row is null)
        {
            return null;
        }

        string id = Read(row, "AreaId");
        string name = Read(row, "AreaName");
        return string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(name)
            ? null
            : new LocalAdministrativeArea(id, name);
    }

    private DataRow? FindContaining(
        string table,
        string columns,
        double latitude,
        double longitude)
    {
        DataTable rows = database.QueryTable(
            $"""
            SELECT TOP 1 {columns}
            FROM {table}
            WHERE [the_geom].STIntersects(
                geography::Point(@latitude, @longitude, 4326)) = 1
            """,
            new Dictionary<string, object?>
            {
                ["latitude"] = latitude,
                ["longitude"] = longitude,
            });

        return rows.Rows.Count == 0 ? null : rows.Rows[0];
    }

    private static string Read(DataRow row, string column) =>
        row[column].ToString()?.Trim() ?? string.Empty;

    private static bool IsValidCoordinate(double latitude, double longitude) =>
        double.IsFinite(latitude)
        && double.IsFinite(longitude)
        && latitude is >= -90 and <= 90
        && longitude is >= -180 and <= 180;
}
