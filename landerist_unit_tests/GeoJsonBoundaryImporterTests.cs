using landerist_library.Infrastructure.Location.Import;

namespace landerist_unit_tests;

public sealed class GeoJsonBoundaryImporterTests
{
    [Fact]
    public void ImportCountries_ImportsGeometryAndRunsMaintenance()
    {
        string root = Path.Combine(
            Path.GetTempPath(),
            "landerist-boundaries-" + Guid.NewGuid().ToString("N"));
        string countries = Path.Combine(root, "Countries");
        Directory.CreateDirectory(countries);
        File.WriteAllText(
            Path.Combine(countries, "countries.geojson"),
            """
            {
              "type": "FeatureCollection",
              "features": [{
                "type": "Feature",
                "properties": { "ISO_A3": "ESP" },
                "geometry": {
                  "type": "Polygon",
                  "coordinates": [[[0,0],[1,0],[1,1],[0,0]]]
                }
              }]
            }
            """);

        try
        {
            RecordingDatabase database = new() { QueryResult = true };
            GeoJsonBoundaryImporter importer = new(database, root);

            BoundaryImportResult result = importer.ImportCountries();

            Assert.Equal(1, result.Imported);
            Assert.Equal(0, result.Errors);
            Assert.Contains(
                database.Calls,
                call => call.Query.Contains(
                    "INSERT INTO [COUNTRIES]",
                    StringComparison.Ordinal));
            Assert.Equal(5, database.Calls.Count);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void ImportCnig_WhenFileIsMissing_DoesNotMutateDatabase()
    {
        RecordingDatabase database = new();
        GeoJsonBoundaryImporter importer = new(
            database,
            Path.GetTempPath());

        Assert.Throws<FileNotFoundException>(() => importer.ImportCnig());
        Assert.Empty(database.Calls);
    }
}
