using landerist_library.Configuration;
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
            Start();
            Run();
            End();
        }

        private static void Start()
        {
            DateStart = DateTime.Now;
            string textStarted =
                "STARTED at " + DateStart.ToShortDateString() + " " + DateStart.ToString(@"hh\:mm\:ss") + "\n";
            Console.WriteLine(textStarted);
        }

        private static void End()
        {
            DateTime dateFinished = DateTime.Now;
            string textFinished =
                "FINISHED at " + dateFinished.ToShortDateString() + " " + dateFinished.ToString(@"hh\:mm\:ss") +
                "\nDuration: " + (dateFinished - DateStart).ToString(@"dd\:hh\:mm\:ss\.fff") + ". ";
            Console.WriteLine("\n" + textFinished);

            Console.Beep(500, 500);
        }

        private static void Run()
        {
            //var uriPage = new Uri("https://www.goolzoom.com/es/valores/");

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
            //var uri = new Uri("http://casavida.es/");
            //var uri = new Uri("https://www.inmobiliariamarbella.es/");
            //var uri = new Uri("https://www.inmoarregi.com/");

            //var website = new Website(uri);
            //var page = new Page(website, uriPage);
            //var page = new Page(uriPage);

            //Websites.Delete(website); return;
            //Websites.Delete(uri); return;
            //Websites.DeleteAll(); return;

            //WebsitesInserter.Insert(uri);return;
            //new WebsitesInserter(false).DeleteAndInsert(uri); return;
            //new WebsitesInserter(false).InsertLinksAlternate(uri); return;
            //new CsvInserter(true).InsertBancodedatos_es(); 
            //new CsvInserter(true).InsertBasededatosempresas_net();

            //Websites.SetHttpStatusCodesToNull();
            //Websites.InsertUpdateUrisFromNotOk();
            //Websites.SetHttpStatusCodesToAll();            
            //Websites.SetRobotsTxtToHttpStatusCodeOk();
            //Websites.SetIpAdress();            
            //Websites.CountCanAccesToMainUri();
            //Websites.CountRobotsSiteMaps();            
            //Websites.InsertMainPages();

            //Scraper.ScrapeMainPage(website);
            //Scraper.ScrapeNonScrapped(10000, false);
            //Scraper.ScrapeUnknowHttpStatusCode();
            //Scraper.ScrapeUnknowIsListing(uri, true);
            //Scraper.ScrapeIsNotListing(uri);
            //Scraper.Scrape(website);
            //Scraper.Scrape(uriPage);
            //Scraper.GetChrome(page);
            //Scraper.ScrapeAllPages();

            //Csv.Export(true);
            //Json.Export(true);

            //var tuple1 = landerist_library.Parse.Location.GoogleMaps.AddressToLatLng.Parse("Alondra 8, las rozas de madrid", landerist_library.Websites.CountryCode.ES);
            //Console.WriteLine(tuple1);

            //var tuple1 = landerist_library.Parse.Location.Goolzoom.CadastralRefToLatLng.Parse("9441515XM7094A0001FT");
            //Console.WriteLine(tuple1);

            //var tuple2 = landerist_library.Parse.Location.Goolzoom.CadastralRefToLatLng.Parse("9441515XM7094A");
            //Console.WriteLine(tuple2);

            //landerist_library.Parse.Listing.MLModel.TrainingData.Create();
            //landerist_library.Parse.Listing.MLModel.TrainingData.CreateIsListing();
            //landerist_library.Parse.Listing.MLModel.TrainingData.CreateListings();

            Pages.CleanText();
        }
    }
}