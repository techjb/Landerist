using landerist_library.Infrastructure.Sql;
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
        LastScrapePages,
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
        Processed,
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
        ListingParserInputAlreadyParsed,
        ListingParserInputIsAnotherListingInHost,
        PageConditionalHeadersCheck,
        PageNotModified,
    }

    public class GlobalStatistics
    {
        private static readonly GlobalStatisticsRepository Repository = new(global::landerist_library.Database.LegacyDatabase.Create());

        public static void TakeSnapshots()
        {
            Websites();
            UpdatedRobotsTxt();
            UpdatedSitemaps();
            UpdatedIpAddress();
            Pages();
            LastScrapePages();
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
            InsertDaily(StatisticsKey.Websites, Repository.CountWebsites());
        }

        private static void UpdatedRobotsTxt()
        {
            InsertDaily(StatisticsKey.UpdatedRobotsTxt, Repository.CountUpdatedRobotsTxtYesterday());
        }

        private static void UpdatedSitemaps()
        {
            InsertDaily(StatisticsKey.UpdatedSitemaps, Repository.CountUpdatedSitemapsYesterday());
        }

        private static void UpdatedIpAddress()
        {
            InsertDaily(StatisticsKey.UpdatedIpAddress, Repository.CountUpdatedIpAddressYesterday());
        }

        private static void Pages()
        {
            InsertDaily(StatisticsKey.Pages, Repository.CountPages());
        }

        private static void LastScrapePages()
        {
            InsertDaily(StatisticsKey.LastScrapePages, Repository.CountLastScrapePagesYesterday());
        }

        private static void NeedUpdate()
        {
            InsertDaily(StatisticsKey.NeedUpdate, Repository.CountNeedUpdatePages());
        }

        private static void WaitingAIRequest()
        {
            InsertDaily(StatisticsKey.WaitingAIRequest, Repository.CountWaitingAIRequestPages());
        }

        private static void UnknownPageType()
        {
            InsertDaily(StatisticsKey.UnknownPageType, Repository.CountUnknownPageTypePages());
        }

        private static void Listings()
        {
            InsertDaily(StatisticsKey.Listings, Repository.CountListings());
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
            InsertDaily(statisticsKey, Repository.CountListings(listingStatus));
        }

        private static void Media()
        {
            InsertDaily(StatisticsKey.Media, Repository.CountMedia());
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
            Repository.DeleteByKeyPrefixAndDate(date, StatisticsKey.HttpStatusCode.ToString());

            foreach (DataRow dataRow in Repository.GetHttpStatusCodeCounts(date).Rows)
            {
                short? httpStatusCode = dataRow["HttpStatusCode"] is DBNull ? null : (short)dataRow["HttpStatusCode"];
                int counter = (int)dataRow["Counter"];
                string key = StatisticsKey.HttpStatusCode + "_" + (httpStatusCode?.ToString() ?? "NULL");
                Repository.Insert(date, key, counter);
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
            return Repository.GetKeysLike(key);
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
            Repository.DeleteByKeyPrefixAndDate(date, StatisticsKey.PageType.ToString());

            foreach (DataRow dataRow in Repository.GetPageTypeCounts(date).Rows)
            {
                string pageType = (string)dataRow["PageType"];
                int counter = (int)dataRow["Counter"];
                string key = StatisticsKey.PageType + "_" + pageType;
                Repository.Insert(date, key, counter);
            }
        }

        private static bool InsertDaily(StatisticsKey key, int counter)
        {
            return Repository.Insert(DateTime.Now, key.ToString(), counter);
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

            return Repository.InsertDailyCounter(key, counter);
        }

        public static DataSet GetStatistics(int lastMonths)
        {
            DataTable dataTable = Repository.GetStatisticsKeys();

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
            return Repository.GetStatistics(key, months);
        }

        public static DataTable GetLatestStatistics(string statisticsKey, int top)
        {
            return Repository.GetLatestStatistics(statisticsKey, top);
        }
    }
}
