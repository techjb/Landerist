namespace landerist_library.Database
{
    public class Countries : DBDelimitations
    {
        private const string TABLE_COUNTRIES = "[COUNTRIES]";

        public static bool DeleteAll()
        {
            return DeleteAll(TABLE_COUNTRIES);
        }

        public static bool Insert(string the_geom, string iso_a3)
        {
            string geom = "geography::STGeomFromWKB(0x" + the_geom + ", 4326)";
            string query =
                "INSERT INTO " + TABLE_COUNTRIES + " VALUES(" + geom + ", @iso_a3)";

            return new DataBase().Query(query, new Dictionary<string, object?> {
                {"iso_a3", iso_a3 },
            });
        }

        public static bool MakeValidAll()
        {
            return MakeValidTheGeom(TABLE_COUNTRIES);
        }

        public static bool ReorientIfNeccesary()
        {
            return ReorientTheGeomIfNeccesary(TABLE_COUNTRIES);
        }

        public static string? Get(double latitude, double longitude)
        {
            return GetString(TABLE_COUNTRIES, "iso_a3", latitude, longitude);
        }
    }
}
