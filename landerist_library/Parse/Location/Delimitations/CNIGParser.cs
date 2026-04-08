using NetTopologySuite.Features;
using NetTopologySuite.IO;
using Newtonsoft.Json;
using System.Data;
using System.Threading;

namespace landerist_library.Parse.Location.Delimitations
{
    public class CNIGParser
    {
        public static void Insert()
        {
            Database.CNIG.DeleteAll();

            string file = Configuration.PrivateConfig.DELIMITATIONS_DIRECTORY + @"CNIG\CNIG.geojson";
            Console.WriteLine("Reading " + file);

            var geoJsonSerializer = GeoJsonSerializer.Create();
            FeatureCollection featureCollection;

            using var streamReader = new StreamReader(file);
            using var jsonTextReader = new JsonTextReader(streamReader);
            featureCollection = geoJsonSerializer.Deserialize<FeatureCollection>(jsonTextReader)!;

            int success = 0;
            int errors = 0;

            Parallel.ForEach(
                featureCollection.Cast<Feature>(),
                new ParallelOptions()
                {
                    //MaxDegreeOfParallelism = Config.MAX_DEGREE_OF_PARALLELISM
                },
                feature =>
                {
                    // WKBWriter is not guaranteed to be thread-safe; use one per iteration/thread.
                    var wkbWriter = new WKBWriter();
                    byte[] wkb = wkbWriter.Write(feature.Geometry);
                    string theGeom = WKBWriter.ToHex(wkb);

                    if (!feature.Attributes.Exists("INSPIREID") ||
                        !feature.Attributes.Exists("NATCODE") ||
                        !feature.Attributes.Exists("NAMEUNIT"))
                    {
                        return;
                    }

                    string? inspireId = feature.Attributes["INSPIREID"]?.ToString()?.Trim();
                    string? natCode = feature.Attributes["NATCODE"]?.ToString()?.Trim();
                    string? nameUnit = feature.Attributes["NAMEUNIT"]?.ToString()?.Trim();

                    if (string.IsNullOrWhiteSpace(inspireId) ||
                        string.IsNullOrWhiteSpace(nameUnit) ||
                        string.IsNullOrWhiteSpace(natCode))
                    {
                        return;
                    }

                    if (Database.CNIG.Insert(theGeom, inspireId, natCode, nameUnit))
                    {
                        Interlocked.Increment(ref success);
                    }
                    else
                    {
                        Interlocked.Increment(ref errors);
                    }
                });

            Database.CNIG.MakeValidAll();
            Database.CNIG.ReorientIfNeccesary();
            Console.WriteLine("Success: " + success + " Errors: " + errors);
        }

        public static (string natCode, string nameUnit)? GetNatCodeAndNameUnit(landerist_orels.ES.Listing listing)
        {
            if (listing.latitude == null || listing.longitude == null)
            {
                return null;
            }

            DataRow? dataRow = Database.CNIG.Get((double)listing.latitude, (double)listing.longitude);
            if (dataRow == null)
            {
                return null;
            }

            string natCode = dataRow["natcode"].ToString() ?? string.Empty;
            string nameUnit = dataRow["nameunit"].ToString() ?? string.Empty;

            if (natCode.Length <= 6 || string.IsNullOrWhiteSpace(nameUnit))
            {
                return null;
            }

            natCode = natCode[6..]; // always natCode is 11 length
            return (natCode, nameUnit);
        }
    }
}
