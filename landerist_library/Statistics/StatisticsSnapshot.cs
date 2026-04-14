using landerist_library.Database;
using landerist_library.Pages;
using landerist_orels.ES;
using System.Data;

namespace landerist_library.Statistics
{
    public enum StatisticsKey
    {
        Listings,
        Media,
        PublishedListings,
        UnpublishedListings,
        Pages,
        Websites,
        UpdatedIpAddress,
        UpdatedPages,
        NeedUpdate,
        WaitingAIRequest,
        UnknownPageType,
        UpdatedWebsites,
        UpdatedRobotsTxt,
        UpdatedSitemaps,
        HttpStatusCode,
        HttpStatusCode_NULL,
        HttpStatusCode_200,
        PageType,
        ScrapedSuccess,
        ScrapedCrashed,
        ScrapedHttpStatusCodeNotOK,
        BatchReaded,
        BatchReadedErrors,
        ListingInsert,
        ListingUpdate,
        LocalAIParsingErrors,
        LocalAIParsingSuccess,
        NotListingCache,
        ResponseBodyTextAlreadyParsed,
        ReponseBodyTextIsAnotherListingInHost,
    }

    public class StatisticsSnapshot
    {
        private const string TABLE_STATISTICS_SNAPSHOT = "[STATISTICS_SNAPSHOT]";

        public static void TakeSnapshots()
        {
            Websites();
            UpdatedRobotsTxt();
            UpdatedSitemaps();
            UpdatedIpAddress();
            Pages();
            UpdatedPages();
            NeedUpdate();
            WaitingAIRequest();
            UnknownPageType();
            Listings();
            PublishedListings();
            UnPublishedListings();
            Media();
            HttpStatusCode();
            PageType();
        }

        private static void Websites()
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + landerist_library.Websites.Websites.WEBSITES;

            InsertDaily(StatisticsKey.Websites, query);
        }

        private static void UpdatedRobotsTxt()
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + landerist_library.Websites.Websites.WEBSITES + " " +
                "WHERE CONVERT(date, [RobotsTxtUpdated]) = CONVERT(date, DATEADD(DAY, -1, GETDATE()))";

            InsertDaily(StatisticsKey.UpdatedRobotsTxt, query);
        }

        private static void UpdatedSitemaps()
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + landerist_library.Websites.Websites.WEBSITES + " " +
                "WHERE CONVERT(date, [SitemapUpdated]) = CONVERT(date, DATEADD(DAY, -1, GETDATE()))";

            InsertDaily(StatisticsKey.UpdatedSitemaps, query);
        }

        private static void UpdatedIpAddress()
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + landerist_library.Websites.Websites.WEBSITES + " " +
                "WHERE CONVERT(date, [IpAddressUpdated]) = CONVERT(date, DATEADD(DAY, -1, GETDATE()))";

            InsertDaily(StatisticsKey.UpdatedIpAddress, query);
        }

        private static void Pages()
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + landerist_library.Pages.Pages.PAGES;

            InsertDaily(StatisticsKey.Pages, query);
        }

        private static void UpdatedPages()
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + landerist_library.Pages.Pages.PAGES + " " +
                "WHERE CONVERT(date, [Updated]) = CONVERT(date, DATEADD(DAY, -1, GETDATE()))";

            InsertDaily(StatisticsKey.UpdatedPages, query);
        }

        private static void NeedUpdate()
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + landerist_library.Pages.Pages.PAGES + " " +
                "WHERE [NextUpdate] < GETDATE()";

            InsertDaily(StatisticsKey.NeedUpdate, query);
        }

        private static void WaitingAIRequest()
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + landerist_library.Pages.Pages.PAGES + " " +
                "WHERE [WaitingStatus] = @WaitingStatus";

            InsertDaily(StatisticsKey.WaitingAIRequest, query, new Dictionary<string, object?>
            {
                { "WaitingStatus", WaitingStatus.waiting_ai_request.ToString() }
            });
        }

        private static void UnknownPageType()
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + landerist_library.Pages.Pages.PAGES + " " +
                "WHERE [PageType] IS NULL";

            InsertDaily(StatisticsKey.UnknownPageType, query);
        }

        private static void Listings()
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + ES_Listings.TABLE_ES_LISTINGS;

            InsertDaily(StatisticsKey.Listings, query);
        }

        private static void PublishedListings()
        {
            SnapshotListings(StatisticsKey.PublishedListings, ListingStatus.published);
        }

        private static void UnPublishedListings()
        {
            SnapshotListings(StatisticsKey.UnpublishedListings, ListingStatus.unpublished);
        }

        private static void SnapshotListings(StatisticsKey statisticsKey, ListingStatus listingStatus)
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + ES_Listings.TABLE_ES_LISTINGS + " " +
                "WHERE [listingStatus] = @ListingStatus";

            InsertDaily(statisticsKey, query, new Dictionary<string, object?>
            {
                { "ListingStatus", listingStatus.ToString() }
            });
        }

        private static void Media()
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + ES_Media.TABLE_ES_MEDIA;

            InsertDaily(StatisticsKey.Media, query);
        }

        public static void SnapshotHttpStatusCode7Days()
        {
            for (var days = -7; days <= -1; days++)
            {
                SnapshotHttpStatusCode(days);
            }
        }

        public static void HttpStatusCode()
        {
            SnapshotHttpStatusCode(-1);
        }

        public static void SnapshotHttpStatusCode(int days)
        {
            DateTime date = DateTime.Today.AddDays(days);

            DeleteByKeyPrefixAndDate(date, StatisticsKey.HttpStatusCode.ToString());

            string query =
                "SELECT [HttpStatusCode], COUNT(*) AS [Counter] " +
                "FROM " + landerist_library.Pages.Pages.PAGES + " " +
                "WHERE CAST([Updated] AS date) = CAST(@Date AS date) " +
                "GROUP BY [HttpStatusCode] ";

            var dataTable = new DataBase().QueryTable(query, new Dictionary<string, object?>
            {
                { "Date", date }
            });

            foreach (DataRow dataRow in dataTable.Rows)
            {
                short? httpStatusCode = dataRow["HttpStatusCode"] is DBNull ? null : (short)dataRow["HttpStatusCode"];
                int counter = (int)dataRow["Counter"];
                string key = StatisticsKey.HttpStatusCode + "_" + (httpStatusCode?.ToString() ?? "NULL");
                Insert(date, key, counter);
            }
        }

        public static List<string> GetHttpStatusCodeKeys()
        {
            return GetKeysLike(StatisticsKey.HttpStatusCode);
        }

        public static List<string> GetPageTypeKeys()
        {
            return GetKeysLike(StatisticsKey.PageType);
        }

        public static List<string> GetKeysLike(StatisticsKey key)
        {
            string query =
                "SELECT DISTINCT [Key] " +
                "FROM " + TABLE_STATISTICS_SNAPSHOT + " " +
                "WHERE [Key] LIKE @Key";

            return new DataBase().QueryListString(query, new Dictionary<string, object?>
            {
                { "Key", key + "%" }
            });
        }

        public static void PageType()
        {
            SnapshotPageType(-1);
        }

        public static void SnapshotPageType7Days()
        {
            for (var days = -7; days <= -1; days++)
            {
                SnapshotPageType(days);
            }
        }

        public static void SnapshotPageType(int days)
        {
            DateTime date = DateTime.Today.AddDays(days);

            DeleteByKeyPrefixAndDate(date, StatisticsKey.PageType.ToString());

            string query =
                "SELECT [PageType], COUNT(*) AS [Counter] " +
                "FROM " + landerist_library.Pages.Pages.PAGES + " " +
                "WHERE CAST([Updated] AS date) = CAST(@Date AS date) " +
                "AND [PageType] IS NOT NULL " +
                "GROUP BY [PageType] ";

            var dataTable = new DataBase().QueryTable(query, new Dictionary<string, object?>
            {
                { "Date", date }
            });

            foreach (DataRow dataRow in dataTable.Rows)
            {
                string pageType = (string)dataRow["PageType"];
                int counter = (int)dataRow["Counter"];
                string key = StatisticsKey.PageType + "_" + pageType;
                Insert(date, key, counter);
            }
        }

        private static bool InsertDaily(StatisticsKey key, string queryInt)
        {
            int counter = new DataBase().QueryInt(queryInt);
            return Insert(DateTime.Now, key.ToString(), counter);
        }

        private static bool InsertDaily(StatisticsKey key, string queryInt, Dictionary<string, object?> parameters)
        {
            int counter = QueryInt(queryInt, parameters);
            return Insert(DateTime.Now, key.ToString(), counter);
        }

        private static int QueryInt(string query, Dictionary<string, object?> parameters)
        {
            DataTable dataTable = new DataBase().QueryTable(query, parameters);
            return Convert.ToInt32(dataTable.Rows[0][0]);
        }

        private static bool DeleteByKeyPrefixAndDate(DateTime date, string keyPrefix)
        {
            string query =
                "DELETE FROM " + TABLE_STATISTICS_SNAPSHOT + " " +
                "WHERE [Key] LIKE @KeyPrefix " +
                "AND CAST([Date] AS date) = CAST(@Date AS date)";

            return new DataBase().Query(query, new Dictionary<string, object?>
            {
                { "Date", date },
                { "KeyPrefix", keyPrefix + "_%" }
            });
        }

        private static bool Insert(DateTime date, string key, int counter)
        {
            string query =
                "DELETE FROM " + TABLE_STATISTICS_SNAPSHOT + " " +
                "WHERE [Key] = @Key AND CAST([Date] AS date) = CAST(@Date AS date); " +
                "INSERT INTO " + TABLE_STATISTICS_SNAPSHOT + " ([Date], [Key], [Counter]) " +
                "VALUES (@Date, @Key, @Counter);";

            return new DataBase().Query(query, new Dictionary<string, object?>
            {
                { "Date", date },
                { "Key", key },
                { "Counter", counter }
            });
        }

        public static bool InsertDailyCounter(StatisticsKey key)
        {
            return InsertDailyCounter(key.ToString());
        }

        public static bool InsertDailyCounter(string key)
        {
            return InsertDailyCounter(key, 1);
        }

        public static bool InsertDailyCounter(StatisticsKey key, int counter)
        {
            return InsertDailyCounter(key.ToString(), counter);
        }

        public static bool InsertDailyCounter(string key, int counter)
        {
            if (Configuration.Config.IsConfigurationLocal())
            {
                return true;
            }

            string query =
                "MERGE " + TABLE_STATISTICS_SNAPSHOT + " AS target " +
                "USING (" +
                "   SELECT " +
                "       CAST(@Date AS DATE) AS DateOnly, " +
                "       @Key AS [Key], " +
                "       @Counter AS [Counter] " +
                "   ) AS source " +
                "ON " +
                "   CAST(target.[Date] AS DATE) = source.DateOnly " +
                "   AND target.[Key] = source.[Key] " +
                "WHEN MATCHED THEN " +
                "   UPDATE SET target.[Counter] = target.[Counter] + source.[Counter] " +
                "WHEN NOT MATCHED THEN " +
                "   INSERT ([Date], [Key], [Counter]) " +
                "   VALUES (source.DateOnly, source.[Key], source.[Counter]);";

            return new DataBase().Query(query, new Dictionary<string, object?>
            {
                { "Date", DateTime.Now },
                { "Key", key },
                { "Counter", counter }
            });
        }

        public static DataSet GetStatistics(int lastMonths)
        {
            string query =
                "SELECT DISTINCT [Key] " +
                "FROM " + TABLE_STATISTICS_SNAPSHOT + " ";

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
                "FROM " + TABLE_STATISTICS_SNAPSHOT + " " +
                "WHERE [Key] = @Key AND " +
                "[Date] > DATEADD(MONTH, @Months, GETDATE()) " +
                "ORDER BY [Date] ASC";

            return new DataBase().QueryTable(query, new Dictionary<string, object?>
            {
                { "Key", key },
                { "Months", months }
            });
        }

        public static DataTable GetLatestStatistics(string statisticsKey, int top)
        {
            string query =
                "SELECT TOP (@Top) [Date], [Counter] " +
                "FROM " + TABLE_STATISTICS_SNAPSHOT + " " +
                "WHERE [Key] = @Key " +
                "ORDER BY [Date] DESC";

            return new DataBase().QueryTable(query, new Dictionary<string, object?>
            {
                { "Top", top },
                { "Key", statisticsKey }
            });
        }
    }
}
