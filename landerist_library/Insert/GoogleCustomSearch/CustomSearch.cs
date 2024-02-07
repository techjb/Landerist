using landerist_library.Configuration;
using landerist_library.Insert.GooglePlaces;
using Newtonsoft.Json;
using Google.Apis.CustomSearchAPI.v1;
using Google.Apis.Services;
using Google.Apis.CustomSearchAPI.v1.Data;
using OpenQA.Selenium.DevTools.V118.Browser;

namespace landerist_library.Insert.GoogleCustomSearch
{
    public class CustomSearch
    {
        private static readonly HashSet<string> Hosts = [];
        private static readonly string CUSTOM_SEARCH_URL = "https://places.googleapis.com/v1/places:searchNearby";
        private const int TOTAL_RESULTS_DESIRED = 30; 

        public static void Search()
        {
            SearhQuery();
            Output();
        }

        private static void SearhQuery()
        {
            string query = "inmobiliaria las rozas de madrid";

            var initializer = new BaseClientService.Initializer
            {
                ApiKey = Config.GOOGLE_CLOUD_LANDERIST_API_KEY,
                //GZipEnabled = true,
            };

            int totalResultsObtained = 0;
            int startIndex = 1;

            while (totalResultsObtained < TOTAL_RESULTS_DESIRED)
            {                
                var customSearchAPIService = new CustomSearchAPIService(initializer);
                var listRequest = customSearchAPIService.Cse.List();
                listRequest.Q = query;
                listRequest.Start = startIndex;
                listRequest.Cx = Config.GOOGLE_SEARCH_ENGINE_ID;
                listRequest.Gl = "es";

                var items = Execute(listRequest, ref totalResultsObtained);
                startIndex += items;
                if (startIndex >= 100)
                {
                    break;
                }
            }
        }

        private static int Execute(CseResource.ListRequest listRequest, ref int totalResultsObtained)
        {
            try
            {
                Search search = listRequest.Execute();
                foreach (var item in search.Items)
                {
                    if (Uri.TryCreate(item.Link, UriKind.Absolute, out Uri? uri))
                    {
                        Hosts.Add(uri.Host);
                    }
                    totalResultsObtained++;
                    if (totalResultsObtained >= TOTAL_RESULTS_DESIRED)
                    {
                        break;
                    }
                }
                return search.Items.Count;
            }
            catch (Exception ex)
            {

            }
            return 0;
        }

        private static void Output()
        {
            Console.WriteLine("Counter: " + Hosts.Count);
            foreach (var host in Hosts)
            {
                Console.WriteLine(host);
            }
        }
    }
}
