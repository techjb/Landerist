namespace landerist_library.Database
{
    public class FtAgenciesUrls
    {
        private const string TABLE_FT_AGENCIES_URLS = "[FT_AGENCIES_URLS]";

        public static bool Insert(string url)
        {
            string query =
                "INSERT INTO " + TABLE_FT_AGENCIES_URLS + " " +
                "VALUES(@Url, NULL)";

            return new DataBase().Query(query, new Dictionary<string, object?>() {
                {"Url", url },
            });
        }

        public static bool Delete(string url)
        {
            string query =
                "DELETE FROM " + TABLE_FT_AGENCIES_URLS + " " +
                "WHERE Url = @Url";

            return new DataBase().Query(query, new Dictionary<string, object?>() {
                {"Url", url },
            });
        }

        public static HashSet<string> GetNotScrapped()
        {
            string query =
                "SELECT [Url] FROM " + TABLE_FT_AGENCIES_URLS + " " +
                "WHERE [AgencyUrl] IS NULL";

            return new DataBase().QueryHashSet(query);
        }


        public static HashSet<string> GetEmpty()
        {
            string query =
                "SELECT [Url] FROM " + TABLE_FT_AGENCIES_URLS + " " +
                "WHERE [AgencyUrl] = ''";

            return new DataBase().QueryHashSet(query);
        }

        public static HashSet<string> GetAgencies()
        {
            string query =
                "SELECT [AgencyUrl] FROM " + TABLE_FT_AGENCIES_URLS + " " +
                "WHERE [AgencyUrl] <> '' AND [AgencyUrl] IS NOT NULL";

            return new DataBase().QueryHashSet(query);
        }

        public static bool Update(string url, string? agencyUrl)
        {
            string query =
                "UPDATE " + TABLE_FT_AGENCIES_URLS + " " +
                "SET [AgencyUrl] = @AgencyUrl " +
                "WHERE [Url] = @Url";

            return new DataBase().Query(query, new Dictionary<string, object?>() {
                {"AgencyUrl", agencyUrl },
                {"Url", url },
            });
        }
    }
}
