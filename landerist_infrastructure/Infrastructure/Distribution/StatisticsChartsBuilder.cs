using landerist_library.Application.Statistics;
using landerist_library.Infrastructure.Sql;
using landerist_orels.ES;

namespace landerist_library.Infrastructure.Distribution;

internal sealed class StatisticsChartsBuilder
{
    private readonly GlobalStatistics _statistics;
    private readonly PageStatisticsRepository _pageStatistics;
    private readonly StatisticsChartFormatter _formatter;
    private readonly List<string> _charts = [];

    public StatisticsChartsBuilder(
        GlobalStatistics statistics,
        PageStatisticsRepository pageStatistics)
    {
        _statistics = statistics;
        _pageStatistics = pageStatistics;
        _formatter = new StatisticsChartFormatter(statistics);
    }

    public IReadOnlyList<string> Build()
    {
        _charts.Clear();
        BarTimeSeries("Processed Pages", [StatisticsKey.Processed]);
        BarTimeSeries("Last Scrape Pages", [StatisticsKey.LastScrapePages]);
        BarDictionary(
            "Next Scrape Distribution",
            "NextScrape",
            _pageStatistics.GroupByNextScrape());
        BarTimeSeries("Last Scrape by HttpStatusCode", _statistics.GetHttpStatusCodeKeys());
        BarTimeSeries("Last Scrape by PageType", _statistics.GetPageTypeKeys());
        BarTimeSeries(
            "Scraper Success/Chrash",
            [StatisticsKey.ScrapedSuccess, StatisticsKey.ScrapedCrashed]);
        BarTimeSeries("Hit Not Listing Cache", [StatisticsKey.NotListingCache]);
        BarTimeSeries(
            "Page conditional headers check",
            [StatisticsKey.PageConditionalHeadersCheck]);
        BarTimeSeries("Page not modified", [StatisticsKey.PageNotModified]);
        BarTimeSeries(
            "ListingParserInput already parsed",
            [StatisticsKey.ListingParserInputAlreadyParsed]);
        BarTimeSeries(
            "ListingParserInput is another listing in host",
            [StatisticsKey.ListingParserInputIsAnotherListingInHost]);
        BarTimeSeries(
            "AI Batch Readed",
            [StatisticsKey.BatchReaded, StatisticsKey.BatchReadedErrors]);
        BarTimeSeries(
            "LocalAI Parsing",
            [StatisticsKey.LocalAIParsingSuccess, StatisticsKey.LocalAIParsingErrors]);
        BarTimeSeries(
            "Listing Insert/Update",
            [StatisticsKey.ListingInsert, StatisticsKey.ListingUpdate]);
        PieDictionary("PageType", _pageStatistics.GroupByPageType());
        BarDictionary(
            "Published Listings PageType",
            "published",
            _pageStatistics.GroupByPageType(ListingStatus.published));
        BarDictionary(
            "Unpublished Listings PageType",
            "unpublished",
            _pageStatistics.GroupByPageType(ListingStatus.unpublished));
        BarDictionary(
            "Unpublished Listings HttpStatusCode",
            "unpublished",
            _pageStatistics.GroupByHttpStatusCode(ListingStatus.unpublished));
        PieDictionary("HttpStatusCode", _pageStatistics.CountByHttpStatusCode());
        return _charts.ToList();
    }

    private void BarTimeSeries(string title, IEnumerable<StatisticsKey> keys) =>
        Add("BarChart", title, _formatter.TimeSeries(keys, yesterday: false));

    private void BarTimeSeries(string title, IEnumerable<string> keys) =>
        Add("BarChart", title, _formatter.TimeSeries(keys, yesterday: false));

    private void BarDictionary(
        string title,
        string label,
        Dictionary<string, object?> values) =>
        Add("BarChart", title, StatisticsChartFormatter.LabeledValues(label, values));

    private void PieDictionary(
        string title,
        Dictionary<string, object?> values) =>
        Add("PieChart", title, StatisticsChartFormatter.DictionaryValues(values));

    private void Add(string chartType, string title, string data) =>
        _charts.Add(StatisticsChartFormatter.Chart(chartType, title, data));
}
