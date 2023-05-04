using landerist_library.Configuration;
using landerist_library.Insert;
using landerist_library.Scrape;
using landerist_library.Websites;

namespace landerist_console
{
    internal class Program
    {

        private static DateTime DateStart;

        static void Main(string[] args)
        {
            Console.Title = "Landerist Console";
            Config.Init(false);
            SetDateStart();
            Run();
            SetFinish();
            EndBeep();
        }

        private static void SetDateStart()
        {
            DateStart = DateTime.Now;
            string textStarted =
                "STARTED at " + DateStart.ToShortDateString() + " " + DateStart.ToString(@"hh\:mm\:ss") + "\n";
            Console.WriteLine(textStarted);
        }

        private static void SetFinish()
        {
            DateTime dateFinished = DateTime.Now;
            string textFinished =
                "FINISHED at " + dateFinished.ToShortDateString() + " " + dateFinished.ToString(@"hh\:mm\:ss") +
                "\nDuration: " + (dateFinished - DateStart).ToString(@"dd\:hh\:mm\:ss\.fff") + ". ";
            Console.WriteLine("\n" + textFinished);
        }

        private static void Run()
        {
            //var uriPage = new Uri("https://www.saroga.es/inmueble/chalet-independiente-satelites-majadahonda/");
            //var uriPage = new Uri("https://www.saroga.es/inmueble/piso-4-dormitorios-avenida-europa-pozuelo/");
            //var uriPage = new Uri("https://www.saroga.es/inmueble/piso-monte-pilar-majadahonda/");
            //var uriPage = new Uri("https://www.saroga.es/inmueble/apartamento-majadahonda/");          
            //var uriPage = new Uri("https://www.inmolocalgestion.com/ficha-inmueble.php?id=53");
            //var uriPage = new Uri("https://www.expimad.com/inmueble/piso-2-habitaciones-sin-comision-de-agenciasesena-urbanizacion-el-quinon-ideal-inversoresactualmente-alquilado-/20231214");
            //var uriPage = new Uri("https://www.prorealty.es/es/las_rozas_de_madrid/molino_de_la_hoz/chalets_independientes/ref-4465");
            var uriPage = new Uri("https://www.goolzoom.com");
            Selenium.CrhomeScrapePage(uriPage);
            //Selenium.FirefoxScrape(uriPage);

            //var uri = new Uri("https://www.goolzoom.com/");
            //var uri = new Uri("https://www.saroga.es/");
            //var uri = new Uri("https://mabelan.es/");
            //var uri = new Uri("https://www.saguar.immo/");
            //var uri = new Uri("https://www.inmolocalgestion.com/");
            //var uri = new Uri("https://www.expimad.com/");
            //var uri = new Uri("https://www.prorealty.es/");

            //var website = new Website(uri);
            //var page = new Page(website, uriPage);

            //website.Delete(); return;
            //Websites.DeleteAll(); return;

            //new WebsitesInserter(false).DeleteAndInsert(uri); return;
            //new WebsitesInserter(false).Insert(uri); return;
            //new WebsitesInserter().FromCsv();            

            //Websites.SetHttpStatusCodesToNull();
            //Websites.InsertUpdateUrisFromNotOk();
            //Websites.SetHttpStatusCodesToAll();            
            //Websites.SetRobotsTxtToHttpStatusCodeOk();
            //Websites.SetIpAdress();            
            //Websites.CountCanAccesToMainUri();
            //Websites.CountRobotsSiteMaps();            
            //Websites.InsertMainPages();

            //new Scraper().ScrapeMainPage(website);
            //new Scraper().ScrapeNonScrapped(uri);
            //new Scraper().ScrapeUnknowIsListing(uri, true);
            //new Scraper().ScrapeIsNotListing(uri);
            //new Scraper().CrhomeScrapePage(page);
            //new Scraper().ScrapeAllPages();

            //Csv.Export(true);
            //Json.Export(true);

            //string regexPattern = @"latitude\s*=\s*(-?\d+(\.\d+)?)\s*,\s*longitude\s*=\s*(-?\d+(\.\d+)?)";
            //string text = "latitude=40.504108,longitude=-3.888549 cca de perro latitude = 28.504108 , longitude = -10.888549";

            //string regexPattern = @"lat\s*:\s*(-?\d+\.\d+)\s*,\s*lng\s*:\s*(-?\d+\.\d+)";
            //string text = "lat:40.504108,lng:-3.888549 cca de perro lat: 28.504108 , lng: -10.888549";

            //string regexPattern = @"LatLng\s*\(\s*(-?\d+\.\d+)\s*,\s*(-?\d+\.\d+)\s*\)";
            //string text = " lola LatLng( 40.504108 , -3.888549 ) LatLng (28.504108 , -10.888549) misers";

            //string text = "acasca";
            //var matches = new Regex(regexPattern).Matches(text);
            //foreach (Match match in matches)
            //{
            //    string latitude;
            //    string longitude;
            //    switch (match.Groups.Count)
            //    {
            //        case 3: latitude = match.Groups[1].Value; longitude = match.Groups[2].Value; break;
            //        case 5: latitude = match.Groups[1].Value; longitude = match.Groups[3].Value; break;
            //        default: continue;
            //    }

            //}          
        }

        private static void EndBeep()
        {
            int counter = 0;
            while (counter < 1)
            {
                counter++;
                Console.Beep(400, 1000);
                Thread.Sleep(300);
            }
        }
    }
}