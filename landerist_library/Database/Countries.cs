﻿using System.Globalization;

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

        public static bool Insert(string the_geom, string iso_a3)
        {
            string geom = "geography::STGeomFromWKB(0x" + the_geom + ", 4326)";
            string query =
                "INSERT INTO " + COUNTRIES + " VALUES(" + geom + ", @iso_a3)";

            return new DataBase().Query(query, new Dictionary<string, object?> {
                {"iso_a3", iso_a3 },
            });
        }

        public static bool MakeValidAll()
        {
            string query =
                "UPDATE " + COUNTRIES + " " +
                "SET [the_geom] = [the_geom].MakeValid()";

            return new DataBase().Query(query);
        }

        public static bool ReorientIfNeccesary()
        {
            string query =
                "UPDATE " + COUNTRIES + " " +
                "SET [the_geom] = [the_geom].ReorientObject().MakeValid() " +
                "WHERE [the_geom].EnvelopeAngle() > 90";

            return new DataBase().Query(query);
        }

        public static string Get(double latitude, double longitude)
        {
            string point =
                "POINT(" + longitude.ToString(CultureInfo.InvariantCulture) + " " +
                latitude.ToString(CultureInfo.InvariantCulture) + ")";

            string query =
                "SELECT TOP 1 iso_a3 " +
                "FROM " + COUNTRIES + " " +
                "WITH(INDEX([SpatialIndex-the_geom])) " +
                "WHERE [the_geom].STIntersects(geography::STGeomFromText('" + point + "', 4326)) = 1";

            return new DataBase().QueryString(query);
        }
    }
}
