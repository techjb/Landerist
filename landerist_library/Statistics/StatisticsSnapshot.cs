using System.Data;
using landerist_library.Database;

namespace landerist_library.Statistics
{
    public class StatisticsSnapshot
    {
        private static readonly string TABLES_STATISTICS_SNAPSHOT = "[STATISTICS_SNAPSHOT]";

        public static void TakeSnapshots()
        {
            Websites();
            UpdatedWebsites();
            Pages();
            UpdatedPages();
            Es_Listings();
            Es_Media();            
        }

        private static void Websites()
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + landerist_library.Websites.Websites.TABLE_WEBSITES;

            Insert("Websites", query);
        }

        private static void UpdatedWebsites()
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + landerist_library.Websites.Websites.TABLE_WEBSITES + " " +
                "WHERE CONVERT(date, [WebsiteUpdated]) = CONVERT(date, DATEADD(DAY, -1, GETDATE()))";

            Insert("Updated Websites", query);
        }

        private static void Pages()
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + landerist_library.Websites.Pages.TABLE_PAGES;

            Insert("Pages", query);
        }

        private static void UpdatedPages()
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + landerist_library.Websites.Pages.TABLE_PAGES + " " +
                "WHERE CONVERT(date, [Updated]) = CONVERT(date, DATEADD(DAY, -1, GETDATE()))";

            Insert("Updated Pages", query);
        }

        private static void Es_Listings()
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + ES_Listings.TABLE_ES_LISTINGS;

            Insert("Es_Listings", query);
        }

        private static void Es_Media()
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + ES_Media.TABLE_ES_MEDIA;

            Insert("Es_Media", query);
        }

        private static bool Insert(string key, string queryCount)
        {
            int counter = new DataBase().QueryInt(queryCount);
            return Insert(key, counter);
        }

        private static bool Insert(string key, int counter)
        {
            string query =
                "INSERT INTO " + TABLES_STATISTICS_SNAPSHOT + " " +
                "VALUES(GETDATE(), @Key, @Counter ) ";

            return new DataBase().Query(query, new Dictionary<string, object?> {
                    { "Key", key },
                    { "Counter", counter }
                });
        }

        public static DataSet GetStatistics(int lastMonths)
        {
            string query =
                "SELECT DISTINCT [Key] " +
                "FROM " + TABLES_STATISTICS_SNAPSHOT + " ";

            DataTable dataTable = new DataBase().QueryTable(query);

            DataSet dataSet = new();
            foreach (DataRow dataRow in dataTable.Rows)
            {
                string key = (string)dataRow["Key"];
                var dataTableKey = GetStatistics(key, lastMonths);
                dataSet.Tables.Add(dataTableKey);
            }
            return dataSet;
        }

        private static DataTable GetStatistics(string key, int months)
        {
            string query =
                "SELECT [Date], [Key], [Counter] " +
                "FROM " + TABLES_STATISTICS_SNAPSHOT + " " +
                "WHERE [Key] = @Key AND " +
                "[Date] > DATEADD(MONTH, @Months, GETDATE()) " +
                "ORDER BY [Date] ASC";

            return new DataBase().QueryTable(query, new Dictionary<string, object?> {
                { "Key", key },
                { "Months", months }
            });
        }
    }
}
