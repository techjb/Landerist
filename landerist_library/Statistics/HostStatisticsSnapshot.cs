using landerist_library.Database;
using landerist_orels.ES;
using System.Data;

namespace landerist_library.Statistics
{
    public enum HostStatisticsKey
    {
        Pages,
        Listings,
        PublishedListings,
        UnpublishedListings,
        HttpStatusCode,
        PageType,
    }

    public static class HostStatisticsSnapshot
    {
        public const string TABLE_HOST_STATISTICS_SNAPSHOT = "[HOST_STATISTICS_SNAPSHOT]";

        public static void TakeSnapshots()
        {
            foreach (var website in Websites.Websites.GetApplySpecialRules())
            {
                try
                {
                    TakeSnapshot(website.Host);
                }
                finally
                {
                    website.Dispose();
                }
            }
        }

        public static void TakeSnapshot(string host)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(host);

            Pages(host);
            Listings(host);
            PublishedListings(host);
            UnpublishedListings(host);
            HttpStatusCode(host);
            PageType(host);
        }

        private static void Pages(string host)
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + landerist_library.Pages.Pages.PAGES + " " +
                "WHERE [Host] = @Host";

            InsertDaily(host, HostStatisticsKey.Pages, query);
        }

        private static void Listings(string host)
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + ES_Listings.TABLE_ES_LISTINGS + " " +
                "WHERE [Host] = @Host";

            InsertDaily(host, HostStatisticsKey.Listings, query);
        }

        private static void PublishedListings(string host)
        {
            SnapshotListings(host, HostStatisticsKey.PublishedListings, ListingStatus.published);
        }

        private static void UnpublishedListings(string host)
        {
            SnapshotListings(host, HostStatisticsKey.UnpublishedListings, ListingStatus.unpublished);
        }

        private static void SnapshotListings(string host, HostStatisticsKey statisticsKey, ListingStatus listingStatus)
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + ES_Listings.TABLE_ES_LISTINGS + " " +
                "WHERE [Host] = @Host AND [listingStatus] = @ListingStatus";

            InsertDaily(host, statisticsKey, query, new Dictionary<string, object?>
            {
                { "Host", host },
                { "ListingStatus", listingStatus.ToString() }
            });
        }

        private static void HttpStatusCode(string host)
        {
            DateTime date = DateTime.Now;

            DeleteByHostKeyPrefixAndDate(date, host, HostStatisticsKey.HttpStatusCode.ToString());

            string query =
                "SELECT [HttpStatusCode], COUNT(*) AS [Counter] " +
                "FROM " + landerist_library.Pages.Pages.PAGES + " " +
                "WHERE [Host] = @Host " +
                "GROUP BY [HttpStatusCode]";

            var dataTable = new DataBase().QueryTable(query, new Dictionary<string, object?>
            {
                { "Host", host }
            });

            foreach (DataRow dataRow in dataTable.Rows)
            {
                short? httpStatusCode = dataRow["HttpStatusCode"] is DBNull ? null : (short)dataRow["HttpStatusCode"];
                int counter = Convert.ToInt32(dataRow["Counter"]);
                string key = HostStatisticsKey.HttpStatusCode + "_" + (httpStatusCode?.ToString() ?? "NULL");
                Insert(date, host, key, counter);
            }
        }

        private static void PageType(string host)
        {
            DateTime date = DateTime.Now;

            DeleteByHostKeyPrefixAndDate(date, host, HostStatisticsKey.PageType.ToString());

            string query =
                "SELECT [PageType], COUNT(*) AS [Counter] " +
                "FROM " + landerist_library.Pages.Pages.PAGES + " " +
                "WHERE [Host] = @Host " +
                "AND [PageType] IS NOT NULL " +
                "GROUP BY [PageType]";

            var dataTable = new DataBase().QueryTable(query, new Dictionary<string, object?>
            {
                { "Host", host }
            });

            foreach (DataRow dataRow in dataTable.Rows)
            {
                string pageType = (string)dataRow["PageType"];
                int counter = Convert.ToInt32(dataRow["Counter"]);
                string key = HostStatisticsKey.PageType + "_" + pageType;
                Insert(date, host, key, counter);
            }
        }

        private static void InsertDaily(string host, HostStatisticsKey key, string queryInt)
        {
            InsertDaily(host, key, queryInt, new Dictionary<string, object?>
            {
                { "Host", host }
            });
        }

        private static void InsertDaily(string host, HostStatisticsKey key, string queryInt, Dictionary<string, object?> parameters)
        {
            int counter = new DataBase().QueryInt(queryInt, parameters);
            Insert(DateTime.Now, host, key.ToString(), counter);
        }

        private static bool DeleteByHostKeyPrefixAndDate(DateTime date, string host, string keyPrefix)
        {
            string query =
                "DELETE FROM " + TABLE_HOST_STATISTICS_SNAPSHOT + " " +
                "WHERE [Host] = @Host " +
                "AND [Key] LIKE @KeyPrefix " +
                "AND CAST([Date] AS date) = CAST(@Date AS date)";

            return new DataBase().Query(query, new Dictionary<string, object?>
            {
                { "Date", date },
                { "Host", host },
                { "KeyPrefix", keyPrefix + "_%" }
            });
        }

        private static bool Insert(DateTime date, string host, string key, int counter)
        {
            string query =
                "DELETE FROM " + TABLE_HOST_STATISTICS_SNAPSHOT + " " +
                "WHERE [Host] = @Host " +
                "AND [Key] = @Key " +
                "AND CAST([Date] AS date) = CAST(@Date AS date); " +
                "INSERT INTO " + TABLE_HOST_STATISTICS_SNAPSHOT + " ([Date], [Host], [Key], [Counter]) " +
                "VALUES (@Date, @Host, @Key, @Counter);";

            return new DataBase().Query(query, new Dictionary<string, object?>
            {
                { "Date", date },
                { "Host", host },
                { "Key", key },
                { "Counter", counter }
            });
        }

        public static DataTable GetLatestStatistics(string host, string statisticsKey, int top)
        {
            string query =
                "SELECT TOP (@Top) [Date], [Counter] " +
                "FROM " + TABLE_HOST_STATISTICS_SNAPSHOT + " " +
                "WHERE [Host] = @Host AND [Key] = @Key " +
                "ORDER BY [Date] DESC";

            return new DataBase().QueryTable(query, new Dictionary<string, object?>
            {
                { "Top", top },
                { "Host", host },
                { "Key", statisticsKey }
            });
        }

        public static DataTable GetLatestStatisticsByPrefix(string host, string keyPrefix)
        {
            string query =
                "SELECT [Key], [Counter] " +
                "FROM " + TABLE_HOST_STATISTICS_SNAPSHOT + " " +
                "WHERE [Host] = @Host " +
                "AND [Key] LIKE @KeyPrefix " +
                "AND CAST([Date] AS date) = (" +
                "   SELECT MAX(CAST([Date] AS date)) " +
                "   FROM " + TABLE_HOST_STATISTICS_SNAPSHOT + " " +
                "   WHERE [Host] = @Host AND [Key] LIKE @KeyPrefix" +
                ") " +
                "ORDER BY [Counter] DESC, [Key] ASC";

            return new DataBase().QueryTable(query, new Dictionary<string, object?>
            {
                { "Host", host },
                { "KeyPrefix", keyPrefix + "_%" }
            });
        }

        public static List<string> GetKeysLike(string host, HostStatisticsKey key)
        {
            string query =
                "SELECT DISTINCT [Key] " +
                "FROM " + TABLE_HOST_STATISTICS_SNAPSHOT + " " +
                "WHERE [Host] = @Host " +
                "AND [Key] LIKE @Key " +
                "ORDER BY [Key] ASC";

            return new DataBase().QueryListString(query, new Dictionary<string, object?>
            {
                { "Host", host },
                { "Key", key + "_%" }
            });
        }

        public static DateTime? GetLatestDate(string host)
        {
            string query =
                "SELECT MAX([Date]) " +
                "FROM " + TABLE_HOST_STATISTICS_SNAPSHOT + " " +
                "WHERE [Host] = @Host";

            var value = new DataBase().QueryTable(query, new Dictionary<string, object?>
            {
                { "Host", host }
            }).Rows[0][0];

            return value is DBNull ? null : (DateTime)value;
        }
    }
}
