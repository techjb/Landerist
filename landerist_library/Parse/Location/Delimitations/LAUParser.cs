using NetTopologySuite.Features;
using NetTopologySuite.IO;
using Newtonsoft.Json;
using System.Data;

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

            var wKBWriter = new WKBWriter();
            Parallel.ForEach(featureCollection.Cast<Feature>(), feature =>
            {
                byte[] wkb = wKBWriter.Write(feature.Geometry);
                string the_geom = WKBWriter.ToHex(wkb);
                string gisco_id = feature.Attributes["GISCO_ID"].ToString()!.Trim();
                string lau_id = feature.Attributes["LAU_ID"].ToString()!.Trim();
                string lau_name = feature.Attributes["LAU_NAME"].ToString()!.Trim();

                if (lau_name.Equals(string.Empty) || lau_id.Equals(string.Empty))
                {
                    return;
                }
                if (Database.LAU.Insert(the_geom, gisco_id, lau_id, lau_name))
                {
                    success++;
                }
                else
                {
                    errors++;
                }
            });

            Database.LAU.MakeValidAll();
            Database.LAU.ReorientIfNeccesary();
            Console.WriteLine("Success: " + success + " Errors: " + errors);
        }

        public static string? GetId(landerist_orels.ES.Listing listing)
        {
            if (listing.latitude == null || listing.longitude == null)
            {
                return null;
            }
            return GetId((double)listing.latitude, (double)listing.longitude);
        }

        public static string? GetId(double latitude, double longitude)
        {
            var idAndName = GetIdAndName(latitude, longitude);
            if (idAndName == null)
            {
                return null;
            }
            return idAndName.Item1;
        }

        public static Tuple<string, string>? GetIdAndName(double latitude, double longitude)
        {
            DataRow? dataRow = Database.LAU.Get(latitude, longitude);
            if (dataRow == null)
            {
                return null;
            }
            string lau_id = dataRow["lau_id"].ToString()!;
            string lau_name = dataRow["lau_name"].ToString()!;

            return Tuple.Create(lau_id, lau_name);
        }
    }
}
