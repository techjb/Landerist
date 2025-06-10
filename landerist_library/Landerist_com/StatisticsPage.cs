using landerist_library.Configuration;
using landerist_library.Export;
using landerist_library.Logs;
using landerist_library.Statistics;
using PuppeteerSharp.BrowserData;
using System.Data;
using System.Diagnostics.Metrics;

namespace landerist_library.Landerist_com
{
    public class StatisticsPage : Landerist_com
    {

        private static readonly string StatisticsTemplateHtmlFile =
            Config.LANDERIST_COM_TEMPLATES + "statistics_template.html";

        private static readonly string StatisticsHtmlFile =
            Config.LANDERIST_COM_OUTPUT + "statistics.html";

        private static string StatisticsTemplate = string.Empty;

        public static void Update()
        {
            try
            {
                StatisticsTemplate = File.ReadAllText(StatisticsTemplateHtmlFile);
                UpdateTemplate(StatisticsKey.Listings, false);
                UpdateTemplate(StatisticsKey.PublishedListings, false);
                UpdateTemplate(StatisticsKey.UnpublishedListings, false);
                UpdateTemplate(StatisticsKey.Websites, false);
                UpdateTemplate(StatisticsKey.Pages, false);
                UpdateTemplate(StatisticsKey.UpdatedPages, true);
                UpdateTemplate(StatisticsKey.NeedUpdate, true);
                UpdateTemplate(StatisticsKey.UnknownPageType, true);                
                UpdateHttpStatusCode();
                UpdateTemplate(StatisticsKey.HttpStatusCode_NULL, true);

                if (UploadStatisticsFile())
                {
                    Log.WriteInfo("StatisticsPage", "Updated statistics page");
                }
            }
            catch (Exception exception)
            {
                Log.WriteError("StatisticsPage Update", exception);
            }
        }

        private static void UpdateHttpStatusCode()
        {
            var statisticsKey = StatisticsSnapshot.GetHttpStatusCodeKeys();
            List<string> data = [];
            foreach (var key in statisticsKey)
            {
                if (key.Equals(StatisticsKey.HttpStatusCode_NULL.ToString()))
                {
                    continue;
                }
                var values = GetValues(key, false);
                var json = "{\"label\": \"" + key + "\", \"values\":[" + string.Join(",", [.. values]) + "]}";
                data.Add(json);
            }
            string dataString = string.Join(",", [.. data]);
            StatisticsTemplate = StatisticsTemplate.Replace("/*HttpStatusCode*/", dataString);
        }

        private static void UpdateTemplate(StatisticsKey statisticsKey, bool yesterday)
        {
            var values = GetValues(statisticsKey.ToString(), yesterday);
            string templateKey = "/*" + statisticsKey.ToString() + "*/";
            string data = string.Join(",", [.. values]);
            StatisticsTemplate = StatisticsTemplate.Replace(templateKey, data);
        }

        private static List<string> GetValues( string statisticKey, bool yesterday)
        {
            var dataTable = StatisticsSnapshot.GetTop100Statistics(statisticKey);
            List<string> values = [];
            foreach (DataRow dataRow in dataTable.Rows.Cast<DataRow>().Reverse())
            {
                int counter = Convert.ToInt32(dataRow["Counter"]);
                var date = ((DateTime)dataRow["Date"]);
                if (yesterday)
                {
                    date = date.AddDays(-1);
                }
                var json = "{\"date\": \"" + date.ToShortDateString() + "\", \"count\": " + counter + "}";
                values.Add(json);
            }
            return values;
        }

        private static bool UploadStatisticsFile()
        {
            File.WriteAllText(StatisticsHtmlFile, StatisticsTemplate);
            return new S3().UploadToWebsiteBucket(StatisticsHtmlFile, "index.html", "statistics");
        }
    }
}
