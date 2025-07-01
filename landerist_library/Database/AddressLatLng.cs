namespace landerist_library.Database
{
    public class AddressLatLng
    {
        private const string ADDRESS_LAT_LNG = "[ADDRESS_LAT_LNG]";

        public static bool Insert(string address, string region, double lat, double lng, bool isAccurate)
        {
            string query =
                "INSERT INTO " + ADDRESS_LAT_LNG + " " +
                "VALUES (GETDATE(), @Address, @Region, @Lat, @Lng, @IsAccurate)";
            return new DataBase().Query(query, new Dictionary<string, object?> {
                {"Address", address },
                {"Region", region },
                {"Lat", lat },
                {"Lng", lng },
                {"IsAccurate", isAccurate }
            });
        }

        public static (double lat, double lng, bool isAccurate)? Select(string address, string region)
        {
            string query =
                "SELECT TOP 1 Lat, Lng, IsAccurate " +
                "FROM " + ADDRESS_LAT_LNG + " " +
                "WHERE Address = @Address AND Region = @Region";

            var dataTable = new DataBase().QueryTable(query, new Dictionary<string, object?> {
                {"Address", address },
                {"Region", region },
            });

            if (dataTable.Rows.Count == 0)
            {
                return null;
            }

            var lat = (double)dataTable.Rows[0]["Lat"];
            var lng = (double)dataTable.Rows[0]["Lng"];
            var isAccurate = (bool)dataTable.Rows[0]["IsAccurate"];

            return (lat, lng, isAccurate);
        }

        public static bool Clean()
        {
            string query =
                "DELETE FROM " + ADDRESS_LAT_LNG + " " +
                "WHERE [DateInsert] < DATEADD(YEAR, -2, GETDATE())";

            return new DataBase().Query(query);
        }
    }
}
