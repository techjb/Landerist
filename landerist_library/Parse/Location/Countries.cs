using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace landerist_library.Parse.Location
{
    // CSV obtained from https://rtr.carto.com/tables/world_countries_geojson/public/map
    public class Countries
    {
        public static void InsertAll()
        {
            Database.Countries.DeleteAll();

            string file = Configuration.Config.DELIMITATIONS_DIRECTORY + @"world_countries_geojson.csv";
            Console.WriteLine("Reading " + file);

            using var reader = new StreamReader(file);
            bool isFirstLine = true;
            int success = 0;
            int errors = 0;
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (line == null)
                {
                    continue;
                }
                
                var values = line.Split(',');
                if (!values.Length.Equals(72))
                {
                    continue;
                }
                if (isFirstLine)
                {
                    isFirstLine = false;
                    continue;
                }

                string the_geom = values[0].Replace("0106000020E61", "1060");
                string iso_a3 = values[34];
                string iso_a2 = values[35];
                //string iso_a3 = values[38];
                //string iso_a2 = values[39];

                if (iso_a3 == "-99")
                {

                }

                if(Database.Countries.Insert(the_geom, iso_a3, iso_a2))
                {
                    success++;
                }
                else
                {
                    errors++;
                }                
            }
            Console.WriteLine("Success: " + success + " Errors: " + errors);
        }
    }
}
