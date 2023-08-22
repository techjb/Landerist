using CsvHelper;
using HtmlAgilityPack;
using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Insert;
using landerist_library.Parse.Listing;
using landerist_library.Scrape;
using landerist_library.Tools;
using landerist_library.Websites;
using OpenQA.Selenium.DevTools.V114.Database;
using System.IO.Compression;
using System.Text;

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
            var uriPage = new Uri("https://www.badainmobiliaria.com/search-form-top/");

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
            var uri = new Uri("https://www.badainmobiliaria.com");

            var website = new Website(uri);
            var page = new Page(website, uriPage);
            //var page = new Page(uriPage);

            //Websites.Delete(website); return;
            //Websites.Delete(uri); return;
            //Websites.DeleteAll(); return;

            //WebsitesInserter.DeleteAndInsert(uri);return;
            //new WebsitesInserter(false).DeleteAndInsert(uri); return;
            //new WebsitesInserter(false).InsertLinksAlternate(uri); return;
            //new CsvInserter(true).InsertBancodedatos_es(); 
            //new CsvInserter(true).InsertBasededatosempresas_net(); // 4985 urls

            //Websites.SetHttpStatusCodesToNull();
            //Websites.InsertUpdateUrisFromNotOk();
            //Websites.SetHttpStatusCodesToAll();            
            //Websites.SetRobotsTxtToHttpStatusCodeOk();
            //Websites.SetIpAdress();            
            //Websites.CountCanAccesToMainUri();
            //Websites.CountRobotsSiteMaps();            
            //Websites.InsertMainPages();

            Scraper.ScrapeUnknowPageType(5000, true);

            //Scraper.ScrapeMainPage(website);
            //Scraper.ScrapeUnknowHttpStatusCode();
            //Scraper.ScrapeUnknowIsListing(uri, true);
            //Scraper.ScrapeIsNotListing(uri);
            //Scraper.Scrape(website);
            //Scraper.ScrapeUnknowPageType(website);
            //Scraper.Scrape(uriPage);
            //Scraper.ScrapeAllPages();

            //Csv.Export(true);
            //Json.Export(true);

            //var tuple1 = landerist_library.Parse.Location.GoogleMaps.AddressToLatLng.Parse("Alondra 8, las rozas de madrid", landerist_library.Websites.CountryCode.ES);
            //Console.WriteLine(tuple1);

            //var tuple1 = landerist_library.Parse.Location.Goolzoom.CadastralRefToLatLng.Parse("9441515XM7094A0001FT");
            //Console.WriteLine(tuple1);

            //var tuple2 = landerist_library.Parse.Location.Goolzoom.CadastralRefToLatLng.Parse("9441515XM7094A");
            //Console.WriteLine(tuple2);

            //landerist_library.Index.ProhibitedUrls.FindNewProhibitedStartsWith();
            //landerist_library.Parse.PageType.LastSegment.FindProhibitedEndsSegments();
            //landerist_library.Parse.PageType.PageTypeParser.ResponseBodyValidToIsListing();
            //landerist_library.Parse.PageType.PageTypeParser.ResponseBodyValidToIsListing(page);

            //landerist_library.Parse.Listing.MLModel.TrainingData.TestData();
            //landerist_library.Parse.Listing.MLModel.TrainingData.CreateIsListing();
            //landerist_library.Parse.Listing.MLModel.TrainingData.CreateListings();
            //landerist_library.Parse.Listing.MLModel.TrainingData.CreateIsListing(1000);
            //landerist_library.Parse.Listing.MLModel.TrainingData.CreateUriResponseBodyText();
            //landerist_library.Parse.Listing.MLModel.IsListingUrl.IsListingUrl.CreateCsv();

            //new landerist_library.Parse.Listing.MLModel.TrainingTests.Danyalktk().Run();
            //new landerist_library.Parse.Listing.MLModel.TrainingTests.AWSComprehend().Run();
            //new landerist_library.Parse.Listing.MLModel.TrainingTests.GoogleCNL().Run();
        }
    }
}