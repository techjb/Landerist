using landerist_orels.ES;
using System.Data;

namespace landerist_library.Application.Statistics;

public interface IHostStatisticsRepository
{
    int CountPages(string host);
    int CountInsertedYesterday(string host);
    int CountLastScrapeYesterday(string host);
    int CountListings(string host);
    int CountListings(string host, ListingStatus listingStatus);
    DataTable GetHttpStatusCodeCounts(string host);
    DataTable GetPageTypeCounts(string host);
    bool DeleteByHostKeyPrefixAndDate(DateTime date, string host, string keyPrefix);
    bool Insert(DateTime date, string host, string key, int counter);
    bool InsertDailyCounter(string host, string key, int counter);
    DataTable GetLatestStatistics(string host, string statisticsKey, int top);
    DataTable GetLatestStatisticsByPrefix(string host, string keyPrefix);
    DataTable GetPagesByPageType(string host);
    DataTable GetPagesByHttpStatusCode(string host);
    DataTable GetPagesByNextScrape(string host);
    DataTable GetPublishedListingsByOperation(string host);
    DataTable GetPublishedListingsByPropertyType(string host);
    DataTable GetListingsByLocationResolver(string host);
    DataTable GetUnpublishedListingsByUnlistingReason(string host);
    List<string> GetKeysLike(string host, HostStatisticsKey key);
    DateTime? GetLatestDate(string host);
}
