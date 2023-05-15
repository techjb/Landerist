using landerist_library.Configuration;
using landerist_library.Download;
using landerist_library.Export;
using landerist_library.Insert;
using landerist_library.Parse.Location;
using landerist_library.Scrape;
using landerist_library.Websites;
using landerist_orels.ES;
using PuppeteerSharp;

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
            var uriPage = new Uri("https://www.inmobiliariamarbella.es/inmuebles/local-en-marbella-p4_l24/");

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
            var uri = new Uri("https://www.inmobiliariamarbella.es/");

            ////var website = new Website(uri);
            //var page = new Page(website, uriPage);
            //var page = new Page(uriPage);

            //Websites.Delete(uri); return;
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

            //Scraper.ScrapeMainPage(website);
            //Scraper.ScrapeNonScrapped();
            //Scraper.ScrapeUnknowHttpStatusCode();
            //Scraper.ScrapeUnknowIsListing(uri, true);
            //Scraper.ScrapeIsNotListing(uri);
            //Scraper.Scrape(website);
            Scraper.Scrape(uriPage);
            //Scraper.GetChrome(page);
            //Scraper.ScrapeAllPages();

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