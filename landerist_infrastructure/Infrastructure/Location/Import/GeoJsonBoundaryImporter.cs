using landerist_library.Database;
using NetTopologySuite.Features;
using NetTopologySuite.IO;
using Newtonsoft.Json;

namespace landerist_library.Infrastructure.Location.Import;

public sealed record BoundaryImportResult(int Imported, int Errors);

public sealed class GeoJsonBoundaryImporter(
    IDatabase database,
    string delimitationsDirectory)
{
    public BoundaryImportResult ImportCountries() =>
        Import(
            Path.Combine(
                delimitationsDirectory,
                "Countries",
                "countries.geojson"),
            "[COUNTRIES]",
            feature =>
            {
                string isoA3 = Attribute(feature, "ISO_A3");
                return string.IsNullOrWhiteSpace(isoA3) || isoA3 == "-99"
                    ? null
                    : new BoundaryRow(
                        "([the_geom], [iso_a3]) VALUES ({0}, @isoA3)",
                        new Dictionary<string, object?>
                        {
                            ["isoA3"] = isoA3,
                        });
            },
            reorientPasses: 2);

    public BoundaryImportResult ImportCnig() =>
        Import(
            Path.Combine(delimitationsDirectory, "CNIG", "CNIG.geojson"),
            "[CNIG]",
            feature =>
            {
                string inspireId = Attribute(feature, "INSPIREID");
                string natCode = Attribute(feature, "NATCODE");
                string nameUnit = Attribute(feature, "NAMEUNIT");
                return string.IsNullOrWhiteSpace(inspireId)
                    || string.IsNullOrWhiteSpace(natCode)
                    || string.IsNullOrWhiteSpace(nameUnit)
                    ? null
                    : new BoundaryRow(
                        """
                        ([the_geom], [inspireid], [natcode], [nameunit])
                        VALUES ({0}, @inspireId, @natCode, @nameUnit)
                        """,
                        new Dictionary<string, object?>
                        {
                            ["inspireId"] = inspireId,
                            ["natCode"] = natCode,
                            ["nameUnit"] = nameUnit,
                        });
            });

    public BoundaryImportResult ImportLau() =>
        Import(
            Path.Combine(
                delimitationsDirectory,
                "LAU",
                "LAU_RG_01M_2021_4326.geojson"),
            "[LAU]",
            feature =>
            {
                string giscoId = Attribute(feature, "GISCO_ID");
                string lauId = Attribute(feature, "LAU_ID");
                string lauName = Attribute(feature, "LAU_NAME");
                return string.IsNullOrWhiteSpace(lauId)
                    || string.IsNullOrWhiteSpace(lauName)
                    ? null
                    : new BoundaryRow(
                        """
                        ([the_geom], [gisco_id], [lau_id], [lau_name])
                        VALUES ({0}, @giscoId, @lauId, @lauName)
                        """,
                        new Dictionary<string, object?>
                        {
                            ["giscoId"] = giscoId,
                            ["lauId"] = lauId,
                            ["lauName"] = lauName,
                        });
            });

    private BoundaryImportResult Import(
        string file,
        string table,
        Func<Feature, BoundaryRow?> map,
        int reorientPasses = 1)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(delimitationsDirectory);
        if (!File.Exists(file))
        {
            throw new FileNotFoundException(
                "The GeoJSON boundary file was not found.",
                file);
        }

        FeatureCollection features = Read(file);
        database.Query($"DELETE FROM {table}");

        int imported = 0;
        int errors = 0;
        WKBWriter writer = new();
        foreach (Feature feature in features)
        {
            BoundaryRow? row = feature.Geometry is null
                ? null
                : map(feature);
            if (row is null)
            {
                errors++;
                continue;
            }

            string geometry =
                "geography::STGeomFromWKB(0x"
                + WKBWriter.ToHex(writer.Write(feature.Geometry))
                + ", 4326)";
            string query =
                $"INSERT INTO {table} "
                + string.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    row.InsertClause,
                    geometry);

            if (database.Query(query, row.Parameters))
            {
                imported++;
            }
            else
            {
                errors++;
            }
        }

        database.Query(
            $"UPDATE {table} SET [the_geom] = [the_geom].MakeValid()");
        for (int pass = 0; pass < reorientPasses; pass++)
        {
            database.Query(
                $"""
                UPDATE {table}
                SET [the_geom] = [the_geom].ReorientObject().MakeValid()
                WHERE [the_geom].EnvelopeAngle() > 90
                """);
        }

        return new BoundaryImportResult(imported, errors);
    }

    private static FeatureCollection Read(string file)
    {
        using StreamReader stream = new(file);
        using JsonTextReader json = new(stream);
        return GeoJsonSerializer.Create()
            .Deserialize<FeatureCollection>(json)
            ?? throw new InvalidDataException(
                $"Could not deserialize GeoJSON boundaries from {file}.");
    }

    private static string Attribute(Feature feature, string name) =>
        feature.Attributes.Exists(name)
            ? feature.Attributes[name]?.ToString()?.Trim() ?? string.Empty
            : string.Empty;

    private sealed record BoundaryRow(
        string InsertClause,
        Dictionary<string, object?> Parameters);
}
