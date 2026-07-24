using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Export;
using landerist_library.Logs;
using landerist_library.Infrastructure.Sql;
using landerist_library.Statistics;
using landerist_orels.ES;
using System.Data;
using System.Globalization;
using System.Text.Json;

namespace landerist_library.Infrastructure.Distribution
{
    public sealed class StatisticsPage : DistributionArtifacts
    {
        private readonly GlobalStatistics _statistics;
        private readonly PageStatisticsRepository _pageStatistics;

        public StatisticsPage(
            GlobalStatistics statistics,
            PageStatisticsRepository pageStatistics)
        {
            ArgumentNullException.ThrowIfNull(statistics);
            ArgumentNullException.ThrowIfNull(pageStatistics);
            _statistics = statistics;
            _pageStatistics = pageStatistics;
        }

        private readonly string StatisticsTemplateHtmlFile =
            Path.Combine(Config.LANDERIST_COM_TEMPLATES!, "statistics", "statistics_template.html");

        private readonly string StatisticsHtmlFile =
            Path.Combine(Config.LANDERIST_COM_OUTPUT!, "statistics.html");


        private readonly List<string> Charts = [];

        private readonly CultureInfo SummaryCulture = CultureInfo.GetCultureInfo("es-ES");

        public void UpdateCharts()
        {
            try
            {
                Charts.Clear();

                ProcessedPages();
                LastScrapePages();
                NextScrapeDistribution();
                //LastScrapeHttpStatusCodeNull();
                //LastScrapeHttpStatusCode200();
                //LastScrapeHttpStatusCodeErrors();
                LastScrapeHttpStatusCode();
                LastScrapePageType();
                ScraperSuccessCrash();
                NotListingsCache();
                PageConditionalHeadersCheck();
                PageNotModified();
                ListingParserInputAlreadyParsed();
                ListingParserInputIsAnotherListingInHost();
                BatchReaded();
                LocalAIParsing();
                ListingInsertUpdate();
                PageType();
                PublishedPageType();
                UnPublishedPageType();
                UnPublishedHttpStatusCode();
                HttpStatusCode();

                if (UpdateStatisticsHtmlPage())
                {
                    Log.WriteInfo("StatisticsPage", "Updated statistics page");
                }
            }
            catch (Exception exception)
            {
                Log.WriteError("StatisticsPage AreaChart", exception);
            }
        }

        private void LastScrapePages()
        {
            BarChart("Last Scrape Pages", StatisticsKey.LastScrapePages, false);
        }

        private void NextScrapeDistribution()
        {
            var dictionary = _pageStatistics.GroupByNextScrape();
            BarChart("Next Scrape Distribution", "NextScrape", dictionary);
        }

        //private void LastScrapeHttpStatusCodeNull()
        //{
        //    LineChart("Last Scrape HttpStatusCode null", StatisticsKey.HttpStatusCode_NULL, true);
        //}

        //private void LastScrapeHttpStatusCode200()
        //{
        //    LineChart("Last Scrape HttpStatusCode 200", StatisticsKey.HttpStatusCode_200, true);
        //}
        //private void LastScrapeHttpStatusCodeErrors()
        //{
        //    var keys = _statistics.GetHttpStatusCodeKeys();
        //    keys.RemoveAll(code => code == StatisticsKey.HttpStatusCode_NULL.ToString() || code == StatisticsKey.HttpStatusCode_200.ToString());
        //    BarChart("Last Scrape HttpStatusCode errors", keys, false);
        //}

        private void LastScrapeHttpStatusCode()
        {
            var keys = _statistics.GetHttpStatusCodeKeys();
            BarChart("Last Scrape by HttpStatusCode", keys, false);
        }

        private void LastScrapePageType()
        {
            var keys = _statistics.GetPageTypeKeys();
            BarChart("Last Scrape by PageType", keys, false);
        }

        private void ProcessedPages()
        {
            BarChart("Processed Pages", StatisticsKey.Processed, false);
        }

        private void ScraperSuccessCrash()
        {
            List<StatisticsKey> statisticsKeys =
            [
                StatisticsKey.ScrapedSuccess,
                StatisticsKey.ScrapedCrashed,
            ];
            BarChart("Scraper Success/Chrash", statisticsKeys, false);
        }

        private void BatchReaded()
        {
            List<StatisticsKey> statisticsKeys =
            [
                StatisticsKey.BatchReaded,
                StatisticsKey.BatchReadedErrors,
            ];
            BarChart("AI Batch Readed", statisticsKeys, false);
        }

        private void LocalAIParsing()
        {
            List<StatisticsKey> statisticsKeys =
            [
                StatisticsKey.LocalAIParsingSuccess,
                StatisticsKey.LocalAIParsingErrors,
            ];
            BarChart("LocalAI Parsing", statisticsKeys, false);
        }

        private void NotListingsCache()
        {
            BarChart("Hit Not Listing Cache", StatisticsKey.NotListingCache, false);
        }

        private void ListingParserInputAlreadyParsed()
        {
            BarChart("ListingParserInput already parsed", StatisticsKey.ListingParserInputAlreadyParsed, false);
        }

        private void PageNotModified()
        {
            BarChart("Page not modified", StatisticsKey.PageNotModified, false);
        }

        private void PageConditionalHeadersCheck()
        {
            BarChart("Page conditional headers check", StatisticsKey.PageConditionalHeadersCheck, false);
        }

        private void ListingParserInputIsAnotherListingInHost()
        {
            BarChart("ListingParserInput is another listing in host", StatisticsKey.ListingParserInputIsAnotherListingInHost, false);
        }

        private void ListingInsertUpdate()
        {
            List<StatisticsKey> statisticsKeys =
            [
                StatisticsKey.ListingInsert,
                StatisticsKey.ListingUpdate,
            ];
            BarChart("Listing Insert/Update", statisticsKeys, false);
        }

        private void PageType()
        {
            var dictionary = _pageStatistics.GroupByPageType();
            PieChart("PageType", dictionary);
        }

        private void ListingsPageType()
        {
            var dictionaryPublished = _pageStatistics.GroupByPageType(ListingStatus.published);
            var dictionaryUnPublished = _pageStatistics.GroupByPageType(ListingStatus.unpublished);

            var published = GetLabelValues("published", dictionaryPublished);
            var unpublished = GetLabelValues("unpublished", dictionaryUnPublished);

            var data = new List<string>
            {
                published, unpublished
            };
            BarChart("Listings PageType", string.Join(",", [.. data]));
        }

        private void PublishedPageType()
        {
            var dictionary = _pageStatistics.GroupByPageType(ListingStatus.published);
            BarChart("Published Listings PageType", "published", dictionary);
        }

        private void UnPublishedPageType()
        {
            var dictionary = _pageStatistics.GroupByPageType(ListingStatus.unpublished);
            BarChart("Unpublished Listings PageType", "unpublished", dictionary);
        }

        private void UnPublishedHttpStatusCode()
        {
            var dictionary = _pageStatistics.GroupByHttpStatusCode(ListingStatus.unpublished);
            BarChart("Unpublished Listings HttpStatusCode", "unpublished", dictionary);
        }

        private void HttpStatusCode()
        {
            var dictionary = _pageStatistics.CountByHttpStatusCode();
            PieChart("HttpStatusCode", dictionary);
        }


        private void AreaChart(string title, StatisticsKey statisticKey, bool yesterday)
        {
            var keys = new List<StatisticsKey> { statisticKey };
            AreaChart(title, keys, yesterday);
        }

        private void AreaChart(string title, List<StatisticsKey> keys, bool yesterday)
        {
            List<string> list = [.. keys.Select(key => key.ToString())];
            AreaChart(title, list, yesterday);
        }

        private void AreaChart(string title, List<string> keys, bool yesterday)
        {
            var dataString = GetDataString(keys, yesterday);
            AreaChart(title, dataString);
        }

        private void PieChart(string title, Dictionary<string, object?> dictionary)
        {
            string dataString = GetValues(dictionary);
            PieChart(title, dataString);
        }

        private void LineChart(string title, StatisticsKey statisticsKey, bool yesterday)
        {
            List<string> keys = [statisticsKey.ToString()];
            LineChart(title, keys, yesterday);
        }

        private void LineChart(string title, List<StatisticsKey> keys, bool yesterday)
        {
            List<string> list = [.. keys.Select(key => key.ToString())];
            LineChart(title, list, yesterday);
        }

        private void LineChart(string title, List<string> keys, bool yesterday)
        {
            var data = GetDataString(keys, yesterday);
            LineChart(title, data);
        }

        private void BarChart(string title, StatisticsKey statisticsKey, bool yesterday)
        {
            List<string> keys = [statisticsKey.ToString()];
            BarChart(title, keys, yesterday);
        }

        private void BarChart(string title, List<StatisticsKey> keys, bool yesterday)
        {
            List<string> list = [.. keys.Select(key => key.ToString())];
            BarChart(title, list, yesterday);
        }
        private void BarChart(string title, List<string> keys, bool yesterday)
        {
            var data = GetDataString(keys, yesterday);
            BarChart(title, data);
        }

        private void BarChart(string title, string key, Dictionary<string, object?> dictionary)
        {
            string dataString = GetLabelValues(key, dictionary);
            BarChart(title, dataString);
        }

        //private void BarChart(string title, List<StatisticsKey> keys, bool yesterday)
        //{
        //    List<string> list = [.. keys.SelectTop1(key => key.ToString())];
        //    BarChart(title, list, yesterday);
        //}

        private void AreaChart(string title, string data)
        {
            AddChart("AreaChart", title, data);
        }

        private void LineChart(string title, string data)
        {
            AddChart("LineChart", title, data);
        }

        private void BarChart(string title, string data)
        {
            AddChart("BarChart", title, data);
        }

        private void PieChart(string title, string data)
        {
            AddChart("PieChart", title, data);
        }

        private void AddChart(string charType, string title, string data)
        {
            var safeTitle = title.Replace("\\", "\\\\").Replace("'", "\\'");
            string chart = $"{charType}('{safeTitle}', [{data}])";
            Charts.Add(chart);
        }

        private string GetDataString(List<string> keys, bool yesterday)
        {
            List<string> data = [];
            foreach (var key in keys)
            {
                var values = GetValues(key, yesterday);
                var json = $"{{\"label\": {JsonSerializer.Serialize(key)}, \"values\":[{string.Join(",", values)}]}}";
                data.Add(json);
            }

            return string.Join(",", data);
        }

        private List<string> GetValues(string statisticKey, bool yesterday)
        {

            var dataTable = _statistics.GetLatestStatistics(statisticKey, 15);
            List<string> values = [];

            foreach (DataRow dataRow in dataTable.Rows.Cast<DataRow>().Reverse())
            {
                int counter = Convert.ToInt32(dataRow["Counter"]);
                var date = (DateTime)dataRow["Date"];

                if (yesterday)
                {
                    date = date.AddDays(-1);
                }

                var dateText = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                var json = $"{{\"key\": {JsonSerializer.Serialize(dateText)}, \"value\": {counter.ToString(CultureInfo.InvariantCulture)}}}";
                values.Add(json);
            }

            return values;
        }

        private string GetLabelValues(string key, Dictionary<string, object?> dictionary)
        {
            var values = GetValues(dictionary);
            return $"{{\"label\": {JsonSerializer.Serialize(key)}, \"values\":[{values}]}}";
        }

        private string GetValues(Dictionary<string, object?> dictionary)
        {
            List<string> data = [];

            foreach (var keyValuePair in dictionary)
            {
                string jsonValue = keyValuePair.Value switch
                {
                    null => "null",
                    IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture) ?? "null",
                    _ => JsonSerializer.Serialize(keyValuePair.Value)
                };

                var json = $"{{\"key\": {JsonSerializer.Serialize(keyValuePair.Key)}, \"value\": {jsonValue}}}";
                data.Add(json);
            }

            return string.Join(",", data);
        }

        private bool UpdateStatisticsHtmlPage()
        {
            var statisticsTemplate = File.ReadAllText(StatisticsTemplateHtmlFile);
            var charts = string.Join("; " + Environment.NewLine, Charts);
            statisticsTemplate = statisticsTemplate.Replace("/*SUMMARY_TABLE*/", GetSummaryTable());
            statisticsTemplate = statisticsTemplate.Replace("/*CHARTS*/", charts);

            File.WriteAllText(StatisticsHtmlFile, statisticsTemplate);
            return new S3().UploadToWebsiteBucket(StatisticsHtmlFile, "index.html", "statistics");
        }

        private string GetSummaryTable()
        {
            List<(string Label, StatisticsKey Key)> rows =
            [
                ("Websites", StatisticsKey.Websites),
                ("Pages", StatisticsKey.Pages),
                ("Unknown Page Type", StatisticsKey.UnknownPageType),
                ("Need Update", StatisticsKey.NeedUpdate),
                ("Waiting AI Request", StatisticsKey.WaitingAIRequest),
                ("Listings", StatisticsKey.Listings),
                ("Published Listings", StatisticsKey.PublishedListings),
                ("Unpublished Listings", StatisticsKey.UnpublishedListings),
            ];

            var tableRows = string.Join(
                Environment.NewLine,
                rows.Select(row =>
                    "                        <tr>" + Environment.NewLine +
                    $"                            <td>{row.Label}</td>" + Environment.NewLine +
                    $"                            <td>{GetLatestCounter(row.Key).ToString("N0", SummaryCulture)}</td>" + Environment.NewLine +
                    "                        </tr>"));

            return
                "                <table>" + Environment.NewLine +
                "                    <thead>" + Environment.NewLine +
                "                        <tr>" + Environment.NewLine +
                "                            <th>Data Type</th>" + Environment.NewLine +
                "                            <th>Value</th>" + Environment.NewLine +
                "                        </tr>" + Environment.NewLine +
                "                    </thead>" + Environment.NewLine +
                "                    <tbody>" + Environment.NewLine +
                tableRows + Environment.NewLine +
                "                    </tbody>" + Environment.NewLine +
                "                </table>";
        }

        private int GetLatestCounter(StatisticsKey statisticsKey)
        {
            var dataTable = _statistics.GetLatestStatistics(statisticsKey.ToString(), 1);
            return dataTable.Rows.Count == 0
                ? 0
                : Convert.ToInt32(dataTable.Rows[0]["Counter"]);
        }
    }
}
