using landerist_library.Insert;
using landerist_library.Scrape;
using landerist_library.Websites;
using landerist_library.Export;
using System.Reflection.Metadata;
using System.Text;
using landerist_library.Parse;
using Newtonsoft.Json;
using landerist_library.Configuration;
using System;
using landerist_library.Logs;
using landerist_library.Database;
using landerist_orels.ES;

namespace landerist_console
{
    internal class Program
    {

        private static DateTime DateStart;

        static void Main(string[] args)
        {
            Console.Title = "Landerist Console";
            SetDateStart();
            Config.Init(false);
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
                "\nDuration: " + (dateFinished - DateStart).ToString(@"hh\:mm\:ss") + ". ";
            Console.WriteLine("\n" + textFinished);
        }

        private static void Run()
        {
            //var uriPage1 = new Uri("https://www.saroga.es/inmueble/chalet-independiente-satelites-majadahonda/");
            //var uriPage2 = new Uri("https://www.saroga.es/inmueble/piso-4-dormitorios-avenida-europa-pozuelo/");
            //var uriPage3 = new Uri("https://www.saroga.es/inmueble/piso-monte-pilar-majadahonda/");
            //var uriPage4 = new Uri("https://www.saroga.es/inmueble/apartamento-majadahonda/");
            var uriPage5 = "https://mabelan.es/proyectos/5384v/";

            //var uri = new Uri("https://www.saroga.es/");
            var uri = new Uri("https://mabelan.es/");
            //var uri = new Uri("https://www.saguar.immo/");
            //var uri = new Uri("https://www.inmolocalgestion.com/");
            //var uri = new Uri("https://www.expimad.com/");

            var website = new Website(uri);
            var page = new Page(website, uriPage5);

            //website.Remove(); return;

            //new UrisInserter().Insert(uri);
            //new UrisInserter().FromCsv();
            //new Websites().RemoveBlockedDomains();

            //new Websites().SetHttpStatusCodesToNull();
            //new Websites().InsertUpdateUrisFromNotOk();
            //new Websites().SetHttpStatusCodesToAll();            
            //new Websites().SetRobotsTxtToHttpStatusCodeOk();
            //new Websites().SetIpAdressToAll();            
            //new Websites().CountCanAccesToMainUri();
            //new Websites().CountRobotsSiteMaps();
            //new Websites().CalculateHashes();
            //new Websites().InsertMainPages();

            //new Scraper().ScrapeMainPage(website);
            //new Scraper().ScrapeNonScrapped(uri);
            //new Scraper().ScrapeUnknowIsListing(uri);
            //new Scraper().ScrapeIsNotListing(uri);
            new Scraper().ScrapePage(page);
            //new Scraper().ScrapeAllPages();

            //Csv.Export(true);
            //Json.Export(true);

            //string guid = "2A82D55502097911496D547A0910B16620AA66244BC774CCA97873C80C6D43BB";
            //var listing1 = ES_Listings.GetListing(guid)!;
            //var listing2 = ES_Listings.GetListing(guid)!;
            //var equals = listing1.Equals(listing2);
        }

        private static void EndBeep()
        {
            while (true)
            {
                Console.Beep(400, 1000);
                Thread.Sleep(300);
            }
        }
    }
}