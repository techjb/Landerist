using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace landerist_library.Database
{
    public class AddressLatLng
    {
        private const string ADDRESS_LAT_LNG = "[ADDRESS_LAT_LNG]";

        public static bool Insert(string address, string region, double lat, double lng, bool isAccurate)
        {
            address = ParseAddress(address);

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

        public static (double, double, bool)? Select(string address, string region)
        {
            address = ParseAddress(address);
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

        private static string ParseAddress(string address)
        {
            return Tools.Strings.Clean(address.ToLower());
        }

        public static bool Clean()
        {
            string query = 
                "DELETE FROM " + ADDRESS_LAT_LNG + " " +
                "WHERE Date < DATEADD(YEAR, -2, GETDATE())";

            return new DataBase().Query(query);
        }
    }
}
