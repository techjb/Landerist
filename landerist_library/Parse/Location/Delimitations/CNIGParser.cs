﻿using NetTopologySuite.Features;
using NetTopologySuite.IO;
using Newtonsoft.Json;
using System.Data;

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

            var wKBWriter = new WKBWriter();
            Parallel.ForEach(featureCollection.Cast<Feature>(),
                new ParallelOptions()
                {
                    //MaxDegreeOfParallelism = Config.MAX_DEGREE_OF_PARALLELISM
                },
                feature =>
            {
                byte[] wkb = wKBWriter.Write(feature.Geometry);
                string the_geom = WKBWriter.ToHex(wkb);

                string inspireId = feature.Attributes["INSPIREID"].ToString()!.Trim();
                string natCode = feature.Attributes["NATCODE"].ToString()!.Trim();
                string nameUnit = feature.Attributes["NAMEUNIT"].ToString()!.Trim();

                if (inspireId.Equals(string.Empty) ||
                    nameUnit.Equals(string.Empty) ||
                    natCode.Equals(string.Empty))
                {
                    return;
                }
                if (Database.CNIG.Insert(the_geom, inspireId, natCode, nameUnit))
                {
                    success++;
                }
                else
                {
                    errors++;
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
            string natCode = dataRow["natcode"].ToString()!;
            string nameUnit = dataRow["nameunit"].ToString()!;

            natCode = natCode[6..]; // always natCode is 11 length
            return (natCode, nameUnit);
        }
    }
}
