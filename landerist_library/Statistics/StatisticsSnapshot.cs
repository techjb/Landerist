using System.Data;
using landerist_library.Database;
using static landerist_library.Statistics.StatisticsSnapshot;

namespace landerist_library.Statistics
{
    public enum StatisticsKey
    {
        Listings,
        Media,
        Pages,
        Websites,
        UpdatedIpAddress,
        UpdatedPages,
        UpdatedWebsites,
        UpdatedRobotsTxt,
        UpdatedSitemaps
    }

    public class StatisticsSnapshot
    {       
        private static readonly string TABLES_STATISTICS_SNAPSHOT = "[STATISTICS_SNAPSHOT]";

        public static void TakeSnapshots()
        {
            SnapshotWebsites();
            SnapshotUpdatedRobotsTxt();
            SnapshotUpdatedSitemaps();
            SnapshotUpdatedIpAddress();
            SnapshotPages();
            SnapshotUpdatedPages();
            SnapshotEs_Listings();
            SnapshotEs_Media();            
        }

        private static void SnapshotWebsites()
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + Websites.Websites.TABLE_WEBSITES;

            Insert(StatisticsKey.Websites, query);
        }

        private static void SnapshotUpdatedRobotsTxt()
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + Websites.Websites.TABLE_WEBSITES + " " +
                "WHERE CONVERT(date, [RobotsTxtUpdated]) = CONVERT(date, DATEADD(DAY, -1, GETDATE()))";

            Insert(StatisticsKey.UpdatedRobotsTxt, query);
        }

        private static void SnapshotUpdatedSitemaps()
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + Websites.Websites.TABLE_WEBSITES + " " +
                "WHERE CONVERT(date, [SitemapUpdated]) = CONVERT(date, DATEADD(DAY, -1, GETDATE()))";

            Insert(StatisticsKey.UpdatedSitemaps, query);
        }

        private static void SnapshotUpdatedIpAddress()
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + Websites.Websites.TABLE_WEBSITES + " " +
                "WHERE CONVERT(date, [IpAddressUpdated]) = CONVERT(date, DATEADD(DAY, -1, GETDATE()))";

            Insert(StatisticsKey.UpdatedIpAddress, query);
        }

        private static void SnapshotPages()
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + Websites.Pages.TABLE_PAGES;

            Insert(StatisticsKey.Pages, query);
        }

        private static void SnapshotUpdatedPages()
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + Websites.Pages.TABLE_PAGES + " " +
                "WHERE CONVERT(date, [Updated]) = CONVERT(date, DATEADD(DAY, -1, GETDATE()))";

            Insert(StatisticsKey.UpdatedPages, query);
        }

        private static void SnapshotEs_Listings()
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + ES_Listings.TABLE_ES_LISTINGS;

            Insert(StatisticsKey.Listings, query);
        }

        private static void SnapshotEs_Media()
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + ES_Media.TABLE_ES_MEDIA;

            Insert(StatisticsKey.Media, query);
        }

        private static bool Insert(StatisticsKey statisticsKey, string queryCount)
        {
            int counter = new DataBase().QueryInt(queryCount);
            return Insert(statisticsKey, counter);
        }

        private static bool Insert(StatisticsKey statisticsKey, int counter)
        {
            string query =
                "INSERT INTO " + TABLES_STATISTICS_SNAPSHOT + " " +
                "VALUES(GETDATE(), @Key, @Counter ) ";

            return new DataBase().Query(query, new Dictionary<string, object?> {
                    { "Key", statisticsKey.ToString() },
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

        public static DataTable GetTop100Statistics(StatisticsKey statisticsKey)
        {
            string query =
                "SELECT TOP 100 [Date], [Counter] " +
                "FROM " + TABLES_STATISTICS_SNAPSHOT + " " +
                "WHERE [Key] = @Key " +                
                "ORDER BY [Date] ASC";

            return new DataBase().QueryTable(query, new Dictionary<string, object?> {
                { "Key", statisticsKey.ToString() }
            });
        }
    }
}
