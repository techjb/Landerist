using landerist_library.Configuration;
using landerist_library.Export;
using landerist_library.Logs;
using landerist_library.Statistics;
using System.Data;

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

                if (UploadStatisticsFile())
                {
                    Log.WriteInfo("StatisticsPage", "Updated");
                }

            }
            catch (Exception exception)
            {
                Log.WriteError("StatisticsPage Update", exception);
            }
        }

        private static void UpdateTemplate(StatisticsKey statisticsKey, bool yesterday)
        {
            var dataTable = StatisticsSnapshot.GetTop100Statistics(statisticsKey);
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

            string templateKey = "/*" + statisticsKey.ToString() + "*/";
            string valuesString = string.Join(",", [.. values]);
            StatisticsTemplate = StatisticsTemplate.Replace(templateKey, valuesString);
        }

        private static bool UploadStatisticsFile()
        {
            File.WriteAllText(StatisticsHtmlFile, StatisticsTemplate);
            return new S3().UploadToWebsiteBucket(StatisticsHtmlFile, "index.html", "statistics");
        }
    }
}
