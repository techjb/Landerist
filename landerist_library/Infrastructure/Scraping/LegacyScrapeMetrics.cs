using landerist_library.Application.Scraping;
using landerist_library.Pages;
using landerist_library.Statistics;

namespace landerist_library.Infrastructure.Scraping;

public sealed class LegacyScrapeMetrics : IScrapeMetrics
{
    public void RecordConditionalHeaderCheck() =>
        GlobalStatistics.InsertDailyCounter(StatisticsKey.PageConditionalHeadersCheck);

    public void RecordPageNotModified(Page page)
    {
        GlobalStatistics.InsertDailyCounter(StatisticsKey.PageNotModified);
        HostStatistics.InsertDailyCounter(page.Host, HostStatisticsKey.PageNotModified);
    }
}
