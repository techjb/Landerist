using landerist_library.Application.Scraping;
using landerist_library.Statistics;

namespace landerist_library.Infrastructure.Scraping;

public sealed class LegacyScrapeBatchMetrics : IScrapeBatchMetrics
{
    public void Record(ScrapeBatchCounters counters)
    {
        GlobalStatistics.InsertDailyCounter(StatisticsKey.Processed, counters.Processed);
        GlobalStatistics.InsertDailyCounter(StatisticsKey.ScrapedSuccess, counters.ScrapedSuccess);
        GlobalStatistics.InsertDailyCounter(StatisticsKey.ScrapedCrashed, counters.Crashed);
        GlobalStatistics.InsertDailyCounter(
            StatisticsKey.ScrapedHttpStatusCodeNotOK,
            counters.DownloadErrors);
    }
}
