using landerist_orels.ES;
using System.Data;

namespace landerist_library.Application.Statistics
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

    public sealed class GlobalStatistics
    {
        private readonly IGlobalStatisticsRepository Repository;
        private readonly bool _persistenceEnabled;

        public GlobalStatistics(IGlobalStatisticsRepository repository, bool persistenceEnabled)
        {
            ArgumentNullException.ThrowIfNull(repository);
            Repository = repository;
            _persistenceEnabled = persistenceEnabled;
        }

        public void TakeSnapshots()
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

        private void Websites()
        {
            InsertDaily(StatisticsKey.Websites, Repository.CountWebsites());
        }

        private void UpdatedRobotsTxt()
        {
            InsertDaily(StatisticsKey.UpdatedRobotsTxt, Repository.CountUpdatedRobotsTxtYesterday());
        }

        private void UpdatedSitemaps()
        {
            InsertDaily(StatisticsKey.UpdatedSitemaps, Repository.CountUpdatedSitemapsYesterday());
        }

        private void UpdatedIpAddress()
        {
            InsertDaily(StatisticsKey.UpdatedIpAddress, Repository.CountUpdatedIpAddressYesterday());
        }

        private void Pages()
        {
            InsertDaily(StatisticsKey.Pages, Repository.CountPages());
        }

        private void LastScrapePages()
        {
            InsertDaily(StatisticsKey.LastScrapePages, Repository.CountLastScrapePagesYesterday());
        }

        private void NeedUpdate()
        {
            InsertDaily(StatisticsKey.NeedUpdate, Repository.CountNeedUpdatePages());
        }

        private void WaitingAIRequest()
        {
            InsertDaily(StatisticsKey.WaitingAIRequest, Repository.CountWaitingAIRequestPages());
        }

        private void UnknownPageType()
        {
            InsertDaily(StatisticsKey.UnknownPageType, Repository.CountUnknownPageTypePages());
        }

        private void Listings()
        {
            InsertDaily(StatisticsKey.Listings, Repository.CountListings());
        }

        private void PublishedListings()
        {
            SnapshotListings(StatisticsKey.PublishedListings, ListingStatus.published);
        }

        private void UnPublishedListings()
        {
            SnapshotListings(StatisticsKey.UnpublishedListings, ListingStatus.unpublished);
        }

        private void SnapshotListings(StatisticsKey statisticsKey, ListingStatus listingStatus)
        {
            InsertDaily(statisticsKey, Repository.CountListings(listingStatus));
        }

        private void Media()
        {
            InsertDaily(StatisticsKey.Media, Repository.CountMedia());
        }

        public void SnapshotHttpStatusCode7Days()
        {
            for (var days = -7; days <= -1; days++)
            {
                SnapshotHttpStatusCode(days);
            }
        }

        public void HttpStatusCode()
        {
            SnapshotHttpStatusCode(-1);
        }

        public void SnapshotHttpStatusCode(int days)
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

        public List<string> GetHttpStatusCodeKeys()
        {
            return GetKeysLike(StatisticsKey.HttpStatusCode);
        }

        public List<string> GetPageTypeKeys()
        {
            return GetKeysLike(StatisticsKey.PageType);
        }

        public List<string> GetKeysLike(StatisticsKey key)
        {
            return Repository.GetKeysLike(key);
        }

        public void PageType()
        {
            SnapshotPageType(-1);
        }

        public void SnapshotPageType7Days()
        {
            for (var days = -7; days <= -1; days++)
            {
                SnapshotPageType(days);
            }
        }

        public void SnapshotPageType(int days)
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

        private bool InsertDaily(StatisticsKey key, int counter)
        {
            return Repository.Insert(DateTime.Now, key.ToString(), counter);
        }

        public bool InsertDailyCounter(StatisticsKey key)
        {
            return InsertDailyCounter(key.ToString());
        }

        public bool InsertDailyCounter(string key)
        {
            return InsertDailyCounter(key, 1);
        }

        public bool InsertDailyCounter(StatisticsKey key, int counter)
        {
            return InsertDailyCounter(key.ToString(), counter);
        }

        public bool InsertDailyCounter(string key, int counter)
        {
            if (!_persistenceEnabled)
            {
                return true;
            }

            return Repository.InsertDailyCounter(key, counter);
        }

        public DataSet GetStatistics(int lastMonths)
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

        private DataTable GetStatistics(string key, int months)
        {
            return Repository.GetStatistics(key, months);
        }

        public DataTable GetLatestStatistics(string statisticsKey, int top)
        {
            return Repository.GetLatestStatistics(statisticsKey, top);
        }
    }
}
