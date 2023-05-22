using System.Data;
using System.Globalization;

namespace landerist_library.Database
{
    internal class LAU
    {
        private const string TABLE_LAU = "[LAU]";

        public static bool DeleteAll()
        {
            string query = "DELETE FROM " + TABLE_LAU;
            return new DataBase().Query(query);
        }

        public static bool Insert(string the_geom, string gisco_id, string lau_id, string lau_name)
        {
            string geom = "geography::STGeomFromWKB(0x" + the_geom + ", 4326)";
            string query =
                "INSERT INTO " + TABLE_LAU + " VALUES(" + geom + ",@gisco_id, @lau_id, @lau_name)";

            return new DataBase().Query(query, new Dictionary<string, object?> {
                {"gisco_id", gisco_id },
                {"lau_id", lau_id },
                {"lau_name", lau_name },
            });
        }

        public static bool MakeValidAll()
        {
            string query =
                "UPDATE " + TABLE_LAU + " " +
                "SET [the_geom] = [the_geom].MakeValid()";

            return new DataBase().Query(query);
        }

        public static bool ReorientIfNeccesary()
        {
            string query =
                "UPDATE " + TABLE_LAU + " " +
                "SET [the_geom] = [the_geom].ReorientObject().MakeValid() " +
                "WHERE [the_geom].EnvelopeAngle() > 90";

            return new DataBase().Query(query);
        }

        public static DataRow? Get(double latitude, double longitude)
        {
            string point =
                "POINT(" + longitude.ToString(CultureInfo.InvariantCulture) + " " +
                latitude.ToString(CultureInfo.InvariantCulture) + ")";

            string query =
                "SELECT TOP 1 lau_id, lau_name " +
                "FROM " + TABLE_LAU + " " +
                "WITH(INDEX([SpatialIndex-the_geom])) " +
                "WHERE [the_geom].STIntersects(geography::STGeomFromText('" + point + "', 4326)) = 1";

            DataTable dataTable = new DataBase().QueryTable(query);
            if (dataTable.Rows.Count > 0)
            {
                return dataTable.Rows[0];
            }
            return null;
        }
    }
}
