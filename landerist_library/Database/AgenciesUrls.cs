using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace landerist_library.Database
{
    public class AgenciesUrls
    {
        private const string TABLE_AGENCIES_URLS = "[AGENCIES_URLS]";

        public static bool Insert(string url, int province)
        {
            string query =
                "INSERT INTO " + TABLE_AGENCIES_URLS + " " +
                "VALUES(@Url, @Province)";

            return new DataBase().Query(query, new Dictionary<string, object?>() {
                {"Url", url },
                {"Province", province },
            });
        }

        public static bool Delete(string url)
        {
            string query =
                "DELETE FROM " + TABLE_AGENCIES_URLS + " " +
                "WHERE Url = @Url";

            return new DataBase().Query(query, new Dictionary<string, object?>() {
                {"Url", url },
            });
        }


    }
}
