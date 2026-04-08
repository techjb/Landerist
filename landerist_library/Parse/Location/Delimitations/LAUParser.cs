using NetTopologySuite.Features;
using NetTopologySuite.IO;
using Newtonsoft.Json;
using System.Data;
using System.Threading;

namespace landerist_library.Parse.Location.Delimitations
{
    public class LAUParser
    {
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

        public static (string lau_id, string lau_name)? GetLauIdAndLauName(landerist_orels.ES.Listing listing)
        {
            if (listing.latitude == null || listing.longitude == null)
            {
                return null;
            }

            DataRow? dataRow = Database.LAU.Get((double)listing.latitude, (double)listing.longitude);
            if (dataRow == null)
            {
                return null;
            }

            string lau_id = dataRow["lau_id"].ToString()!;
            string lau_name = dataRow["lau_name"].ToString()!;

            return (lau_id, lau_name);
        }
    }
}
