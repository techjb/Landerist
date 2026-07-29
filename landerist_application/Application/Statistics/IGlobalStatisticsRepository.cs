using landerist_orels.ES;
using System.Data;

namespace landerist_library.Application.Statistics;

public interface IGlobalStatisticsRepository
{
    int CountWebsites();
    int CountUpdatedRobotsTxtYesterday();
    int CountUpdatedSitemapsYesterday();
    int CountUpdatedIpAddressYesterday();
    int CountPages();
    int CountLastScrapePagesYesterday();
    int CountNeedUpdatePages();
    int CountWaitingAIRequestPages();
    int CountUnknownPageTypePages();
    int CountListings();
    int CountListings(ListingStatus listingStatus);
    int CountMedia();
    DataTable GetHttpStatusCodeCounts(DateTime date);
    DataTable GetPageTypeCounts(DateTime date);
    List<string> GetKeysLike(StatisticsKey key);
    bool DeleteByKeyPrefixAndDate(DateTime date, string keyPrefix);
    bool Insert(DateTime date, string key, int counter);
    bool InsertDailyCounter(string key, int counter);
    Task<bool> InsertDailyCounterAsync(
        string key,
        int counter,
        CancellationToken cancellationToken = default);
    DataTable GetStatisticsKeys();
    DataTable GetStatistics(string key, int months);
    DataTable GetLatestStatistics(string statisticsKey, int top);
}
