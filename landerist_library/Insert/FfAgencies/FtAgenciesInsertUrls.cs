using HtmlAgilityPack;
using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Tools;
using Newtonsoft.Json;
using System.Text;

namespace landerist_library.Insert.FtAgencies
{
    public class FtAgenciesInsertUrls
    {
        private const string USER_AGENT = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36";
        private const string COOKIE = "userUUID=61b18f3d-9e1d-4fb5-8af4-e73f73cc84ce; contacte40ecfdb-4f2d-4db9-8045-8c4ce7c532a7=\"{'maxNumberContactsAllow':10}\"; sende40ecfdb-4f2d-4db9-8045-8c4ce7c532a7=\"{}\"; didomi_token=eyJ1c2VyX2lkIjoiMThlZTVjMWMtZjllMy02ZDc1LTk1MWEtODAxZTM4YmZhZWU0IiwiY3JlYXRlZCI6IjIwMjQtMDQtMTZUMDc6MTQ6MjcuODcwWiIsInVwZGF0ZWQiOiIyMDI0LTA0LTE2VDA3OjE0OjI5LjIzNVoiLCJ2ZW5kb3JzIjp7ImVuYWJsZWQiOlsiZ29vZ2xlIiwiYzpsaW5rZWRpbi1tYXJrZXRpbmctc29sdXRpb25zIiwiYzptaXhwYW5lbCIsImM6YWJ0YXN0eS1MTGtFQ0NqOCIsImM6aG90amFyIiwiYzp5YW5kZXhtZXRyaWNzIiwiYzpiZWFtZXItSDd0cjdIaXgiLCJjOmFwcHNmbHllci1HVVZQTHBZWSIsImM6dGVhbGl1bWNvLURWRENkOFpQIiwiYzp0aWt0b2stS1pBVVFMWjkiLCJjOmdvb2dsZWFuYS00VFhuSmlnUiIsImM6aWRlYWxpc3RhLUx6dEJlcUUzIiwiYzppZGVhbGlzdGEtZmVSRWplMmMiLCJjOmNvbnRlbnRzcXVhcmUiLCJjOm1pY3Jvc29mdCJdfSwicHVycG9zZXMiOnsiZW5hYmxlZCI6WyJhbmFseXRpY3MtSHBCSnJySzciLCJnZW9sb2NhdGlvbl9kYXRhIiwiZGV2aWNlX2NoYXJhY3RlcmlzdGljcyJdfSwidmVyc2lvbiI6MiwiYWMiOiJDaEdBRUFGa0ZDSUEuQUFBQSJ9; euconsent-v2=CP9KXsAP9KXsAAHABBENAwEsAP_gAAAAAAAAF5wBgAIAAqABaAFsAUgC8wAAACkoAMAAQUJKQAYAAgoSQgAwABBQkdABgACChISADAAEFCQA.f_wAAAAAAAAA; smc=\"{}\"; utag_main__prevCompleteClickName=255-idealista/others > >; SESSION=b172e53e2c112233~d8279848-c683-4804-a965-05931418b6d5; utag_main__sn=3; utag_main__se=1%3Bexp-session; utag_main__ss=1%3Bexp-session; utag_main__st=1713784693963%3Bexp-session; utag_main_ses_id=1713782893963%3Bexp-session; utag_main__pn=1%3Bexp-session; datadome=tZ~hp2unhYisDCg2LBpGEZ~OkuEycfmfKKlQzLVdwXdwpwZ4ocUYcbvWwuG5skxs0bxhvcleBZuxJRmutpAvdXpJatfHTFTWY1JpO40NpbyOJnJtZm~6SQKYbPyThoqF";
        private static readonly HashSet<string> HashSetUrls = [];
        private static readonly HashSet<string> Provinces = ["a-coruna", "albacete", "alicante", "almeria", "araba-alava", "asturias", "avila", "badajoz", "barcelona", "bizkaia", "burgos", "caceres", "cadiz", "cantabria", "castellon", "ceuta", "ciudad-real", "cordoba", "cuenca", "gipuzkoa", "girona", "granada", "guadalajara", "huelva", "huesca", "illes-balears", "jaen", "la-rioja", "las-palmas", "leon", "lleida", "lugo", "madrid", "malaga", "melilla", "murcia", "navarra", "ourense", "palencia", "pontevedra", "salamanca", "santa-cruz-de-tenerife", "segovia", "sevilla", "soria", "tarragona", "teruel", "toledo", "valencia", "valladolid", "zamora", "zaragoza"];
        private static string CurrentProvince = string.Empty;

        public static void GetProvincesList()
        {
            var fileName = PrivateConfig.INSERT_DIRECTORY + @"FtAgencies\index.html";
            var html = File.ReadAllText(fileName);

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            var links = htmlDocument.DocumentNode
                .SelectNodes("//a[@href]")
                .Select(a => a.GetAttributeValue("href", string.Empty))
                .Where(href => href.StartsWith("/buscar-agencias-inmobiliarias/"))
                .Select(href => href.Replace("/buscar-agencias-inmobiliarias/", ""))
                .Select(href => href.Replace("/todas-las-zonas/l", ""))
                .Select(href => href.Replace("-provincia", ""))
                .Distinct()
                .ToList();

            foreach (var link in links)
            {
                Console.Write("\"" + link + "\",");
            }
        }

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
                FtAgenciesUrls.Insert(url, CurrentProvince);
            }
        }

        private static void ScrapePage(int page = 1)
        {
            Console.WriteLine("Scrapping province: " + CurrentProvince + " page: " + page);

            string url = PrivateConfig.FT_AGENCIES_URL + CurrentProvince + 
                "-provincia/todas-las-zonas/l?pagina=" + page;

            try
            {
                var html = ScrapingBee.DownloadString(url, false);
                ParseResponse(html);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        private static void ParseResponse(string? html)
        {
            if (html == null)
            {
                return;
            }
            HtmlDocument htmlDocument = new();
            htmlDocument.LoadHtml(html);

            GetAgenciesUrls(htmlDocument);

            //bool isLastPage = (bool)responseObject.body.pagination.isLastPage;
            //int page = (int)responseObject.body.pagination.page;
            //if (!isLastPage)
            //{
            //    page++;
            //    ScrapePage(page);
            //}
        }

        private static void GetAgenciesUrls(HtmlDocument htmlDocument)
        {
            var agencies = htmlDocument.DocumentNode
               .SelectNodes($"//a[contains(@class, 'cta-spain-link')]")
               .Select(a => a.GetAttributeValue("href", string.Empty))
               .Where(href => !string.IsNullOrEmpty(href))
               .Distinct()
               .ToList();

            foreach (var agencie in agencies)
            {
                HashSetUrls.Add(agencie);
            }
        }
    }
}
