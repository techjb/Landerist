using System.Globalization;
using landerist_library.Application.Parsing;
using landerist_library.Database;
using landerist_library.Websites;

namespace landerist_library.Infrastructure.Location.Validation;

public sealed class CountryCoordinateValidator : ICoordinateValidator
{
    private static readonly IReadOnlyDictionary<CountryCode, string> CountryIsoA3 =
        new Dictionary<CountryCode, string>
        {
            [CountryCode.ES] = "ESP",
        };

    private readonly IDatabase _database;
    private readonly CountryCode _countryCode;

    public CountryCoordinateValidator(IDatabase database, CountryCode countryCode)
    {
        ArgumentNullException.ThrowIfNull(database);
        _database = database;
        _countryCode = countryCode;
    }

    public bool Contains(double latitude, double longitude)
    {
        if (!IsValidCoordinate(latitude, longitude))
        {
            return false;
        }

        if (_countryCode == CountryCode.ES && ContainsSpain(latitude, longitude))
        {
            return true;
        }

        if (!CountryIsoA3.TryGetValue(_countryCode, out string? expectedIso3))
        {
            throw new NotSupportedException(
                $"Country code {_countryCode} does not have an ISO A3 mapping.");
        }

        string? actualIso3 = _database.QueryString(
            """
            SELECT TOP 1 [iso_a3]
            FROM [COUNTRIES]
            WHERE [the_geom].STIntersects(
                geography::Point(@latitude, @longitude, 4326)) = 1
            """,
            Coordinates(latitude, longitude));

        return string.Equals(
            actualIso3,
            expectedIso3,
            StringComparison.OrdinalIgnoreCase);
    }

    private bool ContainsSpain(double latitude, double longitude) =>
        _database.QueryExists(
            """
            SELECT 1
            FROM [COUNTRY_SPAIN]
            WHERE [geography].STIntersects(
                geography::Point(@latitude, @longitude, 4326)) = 1
            """,
            Coordinates(latitude, longitude));

    private static Dictionary<string, object?> Coordinates(
        double latitude,
        double longitude) =>
        new()
        {
            ["latitude"] = latitude,
            ["longitude"] = longitude,
        };

    private static bool IsValidCoordinate(double latitude, double longitude) =>
        double.IsFinite(latitude)
        && double.IsFinite(longitude)
        && latitude is >= -90 and <= 90
        && longitude is >= -180 and <= 180;
}
