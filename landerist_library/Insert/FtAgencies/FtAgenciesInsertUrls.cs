using HtmlAgilityPack;
using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Tools;

namespace landerist_library.Insert.FtAgencies
{
    public class FtAgenciesInsertUrls
    {
        private static readonly HashSet<string> HashSetUrls = [];
        private static readonly HashSet<string> Provinces = ["a-coruna", "albacete", "alicante", "almeria", "araba-alava", "asturias", "avila", "badajoz", "barcelona", "bizkaia", "burgos", "caceres", "cadiz", "cantabria", "castellon", "ceuta", "ciudad-real", "cordoba", "cuenca", "gipuzkoa", "girona", "granada", "guadalajara", "huelva", "huesca", "illes-balears", "jaen", "la-rioja", "las-palmas", "leon", "lleida", "lugo", "madrid", "malaga", "melilla", "murcia", "navarra", "ourense", "palencia", "pontevedra", "salamanca", "santa-cruz-de-tenerife", "segovia", "sevilla", "soria", "tarragona", "teruel", "toledo", "valencia", "valladolid", "zamora", "zaragoza"];

        private static int Inserted = 0;
        private static int Errors = 0;

        private static object Sync = new();

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
            Parallel.ForEach(Provinces, new ParallelOptions()
            {
                MaxDegreeOfParallelism = 49
            }, province =>
            {
                ScrapePage(province, false, 1);
                ScrapePage(province, true, 1);

            });
            InsertUrls();
        }

        private static void ScrapePage(string province, bool rent, int page)
        {
            string url = PrivateConfig.FT_AGENCIES_URL + province +
                "-provincia/todas-las-zonas/l?" +
                (rent ? "tipo=alquiler&" : "") +
                "pagina=" + page;

            Console.WriteLine("Scraping " + url);

            try
            {
                var html = ScrapingBee.DownloadString(url, false);
                ParseResponse(province, rent, page, html);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        private static void ParseResponse(string province, bool rent, int page, string? html)
        {
            if (html == null)
            {
                return;
            }

            HtmlDocument htmlDocument = new();
            htmlDocument.LoadHtml(html);

            AddAgenciesUrls(htmlDocument);

            bool isLastPage = htmlDocument.DocumentNode.SelectSingleNode("//a[@rel='next']") == null;
            if (!isLastPage)
            {
                page++;
                ScrapePage(province, rent, page);
            }
        }

        private static void AddAgenciesUrls(HtmlDocument htmlDocument)
        {
            var agencies = htmlDocument.DocumentNode
               .SelectNodes($"//a[contains(@class, 'cta-spain-link')]")
               .Select(a => a.GetAttributeValue("href", string.Empty))
               .Where(href => !string.IsNullOrEmpty(href))
               .Distinct()
               .ToList();

            lock (Sync)
            {
                foreach (var agencie in agencies)
                {
                    HashSetUrls.Add(agencie);
                }
            }
        }

        private static void InsertUrls()
        {
            Console.WriteLine("Inserting " + HashSetUrls.Count + " urls");
            foreach (var url in HashSetUrls)
            {
                if (FtAgenciesUrls.Insert(url))
                {
                    Inserted++;
                }
                else
                {
                    Errors++;
                }
            }
            Console.WriteLine("Inserted: " + Inserted + " Errors: " + Errors);
        }
    }
}
