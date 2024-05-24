using HtmlAgilityPack;
using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Downloaders;
using landerist_library.Export;
using landerist_library.Index;
using landerist_library.Insert.GoogleCustomSearch;
using landerist_library.Insert.GooglePlaces;
using landerist_library.Insert.IdAgenciesScraper;
using landerist_library.Landerist_com;
using landerist_library.Logs;
using landerist_library.Parse.Listing.ChatGPT;
using landerist_library.Parse.Location;
using landerist_library.Parse.Location.Delimitations;
using landerist_library.Parse.PageTypeParser;
using landerist_library.Scrape;
using landerist_library.Statistics;
using landerist_library.Tools;
using landerist_library.Websites;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
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
            Config.SetToLocal();
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
            //Config.SetToProduction();
            Config.SetDatabaseToProduction();

            #region Urls

            //var uri = new Uri("https://www.viviendasasturias.es/");
            //var uri = new Uri("https://inmocolsol.es/valladolid/local-comercial-en-alquiler-372235");
            //var page = new Page("http://34mallorca.com/detalles-del-inmueble/carismatico-edificio-en-el-centro-de-palma/19675687");
            //var page = new Page("https://www.inverolid.es/propiedades/piso-en-alquiler-arroyo-de-la-encomienda/");
            //var page = new Page("https://raymarinmobiliaria.com/listing/piso-en-santiago-el-mayor-murcia/");
            //var page = new Page("http://www.bbilbao.es/es/inmueble/alquiler/oficina/areatza/areatza/691?&return=portada");
            //var page = new Page("https://inmobiliariaperelada.com/inmueble/34/casona-");            
            //var page = new Page("https://casasypisosalicante.com/?id=16070639060&amp;idioma=ES");
            //var page = new Page("https://www.viviendasasturias.es/Venta-Piso-Oviedo-Montecerrao-590?idioma=es");
            //var page = new Page("https://www.viviendasasturias.es/Venta-Piso-Oviedo-Montecerrao-590");
            //var page = new Page("https://www.suavisdomus.es/inmuebles/casa-en-morales-del-vino-en-solar-de-114m2/");
            //var page = new Page("https://baronybaron.com/property/c-san-pedro-cabezon-de-la-sal-179m2/");
            //var page = new Page("https://empordaimmo.com/característica/persianas/"); // no listing
            //var page = new Page("https://inmobiliariaalonsodiaz.com/inmueble/piso-en-venta-en-doctor-fleming-la-felguera-langreo/");
            //var page = new Page("https://buscopisos.es/inmueble/venta/piso/cordoba/cordoba/bp01-00250/");


            #endregion

            #region Websites

            //var website = new Website(uri);
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
            //Websites.UpdateNumPages();
            //Websites.Update();

            //Websites.DeleteFromFile();
            //new Website("promoaguilera.com").Delete();
            //new Website("inmobiliariainsignia.com").UpdateListingExample("https://inmobiliariainsignia.com/inmueble/preciosa-casa-reformada-a-capricho-en-la-localidad-urrugne/");
            //new Website("aransahomes.es").RemoveListingExample();
            //Websites.UpdateListingExampleUriFromFile();
            //Websites.TestListingExampleUri();
            //Websites.UpdateListingsExampleNodeSetNulls();
            //Websites.DeleteNullListingExampleHtml();


            #endregion

            #region Pages

            //Pages.DeleteNumPagesExceded();
            //Pages.Delete(PageType.ForbiddenLastSegment);
            //Pages.DeleteDuplicateUriQuery();
            //Pages.DeleteListingsHttpStatusCodeError();
            //Pages.DeleteListingsResponseBodyRepeated();            
            //Pages.UpdateInvalidCadastastralReferences();
            //Pages.UpdateNextUpdate();
            //Pages.RemoveResponseBodyTextHash(PageType.Listing);
            //Pages.DeleteUnpublishedListings();

            #endregion

            #region Scrapper

            //new Scraper(false).ScrapeUnknowPageType(10000);
            //new Scraper().ScrapeMainPage(website);
            //new Scraper().ScrapeUnknowHttpStatusCode();
            //new Scraper(true).ScrapeUnknowIsListing(uri);
            //new Scraper().ScrapeIsNotListing(uri);
            //new Scraper().Scrape(website);
            //new Scraper().ScrapeUnknowPageType(website);
            //new Scraper().ScrapeAllPages();            
            //new Scraper().ScrapeResponseBodyRepeatedInListings();
            //new Scraper().Start();
            //Scraper.Scrape(page); 


            #endregion

            #region Downloaders

            //new HttpClientDownloader().Get(uriPage);
            //PuppeteerDownloader.InstallChrome();
            //PuppeteerDownloader.KillChrome();
            //PuppeteerDownloader.DoTest();            

            #endregion

            #region Parse

            //landerist_library.Parse.Listing.ListingsParser.Start();
            //landerist_library.Parse.Listing.ListingsParser.ParseListing(page);

            //var tuple1 = landerist_library.Parse.Location.GoogleMaps.AddressToLatLng.Parse("Av. Domingo Bueno, 126. O Porriño, 36.400 Pontevedra", CountryCode.ES);
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

            landerist_library.Parse.Listing.VertexAI.ParseListingVertexAI.Test();

            #endregion

            #region Index

            //website.SetSitemap();

            #endregion

            #region Export Backup 

            //Csv.Export(true);
            //landerist_library.Export.Json.Export("es_listings_full.json", true);
            //landerist_library.Landerist_com.FilesUpdater.UpdateFiles();
            //new landerist_library.Landerist_com.DownloadsPage().Update();            
            //landerist_library.Landerist_com.StatisticsPage.Update();
            //landerist_library.Landerist_com.Landerist_com.InvalidateCloudFront();

            //Backup.Update();
            #endregion

            #region Statistics

            //landerist_library.Statistics.StatisticsSnapshot.TakeSnapshots();

            #endregion

            #region Listing Example
            //string url1 = "https://www.inmobiliaria-teval.com/inmueble/salou-apartamento-ln-23523-eva-2/";            
            //string url2 = "https://www.inmobiliaria-teval.com/inmueble/miami-playa-el-casalot-chalet-adosado/";
            //ListingHTMLDom.Test(url1, url2);
            //Csv.ExportHostsMainUri();

            #endregion

            #region Insert

            //PlacesSearch.Search();
            //CustomSearch.Start();
            //InsertIdUrls.Start();
            //GetAgenciesUrls.Start();            
            //ExportAgenciesUrls.Start();

            #endregion

            #region landerist_com

            //Landerist_com.UpdateDownloadsAndStatistics();
            //StatisticsPage.Update();

            #endregion

        }
    }
}