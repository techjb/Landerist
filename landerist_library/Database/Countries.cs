using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace landerist_library.Database
{
    public class Countries
    {
        private const string COUNTRIES = "[COUNTRIES]";
        
        public static bool DeleteAll()
        {
            string query = "DELETE FROM " + COUNTRIES;
            return new DataBase().Query(query);
        }

        public static bool Insert(string the_geom, string iso_a3, string iso_a2)
        {
            string geom = "geography::STGeomFromWKB(0x"+ the_geom + ", 4326)";
            string query =
                "INSERT INTO " + COUNTRIES + " VALUES("+ geom + ", @iso_a3, @iso_a2)";

            return new DataBase().Query(query, new Dictionary<string, object?> {
                {"iso_a3", iso_a3 },
                {"iso_a2", iso_a2 },
            });
        }
    }
}
