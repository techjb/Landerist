using Newtonsoft.Json;
using System.Text;

namespace landerist_library.Insert.IdAgenciesScraper
{
    public class IdAgenciesScraper
    {
        private const string USER_AGENT = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Safari/537.36";
        private const string COOKIE = "userUUID=54fcb726-73f1-41c9-a951-75c90d8609af; didomi_token=eyJ1c2VyX2lkIjoiMThkN2EwZjQtMzE0MC02ZmQ1LWIyNzItMWE2NzdmMDEwYjE2IiwiY3JlYXRlZCI6IjIwMjQtMDItMDVUMTY6MTc6MTcuMTA4WiIsInVwZGF0ZWQiOiIyMDI0LTAyLTA1VDE2OjE3OjE4LjQ3MVoiLCJ2ZW5kb3JzIjp7ImVuYWJsZWQiOlsiZ29vZ2xlIiwiYzpsaW5rZWRpbi1tYXJrZXRpbmctc29sdXRpb25zIiwiYzptaXhwYW5lbCIsImM6YWJ0YXN0eS1MTGtFQ0NqOCIsImM6aG90amFyIiwiYzp5YW5kZXhtZXRyaWNzIiwiYzpiZWFtZXItSDd0cjdIaXgiLCJjOmFwcHNmbHllci1HVVZQTHBZWSIsImM6dGVhbGl1bWNvLURWRENkOFpQIiwiYzp0aWt0b2stS1pBVVFMWjkiLCJjOmlkZWFsaXN0YS1MenRCZXFFMyIsImM6aWRlYWxpc3RhLWZlUkVqZTJjIiwiYzpjb250ZW50c3F1YXJlIiwiYzptaWNyb3NvZnQiXX0sInB1cnBvc2VzIjp7ImVuYWJsZWQiOlsiYW5hbHl0aWNzLUhwQkpycks3IiwiZ2VvbG9jYXRpb25fZGF0YSIsImRldmljZV9jaGFyYWN0ZXJpc3RpY3MiXX0sInZlcnNpb24iOjIsImFjIjoiQUZtQUNBRmsuQUFBQSJ9; euconsent-v2=CP5gXIAP5gXIAAHABBENAmEsAP_gAAAAAAAAF5wBgAIAAqABaAFsAUgC8wAAACkoAMAARBqKQAYAAiDUQgAwABEGodABgACINQSADAAEQagA.f_wAAAAAAAAA; galleryHasBeenBoosted=true; askToSaveAlertPopUp=true; cookieSearch-1=\"/venta-terrenos/las-rozas-de-madrid-madrid/:1707212256274\"; sendc6b604b4-ee76-4dbb-a365-0a23cdf4a913=\"{}\"; SESSION=f210a8a454e0afe2~a0cb1c05-4975-41e3-9701-800180cd1cfe; utag_main__sn=6; utag_main_ses_id=1707319631993%3Bexp-session; utag_main__prevVtUrl=https://www.google.com/%3Bexp-1707323232131; utag_main__prevVtUrlReferrer=https://www.google.com/%3Bexp-1707323232131; utag_main__prevVtSource=Search engines%3Bexp-1707323232131; utag_main__prevVtCampaignName=organicWeb%3Bexp-1707323232131; utag_main__prevVtCampaignCode=%3Bexp-1707323232131; utag_main__prevVtCampaignLinkName=%3Bexp-1707323232131; utag_main__prevVtRecipientId=%3Bexp-1707323232131; utag_main__prevVtProvider=%3Bexp-1707323232131; utag_main__ss=0%3Bexp-session; contacta0cb1c05-4975-41e3-9701-800180cd1cfe=\"{'maxNumberContactsAllow':10}\"; smc=\"{}\"; utag_main__prevCompleteClickName=; utag_main__pn=53%3Bexp-session; datadome=LJf~HyvdsskCh5tEGU2yewes56PMC7sI6GnTdjHrtTvhKHxywzSDIQi8_sikhCwY5PSWphymURmPU5_7ZnBi2KKi7~q_FUBX0bifCZoLAMZwBl1iZrF0J~6hjuqLHyb3; utag_main__se=84%3Bexp-session; utag_main__st=1707323532453%3Bexp-session; utag_main__prevCompletePageName=023-idealista/expertAgencyList > portal > viewExpertAgencyList%3Bexp-1707325332462; utag_main__prevLevel2=023-idealista/expertAgencyList%3Bexp-1707325332462";
        private static readonly HashSet<string> HashSetUrls = [];
        public static void Start()
        {
            Scrape();
            Output();
        }

        private static async void Scrape(int page = 1)
        {
            var httpClient = new HttpClient();            

            var request = new HttpRequestMessage(HttpMethod.Post, Configuration.Config.IDAGENCIES_URL)
            {
                Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(new
                {
                    location = "0-EU-ES-15",
                    operation = "SALE",
                    typology = "HOUSING",
                    minPrice = 0,
                    maxPrice = (int?)null,
                    languages = Array.Empty<string>(),
                    pageNumber = page
                }), Encoding.UTF8, "application/json")
            };

            request.Headers.Add("Accept-Language", "es-ES,es;q=0.9");
            request.Headers.Add("Cookie", COOKIE);
            request.Headers.Add("User-Agent", USER_AGENT);

            try
            {
                var response = httpClient.SendAsync(request).Result;
                Console.WriteLine(page + " Sucess: " + response.IsSuccessStatusCode);
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    ParseResponse(responseString);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void ParseResponse(string? responseString)
        {
            if (responseString == null)
            {
                return;
            }
            dynamic? responseObject = JsonConvert.DeserializeObject(responseString);
            if (responseObject == null)
            {
                return;
            }
            GetAgenciesUrls(responseObject);
            bool isLastPage = (bool)responseObject.body.pagination.isLastPage;
            int page = (int)responseObject.body.pagination.page;
            if (!isLastPage)
            {
                page++;
                Scrape(page);
            }
        }

        private static void GetAgenciesUrls(dynamic responseObject)
        {
            var agencies = responseObject.body.agenciesListing.matchingAgencies;
            foreach (var agencie in agencies)
            {
                try
                {
                    var url = (string)agencie.commercialData.microsite.url;
                    HashSetUrls.Add(url);
                }
                catch (Exception ex)
                {

                }
            }
        }

        private static void Output()
        {
            foreach (var url in HashSetUrls)
            {
                Console.WriteLine($"{url}");
            }
            Console.WriteLine(HashSetUrls.Count);
        }

    }
}
