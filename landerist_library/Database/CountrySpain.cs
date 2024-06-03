using System.Globalization;

namespace landerist_library.Database
{
    public class CountrySpain
    {
        private static readonly string COUNTRY_SPAIN = "[COUNTRY_SPAIN]";
        public static void Insert()
        {
            string wkt = File.ReadAllText("C:\\Users\\Chus\\Downloads\\spain2.csv");
            string query =
                "INSERT INTO " + COUNTRY_SPAIN + " ([geography]) " +
                "VALUES (geography::STGeomFromText(@wkt, 4326))";
            new DataBase().Query(query, new Dictionary<string, object?> {
                {"wkt", wkt },
            });

        }

        public static bool Contains(double latitude, double longitude)
        {

            string point =
                "POINT(" + longitude.ToString(CultureInfo.InvariantCulture) + " " +
                latitude.ToString(CultureInfo.InvariantCulture) + ")";

            string query =
                "SELECT 1 " +
                "FROM " + COUNTRY_SPAIN + " " +
                "WHERE [geography].STIntersects(geography::STGeomFromText('" + point + "', 4326)) = 1";

            return new DataBase().QueryExists(query);
        }
    }
}
