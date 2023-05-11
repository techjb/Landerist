using landerist_library.Configuration;
using landerist_library.Download;
using landerist_library.Export;
using landerist_library.Insert;
using landerist_library.Parse.LocationParser;
using landerist_library.Scrape;
using landerist_library.Websites;
using landerist_orels.ES;

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


            //SeleniumDownloader.GetChrome(uriPage);
            //PuppeteerDownloader.Get(uriPage);
            //new HttpClientDownloader().Get(uriPage);

            //var uri = new Uri("https://www.goolzoom.com/");
            //var uri = new Uri("https://www.saroga.es/");
            //var uri = new Uri("https://mabelan.es/");
            //var uri = new Uri("https://www.saguar.immo/");
            //var uri = new Uri("https://www.inmolocalgestion.com/");
            //var uri = new Uri("https://www.expimad.com/");
            //var uri = new Uri("https://www.prorealty.es/");

            //var uri = new Uri("http://real-viv.com/");
            var uri = new Uri("http://casavida.es/");
            //var uri = new Uri("https://www.inmobiliariamarbella.es/");


            var website = new Website(uri);
            //var page = new Page(website, uriPage);

            //website.Delete(); return;
            //Websites.DeleteAll(); return;

            //WebsitesInserter.Insert(uri);return;
            //new WebsitesInserter(false).DeleteAndInsert(uri); return;
            //new WebsitesInserter(false).InsertLinksAlternate(uri); return;
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
            //new Scraper().ScrapeNonScrapped();
            //new Scraper().ScrapeUnknowIsListing(uri, true);
            //new Scraper().ScrapeIsNotListing(uri);
            //new Scraper().Scrape(page);
            //new Scraper().GetChrome(page);
            //new Scraper().ScrapeAllPages();

            //Csv.Export(true);
            Json.Export(true);

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