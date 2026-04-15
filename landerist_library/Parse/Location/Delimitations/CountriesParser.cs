using landerist_library.Websites;
using NetTopologySuite.Features;
using NetTopologySuite.IO;
using Newtonsoft.Json;

namespace landerist_library.Parse.Location.Delimitations
{
    public class CountriesParser
    {
        // CSV obtained from https://rtr.carto.com/tables/world_countries_geojson/public/map
        //public static void Insert()
        //{
        //    Database.CountriesParser.DeleteAll();

        //    string file = Configuration.Config.DELIMITATIONS_DIRECTORY + @"world_countries_geojson.csv";
        //    Console.WriteLine("Reading " + file);

        //    using var reader = new StreamReader(file);
        //    bool isFirstLine = true;
        //    int success = 0;
        //    int ErrorsMainUri = 0;
        //    while (!reader.EndOfStream)
        //    {
        //        var line = reader.ReadLine();
        //        if (line == null)
        //        {
        //            continue;
        //        }

        //        var values = line.Split(',');
        //        if (!values.Length.Equals(72))
        //        {
        //            continue;
        //        }
        //        if (isFirstLine)
        //        {
        //            isFirstLine = false;
        //            continue;
        //        }

        //        string the_geom = values[0].Replace("0106000020E61", "1060");
        //        string iso_a3 = values[34];
        //        string iso_a2 = values[35];
        //        //string iso_a3 = values[38];
        //        //string iso_a2 = values[39];

        //        if (iso_a3 == "-99")
        //        {

        //        }

        //        if(Database.CountriesParser.Insert(the_geom, iso_a3, iso_a2))
        //        {
        //            success++;
        //        }
        //        else
        //        {
        //            ErrorsMainUri++;
        //        }
        //    }
        //    Console.WriteLine("ScrapedSuccess: " + success + " ErrorsMainUri: " + ErrorsMainUri);
        //}

        /// <summary>
        /// GeoJSON source: https://datahub.io/core/geo-countries
        /// </summary>
        public static void Insert()
        {
            Database.Countries.DeleteAll();

            string file = Configuration.PrivateConfig.DELIMITATIONS_DIRECTORY + @"Countries\countries.geojson";
            Console.WriteLine("Reading " + file);

            if (!File.Exists(file))
            {
                Console.WriteLine("File not found: " + file);
                return;
            }

            var geoJsonSerializer = GeoJsonSerializer.Create();
            FeatureCollection? featureCollection;

            using var streamReader = new StreamReader(file);
            using var jsonTextReader = new JsonTextReader(streamReader);
            featureCollection = geoJsonSerializer.Deserialize<FeatureCollection>(jsonTextReader);

            if (featureCollection is null)
            {
                Console.WriteLine("Could not deserialize countries GeoJSON.");
                return;
            }

            int success = 0;
            int errors = 0;

            var wkbWriter = new WKBWriter();

            foreach (var feature in featureCollection)
            {
                if (feature?.Geometry is null)
                {
                    errors++;
                    continue;
                }

                if (!feature.Attributes.Exists("ISO_A3"))
                {
                    errors++;
                    continue;
                }

                string isoA3 = feature.Attributes["ISO_A3"]?.ToString()?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(isoA3) || isoA3 == "-99")
                {
                    continue;
                }

                byte[] wkb = wkbWriter.Write(feature.Geometry);
                string theGeom = WKBWriter.ToHex(wkb);

                if (Database.Countries.Insert(theGeom, isoA3))
                {
                    success++;
                }
                else
                {
                    errors++;
                }
            }

            Database.Countries.MakeValidAll();
            // run twice because there is a problen in ios_a3 = "NOR"
            Database.Countries.ReorientIfNeccesary();
            Database.Countries.ReorientIfNeccesary();

            Console.WriteLine("Success: " + success + " Errors: " + errors);
        }

        public static bool ContainsCountry(CountryCode countryCode, double latitude, double longitude)
        {
            return countryCode switch
            {
                CountryCode.ES => Database.CountrySpain.Contains(latitude, longitude),
                _ => ContainsCountryAll(countryCode, latitude, longitude),
            };
        }

        public static bool ContainsCountryAll(CountryCode countryCode, double latitude, double longitude)
        {
            string? iso3 = Database.Countries.Get(latitude, longitude);
            if (iso3 is null)
            {
                return false;
            }

            return countryCode switch
            {
                CountryCode.ES => iso3 == "ESP",
                _ => false
            };
        }
    }
}
