using Newtonsoft.Json;
using System.Text;
using landerist_library.Configuration;

namespace landerist_library.Insert.GooglePlaces
{
    public class PlacesSearch
    {
        private static readonly HashSet<string> Hosts = [];
        private static readonly string PLACES_NEARBY_URL = "https://places.googleapis.com/v1/places:searchNearby";

        public static void Search()
        {
            var results = SearchNearby().Result;
            AddHosts(results);
            Output();
        }

        private static async Task<string?> SearchNearby()
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("X-Goog-Api-Key", PrivateConfig.GOOGLE_CLOUD_LANDERIST_API_KEY);
            httpClient.DefaultRequestHeaders.Add("X-Goog-FieldMask", "places.websiteUri");

            var content = new StringContent(@"{
                ""includedPrimaryTypes"": [""real_estate_agency""],
                ""regionCode"": ""ES"",
                ""locationRestriction"": {
                    ""circle"": {
                        ""center"": {
                            ""latitude"": 40.5166280339,
                            ""longitude"": -3.8969465771
                        },
                        ""radius"": 5000.0
                    }
                }
            }", Encoding.UTF8, "application/json");

            try
            {
                var response = await httpClient.PostAsync(PLACES_NEARBY_URL, content);
                return await response.Content.ReadAsStringAsync();
            }
            catch
            {

            }
            return null;            
        }

        private static void AddHosts(string? json)
        {
            if (json == null)
            {
                return;
            }

            try
            {
                RootObject? placesObject = JsonConvert.DeserializeObject<RootObject>(json);
                if (placesObject != null)
                {
                    foreach (var place in placesObject.Places)
                    {
                        if (Uri.TryCreate(place.WebsiteUri, UriKind.Absolute, out Uri? uri))
                        {
                            Hosts.Add(uri.Host);
                        }                        
                    }
                }
            }
            catch{ }
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
