using Google.Apis.CustomSearchAPI.v1;
using Google.Apis.CustomSearchAPI.v1.Data;
using Google.Apis.Services;
using landerist_library.Configuration;

namespace landerist_library.Insert.GoogleCustomSearch
{
    public class CustomSearch
    {
        private static readonly Dictionary<string, int> Hosts = [];
        private const int TOTAL_RESULTS_DESIRED = 30;

        public static void Start()
        {
            var municipalities = GetMunicipalities();
            Search(municipalities);
            Output();
        }

        private static HashSet<string> GetMunicipalities()
        {
            HashSet<string> result = [];
            return result;
        }

        private static void Search(HashSet<string> municipalities)
        {
            int total = municipalities.Count;
            int processed = 0;
            foreach (var municipality in municipalities)
            {
                processed++;
                Console.WriteLine(processed + "/" + total);
                Searh(municipality);
            }
        }

        private static void Searh(string municipality)
        {
            string query = "inmobiliaria " + municipality;

            var initializer = new BaseClientService.Initializer
            {
                ApiKey = PrivateConfig.GOOGLE_CLOUD_LANDERIST_API_KEY,
            };

            int totalResultsObtained = 0;
            int startIndex = 1;

            while (totalResultsObtained < TOTAL_RESULTS_DESIRED)
            {
                var customSearchAPIService = new CustomSearchAPIService(initializer);
                var listRequest = customSearchAPIService.Cse.List();
                listRequest.Q = query;
                listRequest.Start = startIndex;
                listRequest.Cx = PrivateConfig.GOOGLE_SEARCH_ENGINE_ID;
                listRequest.Gl = "es";

                var items = ExecuteRequest(listRequest, ref totalResultsObtained);
                startIndex += items;

                //break;
                if (startIndex >= 100)
                {
                    break;
                }
            }
        }

        private static int ExecuteRequest(CseResource.ListRequest listRequest, ref int totalResultsObtained)
        {
            try
            {
                Search search = listRequest.Execute();
                foreach (var item in search.Items)
                {
                    if (Uri.TryCreate(item.Link, UriKind.Absolute, out Uri? uri))
                    {
                        AddHost(uri.Host);
                    }
                    totalResultsObtained++;
                    if (totalResultsObtained >= TOTAL_RESULTS_DESIRED)
                    {
                        break;
                    }
                }
                return search.Items.Count;
            }
            catch //(Exception ex)
            {

            }
            return 0;
        }

        private static void AddHost(string host)
        {
            if (Hosts.TryGetValue(host, out int value))
            {
                Hosts[host] = ++value;
            }
            else
            {
                Hosts.Add(host, 1);
            }
        }

        private static void Output()
        {
            Console.WriteLine("Counter: " + Hosts.Count);
            var hosts = Hosts.OrderByDescending(x => x.Value).Take(50);

            foreach (var host in hosts)
            {
                Console.WriteLine($"{host.Key} : {host.Value}");
            }
        }
    }
}
