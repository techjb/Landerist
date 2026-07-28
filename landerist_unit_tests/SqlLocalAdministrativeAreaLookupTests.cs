using landerist_library.Infrastructure.Location;
using landerist_library.Websites;

namespace landerist_unit_tests;

public sealed class SqlLocalAdministrativeAreaLookupTests
{
    [Fact]
    public void Find_ForSpain_UsesCnigAndNormalizesNatCode()
    {
        RecordingDatabase database = new();
        database.TableResult.Columns.Add("AreaId", typeof(string));
        database.TableResult.Columns.Add("AreaName", typeof(string));
        database.TableResult.Rows.Add("ABCDEF28079", "Madrid");
        SqlLocalAdministrativeAreaLookup lookup = new(database);

        var area = lookup.Find(CountryCode.ES, 40.4168, -3.7038);

        Assert.NotNull(area);
        Assert.Equal("28079", area.Id);
        Assert.Equal("Madrid", area.Name);
        Assert.Contains("FROM [CNIG]", database.LastQuery);
        Assert.Equal(40.4168, database.LastParameters!["latitude"]);
        Assert.Equal(-3.7038, database.LastParameters["longitude"]);
    }

    [Theory]
    [InlineData(double.NaN, 0)]
    [InlineData(91, 0)]
    [InlineData(0, 181)]
    public void Find_WithInvalidCoordinates_DoesNotQuery(
        double latitude,
        double longitude)
    {
        RecordingDatabase database = new();
        SqlLocalAdministrativeAreaLookup lookup = new(database);

        var area = lookup.Find(CountryCode.ES, latitude, longitude);

        Assert.Null(area);
        Assert.Empty(database.Calls);
    }
}
