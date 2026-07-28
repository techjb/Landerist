using landerist_library.Infrastructure.Statistics;
using landerist_library.Application.Scraping;
using landerist_library.Database;
using landerist_library.Infrastructure.Sql;
using landerist_library.Pages;
using landerist_library.Application.Statistics;

namespace landerist_library.Infrastructure.Scraping;

public sealed class SqlScrapeMetrics : IScrapeMetrics
{
    private readonly GlobalStatisticsRepository _globalStatistics;
    private readonly HostStatisticsRepository _hostStatistics;

    public SqlScrapeMetrics(IDatabase database)
    {
        ArgumentNullException.ThrowIfNull(database);
        _globalStatistics = new GlobalStatisticsRepository(database);
        _hostStatistics = new HostStatisticsRepository(database);
    }

    public void RecordConditionalHeaderCheck() =>
        _globalStatistics.InsertDailyCounter(StatisticsKey.PageConditionalHeadersCheck.ToString(), 1);

    public void RecordPageNotModified(Page page)
    {
        _globalStatistics.InsertDailyCounter(StatisticsKey.PageNotModified.ToString(), 1);
        _hostStatistics.InsertDailyCounter(page.Host, HostStatisticsKey.PageNotModified.ToString(), 1);
    }
}
