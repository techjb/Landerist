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
            Config.LANDERIST_COM_DIRECTORY + "statistics_template.html";

        private static readonly string StatisticsHtmlFile =
            Config.LANDERIST_COM_DIRECTORY + "statistics.html";

        private static string StatisticsTemplate = string.Empty;

        public StatisticsPage()
        {
            StatisticsTemplate = File.ReadAllText(StatisticsTemplateHtmlFile);
        }
        public void Update()
        {
            try
            {
                UpdateTemplate(StatisticsKey.Listings);
                UpdateTemplate(StatisticsKey.Websites);
                UpdateTemplate(StatisticsKey.Pages);
                UpdateTemplate(StatisticsKey.UpdatedPages);                

                if (UploadStatisticsFile())
                {
                    Log.WriteLogInfo("StatisticsPage", "Updated");
                }

            }
            catch (Exception exception)
            {
                Log.WriteLogErrors("StatisticsPage Update", exception);
            }
        }

        private static void UpdateTemplate(StatisticsKey statisticsKey)
        {
            var dataTable = StatisticsSnapshot.GetTop100Statistics(statisticsKey);
            List<string> values = [];
            foreach (DataRow dataRow in dataTable.Rows)
            {
                int counter = Convert.ToInt32(dataRow["Counter"]);
                var date = ((DateTime)dataRow["Date"]).ToShortDateString();
                var json = "{\"date\": \"" + date + "\", \"count\": " + counter + "}";
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
