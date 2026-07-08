using landerist_library.Infrastructure.Sql;
using landerist_orels.ES;
using System.Data;

namespace landerist_library.Statistics
{
    public enum HostStatisticsKey
    {
        Pages,
        Inserted,
        LastScrape,
        Listings,
        PublishedListings,
        UnpublishedListings,
        HttpStatusCode,
        PageType,
        NotListingCache,
        ListingParserInputAlreadyParsed,
        ListingParserInputIsAnotherListingInHost,
        PageNotModified,
        ParseListingRetryNotListing,
    }

    public static class HostStatistics
    {
        public const string HOST_STATISTICS = "[HOST_STATISTICS]";
        private static readonly HostStatisticsRepository Repository = new();

        public static void TakeSnapshots()
        {
            foreach (var website in Websites.Websites.GetAll())
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
            Inserted(host);
            LastScrape(host);
            Listings(host);
            PublishedListings(host);
            UnpublishedListings(host);
            HttpStatusCode(host);
            PageType(host);
        }

        private static void Pages(string host)
        {
            InsertDaily(host, HostStatisticsKey.Pages, Repository.CountPages(host));
        }

        private static void Inserted(string host)
        {
            InsertDaily(host, HostStatisticsKey.Inserted, Repository.CountInsertedYesterday(host));
        }

        private static void LastScrape(string host)
        {
            InsertDaily(host, HostStatisticsKey.LastScrape, Repository.CountLastScrapeYesterday(host));
        }

        private static void Listings(string host)
        {
            InsertDaily(host, HostStatisticsKey.Listings, Repository.CountListings(host));
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
            InsertDaily(host, statisticsKey, Repository.CountListings(host, listingStatus));
        }

        private static void HttpStatusCode(string host)
        {
            DateTime date = DateTime.Now;
            Repository.DeleteByHostKeyPrefixAndDate(date, host, HostStatisticsKey.HttpStatusCode.ToString());

            foreach (DataRow dataRow in Repository.GetHttpStatusCodeCounts(host).Rows)
            {
                short? httpStatusCode = dataRow["HttpStatusCode"] is DBNull ? null : (short)dataRow["HttpStatusCode"];
                int counter = Convert.ToInt32(dataRow["Counter"]);
                string key = HostStatisticsKey.HttpStatusCode + "_" + (httpStatusCode?.ToString() ?? "NULL");
                Repository.Insert(date, host, key, counter);
            }
        }

        private static void PageType(string host)
        {
            DateTime date = DateTime.Now;
            Repository.DeleteByHostKeyPrefixAndDate(date, host, HostStatisticsKey.PageType.ToString());

            foreach (DataRow dataRow in Repository.GetPageTypeCounts(host).Rows)
            {
                string pageType = (string)dataRow["PageType"];
                int counter = Convert.ToInt32(dataRow["Counter"]);
                string key = HostStatisticsKey.PageType + "_" + pageType;
                Repository.Insert(date, host, key, counter);
            }
        }

        private static bool InsertDaily(string host, HostStatisticsKey key, int counter)
        {
            return Repository.Insert(DateTime.Now, host, key.ToString(), counter);
        }

        public static bool InsertDailyCounter(string host, HostStatisticsKey key)
        {
            return InsertDailyCounter(host, key.ToString());
        }

        public static bool InsertDailyCounter(string host, string key)
        {
            return InsertDailyCounter(host, key, 1);
        }

        public static bool InsertDailyCounter(string host, HostStatisticsKey key, int counter)
        {
            return InsertDailyCounter(host, key.ToString(), counter);
        }

        public static bool InsertDailyCounter(string host, string key, int counter)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(host);
            ArgumentException.ThrowIfNullOrWhiteSpace(key);

            if (Configuration.Config.IsConfigurationLocal())
            {
                return true;
            }

            return Repository.InsertDailyCounter(host, key, counter);
        }

        public static DataTable GetLatestStatistics(string host, string statisticsKey, int top)
        {
            return Repository.GetLatestStatistics(host, statisticsKey, top);
        }

        public static DataTable GetLatestStatisticsByPrefix(string host, string keyPrefix)
        {
            return Repository.GetLatestStatisticsByPrefix(host, keyPrefix);
        }

        public static DataTable GetPagesByPageType(string host)
        {
            return Repository.GetPagesByPageType(host);
        }

        public static DataTable GetPagesByHttpStatusCode(string host)
        {
            return Repository.GetPagesByHttpStatusCode(host);
        }

        public static DataTable GetPagesByNextScrape(string host)
        {
            return Repository.GetPagesByNextScrape(host);
        }

        public static DataTable GetPublishedListingsByOperation(string host)
        {
            return Repository.GetPublishedListingsByOperation(host);
        }

        public static DataTable GetPublishedListingsByPropertyType(string host)
        {
            return Repository.GetPublishedListingsByPropertyType(host);
        }

        public static DataTable GetListingsByLocationResolver(string host)
        {
            return Repository.GetListingsByLocationResolver(host);
        }

        public static DataTable GetUnpublishedListingsByUnlistingReason(string host)
        {
            return Repository.GetUnpublishedListingsByUnlistingReason(host);
        }

        public static List<string> GetKeysLike(string host, HostStatisticsKey key)
        {
            return Repository.GetKeysLike(host, key);
        }

        public static DateTime? GetLatestDate(string host)
        {
            return Repository.GetLatestDate(host);
        }
    }
}
