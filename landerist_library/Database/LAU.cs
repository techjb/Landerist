using System.Data;

namespace landerist_library.Database
{
    internal class LAU : DBDelimitations
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
            return MakeValidTheGeom(TABLE_LAU);
        }

        public static bool ReorientIfNeccesary()
        {
            return ReorientTheGeomIfNeccesary(TABLE_LAU);
        }

        public static DataRow? Get(double latitude, double longitude)
        {
            return GetdDataRow(TABLE_LAU, "lau_id, lau_name", latitude, longitude);
        }
    }
}
