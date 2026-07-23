using landerist_library.Application.Listings;
using landerist_library.Database;
using landerist_library.Pages;
using landerist_library.Parse.CadastralReference;
using landerist_library.Parse.Location;
using landerist_library.Parse.Location.Providers.GoogleMaps;
using landerist_library.Websites;
using landerist_orels.ES;
using System.Data;
using System.Globalization;

namespace landerist_library.Infrastructure.Listings;

public sealed class SqlListingEnricher : IListingEnricher
{
    private readonly IDatabase _database;

    public SqlListingEnricher(IDatabase database)
    {
        ArgumentNullException.ThrowIfNull(database);
        _database = database;
    }

    public void Enrich(Page page, Listing listing)
    {
        new LocationParser(
            page,
            listing,
            new GoogleMapsApi(_database),
            new AddressToCadastralReference(_database)).SetLocation();
        SetAdministrativeArea(page.Website.CountryCode, listing);
    }

    private void SetAdministrativeArea(CountryCode countryCode, Listing listing)
    {
        if (listing.latitude is not double latitude ||
            listing.longitude is not double longitude ||
            !IsValidCoordinate(latitude, longitude))
        {
            return;
        }

        string point = string.Create(
            CultureInfo.InvariantCulture,
            $"POINT({longitude} {latitude})");
        string columns = countryCode == CountryCode.ES
            ? "natcode AS AreaId, nameunit AS AreaName"
            : "lau_id AS AreaId, lau_name AS AreaName";
        string table = countryCode == CountryCode.ES ? "[CNIG]" : "[LAU]";
        string query =
            "SELECT TOP 1 " + columns + " " +
            "FROM " + table + " WITH(INDEX([SpatialIndex-the_geom])) " +
            "WHERE [the_geom].STIntersects(" +
            "geography::STGeomFromText(@Point, 4326)) = 1";

        DataTable rows = _database.QueryTable(query, new Dictionary<string, object?>
        {
            { "Point", point }
        });
        if (rows.Rows.Count == 0)
        {
            return;
        }

        string areaId = rows.Rows[0]["AreaId"].ToString()?.Trim() ?? string.Empty;
        string areaName = rows.Rows[0]["AreaName"].ToString()?.Trim() ?? string.Empty;
        if (countryCode == CountryCode.ES)
        {
            if (areaId.Length != 11 || string.IsNullOrWhiteSpace(areaName))
            {
                return;
            }
            areaId = areaId[6..];
        }
        else if (string.IsNullOrWhiteSpace(areaId) || string.IsNullOrWhiteSpace(areaName))
        {
            return;
        }

        listing.lauId = areaId;
        listing.lauName = areaName;
    }

    private static bool IsValidCoordinate(double latitude, double longitude) =>
        double.IsFinite(latitude) &&
        double.IsFinite(longitude) &&
        latitude is >= -90 and <= 90 &&
        longitude is >= -180 and <= 180;
}
