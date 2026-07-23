using landerist_library.Application.Websites;
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

    public sealed class HostStatistics
    {
        private readonly HostStatisticsRepository Repository;
        private readonly IWebsiteCatalog WebsiteCatalog;

        public HostStatistics(
            HostStatisticsRepository repository,
            IWebsiteCatalog websiteCatalog)
        {
            ArgumentNullException.ThrowIfNull(repository);
            ArgumentNullException.ThrowIfNull(websiteCatalog);
            Repository = repository;
            WebsiteCatalog = websiteCatalog;
        }

        public void TakeSnapshots()
        {
            foreach (string host in WebsiteCatalog.GetHosts())
            {
                TakeSnapshot(host);
            }
        }

        public void TakeSnapshot(string host)
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

        private void Pages(string host)
        {
            InsertDaily(host, HostStatisticsKey.Pages, Repository.CountPages(host));
        }

        private void Inserted(string host)
        {
            InsertDaily(host, HostStatisticsKey.Inserted, Repository.CountInsertedYesterday(host));
        }

        private void LastScrape(string host)
        {
            InsertDaily(host, HostStatisticsKey.LastScrape, Repository.CountLastScrapeYesterday(host));
        }

        private void Listings(string host)
        {
            InsertDaily(host, HostStatisticsKey.Listings, Repository.CountListings(host));
        }

        private void PublishedListings(string host)
        {
            SnapshotListings(host, HostStatisticsKey.PublishedListings, ListingStatus.published);
        }

        private void UnpublishedListings(string host)
        {
            SnapshotListings(host, HostStatisticsKey.UnpublishedListings, ListingStatus.unpublished);
        }

        private void SnapshotListings(string host, HostStatisticsKey statisticsKey, ListingStatus listingStatus)
        {
            InsertDaily(host, statisticsKey, Repository.CountListings(host, listingStatus));
        }

        private void HttpStatusCode(string host)
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

        private void PageType(string host)
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

        private bool InsertDaily(string host, HostStatisticsKey key, int counter)
        {
            return Repository.Insert(DateTime.Now, host, key.ToString(), counter);
        }

        public bool InsertDailyCounter(string host, HostStatisticsKey key)
        {
            return InsertDailyCounter(host, key.ToString());
        }

        public bool InsertDailyCounter(string host, string key)
        {
            return InsertDailyCounter(host, key, 1);
        }

        public bool InsertDailyCounter(string host, HostStatisticsKey key, int counter)
        {
            return InsertDailyCounter(host, key.ToString(), counter);
        }

        public bool InsertDailyCounter(string host, string key, int counter)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(host);
            ArgumentException.ThrowIfNullOrWhiteSpace(key);

            if (Configuration.Config.IsConfigurationLocal())
            {
                return true;
            }

            return Repository.InsertDailyCounter(host, key, counter);
        }

        public DataTable GetLatestStatistics(string host, string statisticsKey, int top)
        {
            return Repository.GetLatestStatistics(host, statisticsKey, top);
        }

        public DataTable GetLatestStatisticsByPrefix(string host, string keyPrefix)
        {
            return Repository.GetLatestStatisticsByPrefix(host, keyPrefix);
        }

        public DataTable GetPagesByPageType(string host)
        {
            return Repository.GetPagesByPageType(host);
        }

        public DataTable GetPagesByHttpStatusCode(string host)
        {
            return Repository.GetPagesByHttpStatusCode(host);
        }

        public DataTable GetPagesByNextScrape(string host)
        {
            return Repository.GetPagesByNextScrape(host);
        }

        public DataTable GetPublishedListingsByOperation(string host)
        {
            return Repository.GetPublishedListingsByOperation(host);
        }

        public DataTable GetPublishedListingsByPropertyType(string host)
        {
            return Repository.GetPublishedListingsByPropertyType(host);
        }

        public DataTable GetListingsByLocationResolver(string host)
        {
            return Repository.GetListingsByLocationResolver(host);
        }

        public DataTable GetUnpublishedListingsByUnlistingReason(string host)
        {
            return Repository.GetUnpublishedListingsByUnlistingReason(host);
        }

        public List<string> GetKeysLike(string host, HostStatisticsKey key)
        {
            return Repository.GetKeysLike(host, key);
        }

        public DateTime? GetLatestDate(string host)
        {
            return Repository.GetLatestDate(host);
        }
    }
}
