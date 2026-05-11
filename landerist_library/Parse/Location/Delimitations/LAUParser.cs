using NetTopologySuite.Features;
using NetTopologySuite.IO;
using Newtonsoft.Json;
using System.Data;
using System.Threading;

namespace landerist_library.Parse.Location.Delimitations
{
    public class LAUParser
    {
        private const string LauIdColumnName = "LauId";
        private const string LauNameColumnName = "LauName";

        public static void Insert()
        {
            Database.LAU.DeleteAll();

            string file = Configuration.PrivateConfig.DELIMITATIONS_DIRECTORY + @"LAU\LAU_RG_01M_2021_4326.geojson";
            Console.WriteLine("Reading " + file);

            var geoJsonSerializer = GeoJsonSerializer.Create();
            FeatureCollection featureCollection;

            using var streamReader = new StreamReader(file);
            using var jsonTextReader = new JsonTextReader(streamReader);
            featureCollection = geoJsonSerializer.Deserialize<FeatureCollection>(jsonTextReader)!;

            int success = 0;
            int errors = 0;

            Parallel.ForEach(featureCollection.Cast<Feature>(), feature =>
            {
                var wkbWriter = new WKBWriter();
                byte[] wkb = wkbWriter.Write(feature.Geometry);
                string the_geom = WKBWriter.ToHex(wkb);

                string gisco_id = feature.Attributes["GISCO_ID"]?.ToString()?.Trim() ?? string.Empty;
                string lau_id = feature.Attributes["LAU_ID"]?.ToString()?.Trim() ?? string.Empty;
                string lau_name = feature.Attributes["LAU_NAME"]?.ToString()?.Trim() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(lau_name) || string.IsNullOrWhiteSpace(lau_id))
                {
                    return;
                }

                if (Database.LAU.Insert(the_geom, gisco_id, lau_id, lau_name))
                {
                    Interlocked.Increment(ref success);
                }
                else
                {
                    Interlocked.Increment(ref errors);
                }
            });

            Database.LAU.MakeValidAll();
            Database.LAU.ReorientIfNeccesary();
            Console.WriteLine("Success: " + success + " Errors: " + errors);
        }

        public static (string lauId, string lauName)? GetLauIdAndLauName(landerist_orels.ES.Listing listing)
        {
            ArgumentNullException.ThrowIfNull(listing);

            if (listing.latitude is not double latitude || listing.longitude is not double longitude)
            {
                return null;
            }

            return GetLauIdAndLauName(latitude, longitude);
        }

        public static (string lauId, string lauName)? GetLauIdAndLauName(double latitude, double longitude)
        {
            if (!IsValidCoordinate(latitude, longitude))
            {
                return null;
            }

            DataRow? dataRow = Database.LAU.Get(latitude, longitude);
            if (dataRow == null)
            {
                return null;
            }

            string lauId = dataRow[LauIdColumnName].ToString()?.Trim() ?? string.Empty;
            string lauName = dataRow[LauNameColumnName].ToString()?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(lauId) || string.IsNullOrWhiteSpace(lauName))
            {
                return null;
            }

            return (lauId, lauName);
        }

        private static bool IsValidCoordinate(double latitude, double longitude)
        {
            return double.IsFinite(latitude)
                && double.IsFinite(longitude)
                && latitude is >= -90 and <= 90
                && longitude is >= -180 and <= 180;
        }
    }
}
