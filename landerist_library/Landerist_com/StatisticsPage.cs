using landerist_library.Configuration;
using landerist_library.Export;
using landerist_library.Logs;
using landerist_library.Statistics;
using landerist_library.Websites;
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
                UpdateTemplate(StatisticsKey.HttpStatusCode_NULL, true);
                UpdateTemplate(StatisticsKey.HttpStatusCode_200, true);
                UpdateHttpStatusCode();
                UpdatePageType();
                UpdateScraped();
                UpdateBathcReaded();
                UpdateListingInsertUpdate();
                UpdatePublishedPageType();
                UpdateUnPublishedPageType();

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
            var keys = StatisticsSnapshot.GetHttpStatusCodeKeys();
            keys.RemoveAll(code => code == StatisticsKey.HttpStatusCode_NULL.ToString() || code == StatisticsKey.HttpStatusCode_200.ToString());
            Update(keys, StatisticsKey.HttpStatusCode);
        }

        private static void UpdatePageType()
        {
            var keys = StatisticsSnapshot.GetPageTypeKeys();
            Update(keys, StatisticsKey.PageType);
        }

        private static void UpdateScraped()
        {
            List<StatisticsKey> statisticsKeys =
            [
                StatisticsKey.ScrapedSuccess,
                StatisticsKey.ScrapedHttpStatusCodeNotOK,
                StatisticsKey.ScrapedCrashed,
            ];
            Update(statisticsKeys, "ScrapedPages");
        }

        private static void UpdateBathcReaded()
        {
            List<StatisticsKey> statisticsKeys =
            [
                StatisticsKey.BatchReaded,
                StatisticsKey.BatchReadedErrors,                
            ];
            Update(statisticsKeys, "BatchReaded");
        }

        private static void UpdateListingInsertUpdate()
        {
            List<StatisticsKey> statisticsKeys =
            [
                StatisticsKey.ListingInsert,
                StatisticsKey.ListingUpdate,
            ];
            Update(statisticsKeys, "ListingInsertUpdate");
        }

        private static void UpdatePublishedPageType()
        {
            UpdatePageType(landerist_orels.ES.ListingStatus.published);
        }

        private static void UpdateUnPublishedPageType()
        {
            UpdatePageType(landerist_orels.ES.ListingStatus.unpublished);
        }

        private static void UpdatePageType(landerist_orels.ES.ListingStatus listingStatus)
        {
            var dictionary = Pages.GetByPageType(listingStatus);
            string statisticsKey = listingStatus.ToString() + "PageType";
            Update(dictionary, statisticsKey);
        }

        private static void Update(List<StatisticsKey> keys, string statisticsKey)
        {
            List<string> list = [.. keys.Select(key => key.ToString())];
            Update(list, statisticsKey);
        }

        private static void Update(List<string> keys, StatisticsKey statisticsKey)
        {
            Update(keys, statisticsKey.ToString());
        }

        private static void Update(List<string> keys, string statisticsKey)
        {
            List<string> data = [];
            foreach (var key in keys)
            {
                var values = GetValues(key, false);
                var json = "{\"label\": \"" + key + "\", \"values\":[" + string.Join(",", [.. values]) + "]}";
                data.Add(json);
            }
            string dataString = string.Join(",", [.. data]);
            StatisticsTemplate = StatisticsTemplate.Replace("/*"+ statisticsKey.ToString() +"*/", dataString);
        }

        private static void Update(Dictionary<string, object?> dictionary, string statisticsKey)
        {
            List<string> data = [];
            foreach (var keyValuePair in dictionary)
            {
                var json = "{\"label\": \"" + keyValuePair.Key + "\", \"value\":" + keyValuePair.Value + "}";
                data.Add(json);
            }
            string dataString = string.Join(",", [.. data]);
            StatisticsTemplate = StatisticsTemplate.Replace("/*" + statisticsKey.ToString() + "*/", dataString);
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
