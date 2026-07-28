using landerist_library.Infrastructure.Statistics;
using landerist_library.Application.Scraping;
using landerist_library.Database;
using landerist_library.Infrastructure.Sql;
using landerist_library.Application.Statistics;

namespace landerist_library.Infrastructure.Scraping;

public sealed class SqlScrapeBatchMetrics : IScrapeBatchMetrics
{
    private readonly GlobalStatisticsRepository _statistics;

    public SqlScrapeBatchMetrics(IDatabase database)
    {
        _statistics = new GlobalStatisticsRepository(database);
    }

    public void Record(ScrapeBatchCounters counters)
    {
        Insert(StatisticsKey.Processed, counters.Processed);
        Insert(StatisticsKey.ScrapedSuccess, counters.ScrapedSuccess);
        Insert(StatisticsKey.ScrapedCrashed, counters.Crashed);
        Insert(StatisticsKey.ScrapedHttpStatusCodeNotOK, counters.DownloadErrors);
    }

    private void Insert(StatisticsKey key, int count) =>
        _statistics.InsertDailyCounter(key.ToString(), count);
}
