using landerist_library.Database;
using Newtonsoft.Json;
using System.Text;

namespace landerist_library.Insert.IdAgenciesScraper
{
    public class InsertIdUrls
    {
        private const string USER_AGENT = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36";
        private const string COOKIE = "userUUID=61b18f3d-9e1d-4fb5-8af4-e73f73cc84ce; contacte40ecfdb-4f2d-4db9-8045-8c4ce7c532a7=\"{'maxNumberContactsAllow':10}\"; sende40ecfdb-4f2d-4db9-8045-8c4ce7c532a7=\"{}\"; didomi_token=eyJ1c2VyX2lkIjoiMThlZTVjMWMtZjllMy02ZDc1LTk1MWEtODAxZTM4YmZhZWU0IiwiY3JlYXRlZCI6IjIwMjQtMDQtMTZUMDc6MTQ6MjcuODcwWiIsInVwZGF0ZWQiOiIyMDI0LTA0LTE2VDA3OjE0OjI5LjIzNVoiLCJ2ZW5kb3JzIjp7ImVuYWJsZWQiOlsiZ29vZ2xlIiwiYzpsaW5rZWRpbi1tYXJrZXRpbmctc29sdXRpb25zIiwiYzptaXhwYW5lbCIsImM6YWJ0YXN0eS1MTGtFQ0NqOCIsImM6aG90amFyIiwiYzp5YW5kZXhtZXRyaWNzIiwiYzpiZWFtZXItSDd0cjdIaXgiLCJjOmFwcHNmbHllci1HVVZQTHBZWSIsImM6dGVhbGl1bWNvLURWRENkOFpQIiwiYzp0aWt0b2stS1pBVVFMWjkiLCJjOmdvb2dsZWFuYS00VFhuSmlnUiIsImM6aWRlYWxpc3RhLUx6dEJlcUUzIiwiYzppZGVhbGlzdGEtZmVSRWplMmMiLCJjOmNvbnRlbnRzcXVhcmUiLCJjOm1pY3Jvc29mdCJdfSwicHVycG9zZXMiOnsiZW5hYmxlZCI6WyJhbmFseXRpY3MtSHBCSnJySzciLCJnZW9sb2NhdGlvbl9kYXRhIiwiZGV2aWNlX2NoYXJhY3RlcmlzdGljcyJdfSwidmVyc2lvbiI6MiwiYWMiOiJDaEdBRUFGa0ZDSUEuQUFBQSJ9; euconsent-v2=CP9KXsAP9KXsAAHABBENAwEsAP_gAAAAAAAAF5wBgAIAAqABaAFsAUgC8wAAACkoAMAAQUJKQAYAAgoSQgAwABBQkdABgACChISADAAEFCQA.f_wAAAAAAAAA; smc=\"{}\"; utag_main__prevCompleteClickName=255-idealista/others > >; SESSION=b172e53e2c112233~d8279848-c683-4804-a965-05931418b6d5; utag_main__sn=3; utag_main__se=1%3Bexp-session; utag_main__ss=1%3Bexp-session; utag_main__st=1713784693963%3Bexp-session; utag_main_ses_id=1713782893963%3Bexp-session; utag_main__pn=1%3Bexp-session; datadome=tZ~hp2unhYisDCg2LBpGEZ~OkuEycfmfKKlQzLVdwXdwpwZ4ocUYcbvWwuG5skxs0bxhvcleBZuxJRmutpAvdXpJatfHTFTWY1JpO40NpbyOJnJtZm~6SQKYbPyThoqF";
        private static readonly HashSet<string> HashSetUrls = [];
        private static readonly HashSet<int> Provinces = [02, 03, 04, 01, 33, 05, 06, 07, 08, 48, 09, 10, 11, 39, 12, 13, 14, 15, 16, 20, 17, 18, 19, 21, 22, 23, 24, 25, 27, 28, 29, 30, 31, 32, 34, 35, 36, 26, 37, 38, 40, 41, 42, 43, 44, 45, 46, 47, 49, 50, 51, 52];
        private static int CurrentProvince = 0;
        public static void Start()
        {
            foreach (var province in Provinces)
            {
                CurrentProvince = province;
                HashSetUrls.Clear();
                ScrapePage(1);
                Insert();
            }
        }        

        private static void Insert()
        {
            Console.WriteLine("Inserting " + HashSetUrls.Count + " urls ..");
            foreach (var url in HashSetUrls)
            {
                AgenciesUrls.Insert(url, CurrentProvince);
            }
        }

        private static async void ScrapePage(int page = 1)
        {

            Console.WriteLine("Scrapping province: " + CurrentProvince + " page: " + page);

            var httpClient = new HttpClient();

            var location = "0-EU-ES-" + CurrentProvince.ToString().PadLeft(2, '0');
            var postData = new
            {
                location,
                operation = "SALE",
                typology = "NO_TYPOLOGY",
                minPrice = 0,
                maxPrice = (int?)null,
                languages = Array.Empty<string>(),
                pageNumber = page
            };

            var request = new HttpRequestMessage(HttpMethod.Post, Configuration.PrivateConfig.IDAGENCIES_URL)
            {
                Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(postData), Encoding.UTF8, "application/json")
            };

            request.Headers.Add("Accept-Language", "es-ES,es;q=0.9");
            request.Headers.Add("Cookie", COOKIE);
            request.Headers.Add("User-Agent", USER_AGENT);

            try
            {
                var response = httpClient.SendAsync(request).Result;
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    ParseResponse(responseString);
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
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
                ScrapePage(page);
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
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message + "\n" + exception.StackTrace);
                }
            }
        }
    }
}
