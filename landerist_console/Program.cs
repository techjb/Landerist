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
using System.Windows.Markup;
using static System.Net.Mime.MediaTypeNames;
using System.Text.RegularExpressions;

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
            //var uriPage = new Uri("https://www.saroga.es/inmueble/chalet-independiente-satelites-majadahonda/");
            //var uriPage = new Uri("https://www.saroga.es/inmueble/piso-4-dormitorios-avenida-europa-pozuelo/");
            //var uriPage = new Uri("https://www.saroga.es/inmueble/piso-monte-pilar-majadahonda/");
            //var uriPage = new Uri("https://www.saroga.es/inmueble/apartamento-majadahonda/");          
            var uriPage = new Uri("https://www.inmolocalgestion.com/ficha-inmueble.php?id=53");

            //var uri = new Uri("https://www.goolzoom.com/");
            //var uri = new Uri("https://www.saroga.es/");
            //var uri = new Uri("https://mabelan.es/");
            //var uri = new Uri("https://www.saguar.immo/");
            var uri = new Uri("https://www.inmolocalgestion.com/");
            //var uri = new Uri("https://www.expimad.com/");

            var website = new Website(uri);
            var page = new Page(website, uriPage);



            //website.Remove(); return;

            //new WebsitesInserter(false).RemoveAndInsert(uri); return;
            //new WebsitesInserter(false).Insert(uri);
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
            //new Scraper().ScrapeNonScrapped(uri, true);
            //new Scraper().ScrapeUnknowIsListing(uri, true);
            //new Scraper().ScrapeIsNotListing(uri);
            new Scraper().ScrapePage(page);
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
            while (counter < 3)
            {
                counter++;
                Console.Beep(400, 1000);
                Thread.Sleep(300);
            }
        }
    }
}