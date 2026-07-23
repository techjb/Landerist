using landerist_library.Application.Scraping;
using landerist_library.Database;
using landerist_library.Infrastructure.Sql;
using landerist_library.Pages;
using landerist_library.Statistics;

namespace landerist_library.Infrastructure.Scraping;

public sealed class SqlPageClassificationMetrics : IPageClassificationMetrics
{
    private readonly GlobalStatisticsRepository _global;
    private readonly HostStatisticsRepository _host;

    public SqlPageClassificationMetrics(IDatabase database)
    {
        ArgumentNullException.ThrowIfNull(database);
        _global = new GlobalStatisticsRepository(database);
        _host = new HostStatisticsRepository(database);
    }

    public void RecordPageNotModified(Page page) =>
        Record(page, StatisticsKey.PageNotModified, HostStatisticsKey.PageNotModified);

    public void RecordNotListingCache(Page page) =>
        Record(page, StatisticsKey.NotListingCache, HostStatisticsKey.NotListingCache);

    public void RecordListingInputAlreadyParsed(Page page) =>
        Record(page, StatisticsKey.ListingParserInputAlreadyParsed, HostStatisticsKey.ListingParserInputAlreadyParsed);

    private void Record(Page page, StatisticsKey globalKey, HostStatisticsKey hostKey)
    {
        _global.InsertDailyCounter(globalKey.ToString(), 1);
        _host.InsertDailyCounter(page.Host, hostKey.ToString(), 1);
    }
}
