using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Parse.Listing.ChatGPT;
using landerist_library.Scrape;
using landerist_library.Websites;
using OpenAI;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;


namespace landerist_console
{
    internal class Program
    {
        private static DateTime DateStart;

        static void Main()
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

#pragma warning disable CA1416 // Validate platform compatibility
            Console.Beep(500, 500);
#pragma warning restore CA1416 // Validate platform compatibility
        }

        private static void Run()
        {
            //var uriPage = new Uri("https://www.badainmobiliaria.com/search-form-top/");

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
            //var uri = new Uri("https://www.badainmobiliaria.com");

            //var website = new Website(uri);
            //var page = new Page(website, uriPage);
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

            //new Scraper(false).ScrapeUnknowPageType(10000);

            //new Scraper().ScrapeMainPage(website);
            //new Scraper().ScrapeUnknowHttpStatusCode();
            //new Scraper(true).ScrapeUnknowIsListing(uri);
            //new Scraper().ScrapeIsNotListing(uri);
            //new Scraper().Scrape(website);
            //new Scraper().ScrapeUnknowPageType(website);
            //new Scraper().Scrape(uriPage);
            //new Scraper().ScrapeAllPages();

            //landerist_library.Parse.Listing.ListingsParser.Start();            

            //Csv.Export(true);
            //landerist_library.Export.Json.Export("es_listings_full.json", true);

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
            //landerist_library.Parse.Listing.MLModel.TrainingData.CreateIsListing();

            //landerist_library.Parse.Listing.IsListingTest.TestFTTC.Start();
            //landerist_library.Parse.Listing.IsListingTest.TestLMStudio.Start();

            //new landerist_library.Parse.Listing.MLModel.TrainingTests.Danyalktk().Run();
            //new landerist_library.Parse.Listing.MLModel.TrainingTests.AWSComprehend().Run();
            //new landerist_library.Parse.Listing.MLModel.TrainingTests.GoogleCNL().Run();

            //new ChatGPTRequest().ListModels();

            //landerist_library.Export.Exports.Start();

            //Backup.Update();

            //landerist_library.Statistics.StatisticsSnapshot.TakeSnapshots();
        }
    }
}