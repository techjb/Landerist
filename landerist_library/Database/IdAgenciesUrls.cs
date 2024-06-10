namespace landerist_library.Database
{
    public class IdAgenciesUrls
    {
        private const string TABLE_ID_AGENCIES_URLS = "[ID_AGENCIES_URLS]";

        public static bool Insert(string url, int province)
        {
            string query =
                "INSERT INTO " + TABLE_ID_AGENCIES_URLS + " " +
                "VALUES(@Url, @Province, NULL)";

            return new DataBase().Query(query, new Dictionary<string, object?>() {
                {"Url", url },
                {"Province", province },
            });
        }

        public static bool Delete(string url)
        {
            string query =
                "DELETE FROM " + TABLE_ID_AGENCIES_URLS + " " +
                "WHERE Url = @Url";

            return new DataBase().Query(query, new Dictionary<string, object?>() {
                {"Url", url },
            });
        }

        public static HashSet<string> GetNotScrapped()
        {
            string query =
                "SELECT [Url] FROM " + TABLE_ID_AGENCIES_URLS + " " +
                "WHERE [AgencyUrl] IS NULL";

            return new DataBase().QueryHashSet(query);
        }


        public static HashSet<string> GetEmpty()
        {
            string query =
                "SELECT [Url] FROM " + TABLE_ID_AGENCIES_URLS + " " +
                "WHERE [AgencyUrl] = ''";

            return new DataBase().QueryHashSet(query);
        }

        public static HashSet<string> GetAgencies()
        {
            string query =
                "SELECT [AgencyUrl] FROM " + TABLE_ID_AGENCIES_URLS + " " +
                "WHERE [AgencyUrl] <> '' AND [AgencyUrl] IS NOT NULL";

            return new DataBase().QueryHashSet(query);
        }

        public static bool Update(string url, string? agencyUrl)
        {
            string query =
                "UPDATE " + TABLE_ID_AGENCIES_URLS + " " +
                "SET [AgencyUrl] = @AgencyUrl " +
                "WHERE [Url] = @Url";

            return new DataBase().Query(query, new Dictionary<string, object?>() {
                {"AgencyUrl", agencyUrl },
                {"Url", url },
            });
        }
    }
}
