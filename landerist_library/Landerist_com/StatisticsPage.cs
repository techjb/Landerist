using landerist_library.Configuration;
using landerist_library.Export;
using landerist_library.Logs;
using landerist_library.Statistics;
using landerist_orels.ES;
using System.Data;

namespace landerist_library.Landerist_com
{
    public class StatisticsPage : Landerist_com
    {

        private static readonly string StatisticsTemplateHtmlFile =
            Config.LANDERIST_COM_TEMPLATES + "statistics_template.html";

        private static readonly string StatisticsHtmlFile =
            Config.LANDERIST_COM_OUTPUT + "statistics.html";


        private static readonly List<string> Charts = [];

        public static void Update()
        {
            try
            {
                Charts.Clear();

                Listings();
                Websites();
                Pages();
                UpdatedPages();
                NeedUpdate();
                NextUpdateDistribution();
                UnknownPageType();
                UpdatedHttpStatusCodeNull();
                UpdatedHttpStatusCode200();
                UpdatedHttpStatusCodeErrors();
                UpdatedPageType();
                ScrapedPages();
                BathcReaded();
                ListingInsertUpdate();
                PageType();
                PublishedPageType();
                UnPublishedPageType();
                UnPublishedHttpStatusCode();
                HttpStatusCode();
                //ListingsPageType();

                if (UpdateStatisctisPage())
                {
                    Log.WriteInfo("StatisticsPage", "Updated statistics page");
                }
            }
            catch (Exception exception)
            {
                Log.WriteError("StatisticsPage AreaChart", exception);
            }
        }

        private static void Listings()
        {
            List<StatisticsKey> statisticsKeys =
            [
                StatisticsKey.PublishedListings,
                StatisticsKey.UnpublishedListings,
            ];
            AreaChart("Listings", statisticsKeys, false);
        }

        private static void Websites()
        {
            AreaChart("Websites", StatisticsKey.Websites, false);
        }

        private static void Pages()
        {
            AreaChart("Pages", StatisticsKey.Pages, false);
        }

        private static void UpdatedPages()
        {
            LineChart("Updated Pages", StatisticsKey.UpdatedPages, false);
        }

        private static void NeedUpdate()
        {
            AreaChart("Need Update", StatisticsKey.NeedUpdate, true);
        }

        private static void NextUpdateDistribution()
        {
            var dictionary = landerist_library.Websites.Pages.GroupByNextUpdate();
            BarChart("Next Update Distribution", "nextupdate", dictionary);
        }

        private static void UnknownPageType()
        {
            AreaChart("Unknown PageType", StatisticsKey.UnknownPageType, true);
        }

        private static void UpdatedHttpStatusCodeNull()
        {
            LineChart("Updated HttpStatusCode null", StatisticsKey.HttpStatusCode_NULL, true);
        }

        private static void UpdatedHttpStatusCode200()
        {
            LineChart("Updated HttpStatusCode 200", StatisticsKey.HttpStatusCode_200, true);
        }
        private static void UpdatedHttpStatusCodeErrors()
        {
            var keys = StatisticsSnapshot.GetHttpStatusCodeKeys();
            keys.RemoveAll(code => code == StatisticsKey.HttpStatusCode_NULL.ToString() || code == StatisticsKey.HttpStatusCode_200.ToString());
            BarChart("Updated HttpStatusCode errors", keys, false);
        }

        private static void UpdatedPageType()
        {
            var keys = StatisticsSnapshot.GetPageTypeKeys();
            BarChart("Updated PageType", keys, false);
        }

        private static void ScrapedPages()
        {
            List<StatisticsKey> statisticsKeys =
            [
                StatisticsKey.ScrapedSuccess,
                StatisticsKey.ScrapedHttpStatusCodeNotOK,
                StatisticsKey.ScrapedCrashed,
            ];
            BarChart("Scraped Pages", statisticsKeys, false);
        }

        private static void BathcReaded()
        {
            List<StatisticsKey> statisticsKeys =
            [
                StatisticsKey.BatchReaded,
                StatisticsKey.BatchReadedErrors,
            ];
            BarChart("Batch Readed", statisticsKeys, false);
        }

        private static void ListingInsertUpdate()
        {
            List<StatisticsKey> statisticsKeys =
            [
                StatisticsKey.ListingInsert,
                StatisticsKey.ListingUpdate,
            ];
            BarChart("Listing Insert/Update", statisticsKeys, false);
        }

        private static void PageType()
        {
            var dictionary = landerist_library.Websites.Pages.GroupByPageType();
            PieChart("PageType", dictionary);
        }

        private static void ListingsPageType()
        {
            var dictionaryPublished = landerist_library.Websites.Pages.GroupByPageType(ListingStatus.published);
            var dictionaryUnPublished = landerist_library.Websites.Pages.GroupByPageType(ListingStatus.unpublished);

            var published = GetLabelValues("published", dictionaryPublished);
            var unpublished = GetLabelValues("unpublished", dictionaryUnPublished);

            var data = new List<string>
            {
                published, unpublished
            };
            BarChart("Listings PageType", string.Join(",", [.. data]));
        }

        private static void PublishedPageType()
        {
            var dictionary = landerist_library.Websites.Pages.GroupByPageType(ListingStatus.published);
            BarChart("Published Listings PageType", "published", dictionary);
        }

        private static void UnPublishedPageType()
        {
            var dictionary = landerist_library.Websites.Pages.GroupByPageType(ListingStatus.unpublished);
            BarChart("Unpublished Listings PageType", "unpublished", dictionary);
        }

        private static void UnPublishedHttpStatusCode()
        {
            var dictionary = landerist_library.Websites.Pages.GroupByHttpStatusCode(ListingStatus.unpublished);
            BarChart("Unpublished Listings HttpStatusCode", "unpublished", dictionary);
        }

        private static void HttpStatusCode()
        {
            var dictionary = landerist_library.Websites.Pages.CountByHttpStatusCode();
            PieChart("HttpStatusCode", dictionary);
        }


        private static void AreaChart(string title, StatisticsKey statisticKey, bool yesterday)
        {
            var keys = new List<StatisticsKey> { statisticKey };
            AreaChart(title, keys, yesterday);
        }

        private static void AreaChart(string title, List<StatisticsKey> keys, bool yesterday)
        {
            List<string> list = [.. keys.Select(key => key.ToString())];
            AreaChart(title, list, yesterday);
        }

        private static void AreaChart(string title, List<string> keys, bool yesterday)
        {
            var dataString = GetDataString(keys, yesterday, false);
            AreaChart(title, dataString);
        }

        private static void PieChart(string title, Dictionary<string, object?> dictionary)
        {
            string dataString = GetValues(dictionary);
            PieChart(title, dataString);
        }

        private static void LineChart(string title, StatisticsKey statisticsKey, bool yesterday)
        {
            List<string> keys = [statisticsKey.ToString()];
            LineChart(title, keys, yesterday);
        }

        private static void LineChart(string title, List<StatisticsKey> keys, bool yesterday)
        {
            List<string> list = [.. keys.Select(key => key.ToString())];
            LineChart(title, list, yesterday);
        }

        private static void LineChart(string title, List<string> keys, bool yesterday)
        {
            var data = GetDataString(keys, yesterday, false);
            LineChart(title, data);
        }

        private static void BarChart(string title, List<StatisticsKey> keys, bool yesterday)
        {
            List<string> list = [.. keys.Select(key => key.ToString())];
            BarChart(title, list, yesterday);
        }
        private static void BarChart(string title, List<string> keys, bool yesterday)
        {
            var data = GetDataString(keys, yesterday, true);
            BarChart(title, data);
        }

        private static void BarChart(string title, string key, Dictionary<string, object?> dictionary)
        {
            string dataString = GetLabelValues(key, dictionary);
            BarChart(title, dataString);
        }

        //private static void BarChart(string title, List<StatisticsKey> keys, bool yesterday)
        //{
        //    List<string> list = [.. keys.SelectTop1(key => key.ToString())];
        //    BarChart(title, list, yesterday);
        //}

        private static void AreaChart(string title, string data)
        {
            AddChart("AreaChart", title, data);
        }

        private static void LineChart(string title, string data)
        {
            AddChart("LineChart", title, data);
        }

        private static void BarChart(string title, string data)
        {
            AddChart("BarChart", title, data);
        }

        private static void PieChart(string title, string data)
        {
            AddChart("PieChart", title, data);
        }

        private static void AddChart(string charType, string title, string data)
        {
            string chart = charType + "('" + title + "', [" + data + "])";
            Charts.Add(chart);
        }

        private static string GetDataString(List<string> keys, bool yesterday, bool last10)
        {
            List<string> data = [];
            foreach (var key in keys)
            {
                var values = GetValues(key, yesterday, last10);
                var json = "{\"label\": \"" + key + "\", \"values\":[" + string.Join(",", [.. values]) + "]}";
                data.Add(json);
            }
            return string.Join(",", [.. data]);
        }

        private static List<string> GetValues(string statisticKey, bool yesterday, bool last10)
        {
            int top = last10 ? 10 : 100;
            var dataTable = StatisticsSnapshot.GetLatestStatistics(statisticKey, top);
            List<string> values = [];
            foreach (DataRow dataRow in dataTable.Rows.Cast<DataRow>().Reverse())
            {
                int counter = Convert.ToInt32(dataRow["Counter"]);
                var date = ((DateTime)dataRow["Date"]);
                if (yesterday)
                {
                    date = date.AddDays(-1);
                }
                var json = "{\"key\": \"" + date.ToShortDateString() + "\", \"value\": " + counter + "}";
                values.Add(json);
            }
            return values;
        }

        private static string GetLabelValues(string key, Dictionary<string, object?> dictionary)
        {
            var values = GetValues(dictionary);
            return "{\"label\": \"" + key + "\", \"values\":[" + values + "]}";
        }


        private static string GetValues(Dictionary<string, object?> dictionary)
        {
            List<string> data = [];
            foreach (var keyValuePair in dictionary)
            {
                var json = "{\"key\": \"" + keyValuePair.Key + "\", \"value\":" + keyValuePair.Value + "}";
                data.Add(json);
            }
            return string.Join(",", [.. data]);
        }


        private static bool UpdateStatisctisPage()
        {
            var statisticsTemplate = File.ReadAllText(StatisticsTemplateHtmlFile);
            var charts = string.Join("; " + Environment.NewLine, [.. Charts]);
            statisticsTemplate = statisticsTemplate.Replace("/*CHARTS*/", charts);

            File.WriteAllText(StatisticsHtmlFile, statisticsTemplate);
            return new S3().UploadToWebsiteBucket(StatisticsHtmlFile, "index.html", "statistics");
        }
    }
}
