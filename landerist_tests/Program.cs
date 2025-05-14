using landerist_library.Configuration;
using landerist_library.Downloaders.Puppeteer;
using landerist_library.Logs;
using landerist_library.Parse.ListingParser.OpenAI.Batch;
using landerist_library.Parse.ListingParser.StructuredOutputs;
using landerist_library.Parse.ListingParser.VertexAI;
using landerist_library.Parse.ListingParser.VertexAI.Batch;
using landerist_library.Scrape;
using landerist_library.Statistics;
using landerist_library.Tasks;
using landerist_library.Websites;
using System.Runtime.InteropServices;
using static System.Net.Mime.MediaTypeNames;



namespace landerist_tests
{
    partial class Program
    {
        private static DateTime DateStart;
        private static readonly ServiceTasks ServiceTasks = new();

        private delegate bool ConsoleEventDelegate(int eventType);
        private static readonly ConsoleEventDelegate Handler = new(ConsoleEventHandler);
        [LibraryImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, [MarshalAs(UnmanagedType.Bool)] bool add);

       
        static void Main()
        {
            Console.Title = "Landerist Tests";
            Start();
            Run();            
            End();
        }

        private static void Start()
        {
            SetConsoleCtrlHandler(Handler, true);
            DateStart = DateTime.Now;
            //Log.Delete();
            Log.Console("Started. Version: " + Config.VERSION);
        }

        private static bool ConsoleEventHandler(int eventType)
        {
            if (eventType == 2)
            {
                End();
            }
            return false;
        }

        private static void End()
        {
            ServiceTasks.Stop();
            var duration = (DateTime.Now - DateStart).ToString(@"dd\:hh\:mm\:ss\.fff");
            Log.Console("Stopped. Version: " + Config.VERSION + " Duration: " + duration);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Console.Beep(500, 500);
            }
        }

        private static void Run()
        {
            Config.SetOnlyDatabaseToProduction();

            #region Urls


            //var page = new Page("https://archigestion.com/manuel-perez-lima-realiza-una-firma-de-libros-en-el-cc-martianez-por-el-apoyo-a-la-lectura/");

            // listing 
            //var page = new Page("https://goldacreestates.com/realestate/top/026712-42136");            
            //var page = new Page("https://www.nerjasolproperty.com/es/apartamento-en-venta-en-torrox-costa/80124/s2");
            //var page = new Page("https://baronybaron.com/property/c-san-pedro-cabezon-de-la-sal-179m2/");

            // no listing
            //var page = new Page("https://www.terramagna.net/detallesdelinmueble/villa-en-venta-chayofa/21282480"); // not canonical
            //var page = new Page("https://sanmiguelinmobiliaria.com/caracteristicas/terraza/");
            //var page = new Page("https://www.fincasaldaba.com/propiedad/adosado-llano-samper/foto-16-7-19-10-01-49-copy/"); // repeated in host
            //var page = new Page("https://parba.com/prestación-propiedad/aire-acondicionado/?view=grid"); // response body too short
            //var page = new Page("https://servicasainmo.com/feature/lavadora/");            
            //var page = new Page("http://www.finquesniu.com/propiedades"); // Download error
            //var page = new Page("https://vitalcasa.com/feature/double-glazing/");
            //var page = new Page("https://inmobiliariaareadelsol.com/property-type/shop/");
            //var page = new Page("https://empordaimmo.com/característica/persianas/");
            //var page = new Page("https://www.inmobiliariasomera.com/tag/ayuda/");

            #endregion

            #region Websites

            //var website = new Website(uri);
            //Websites.Delete(website); return;
            //Websites.Delete(uri); return;
            //Websites.DeleteAll(); return;            

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
            //Pages.RemoveResponseBodyTextHashToAll();
            //Pages.RemoveResponseBodyTextHash(PageType.NotListingByLastSegment);
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
            //new Scraper().Scrape(page);
            //new Scraper().DoTest();
            //landerist_library.Scrape.PageSelector.Select();


            #endregion

            #region Downloaders

            //new HttpClientDownloader().Get(uriPage);
            //PuppeteerDownloader.UpdateChrome();
            //PuppeteerDownloader.KillChrome();
            //PuppeteerDownloader.DoTest();            
            //PuppeteerDownloader.UpdateChrome();

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

            //new ChatGPTRequest().ListModels();
            //landerist_library.Parse.Location.LauIdParser.SetLauIdToAllListings();

            //landerist_library.Parse.Listing.VertexAI.ParseListingVertexAI.Test();

            //landerist_library.Parse.ListingParser.Listing.OpenAI.Batch.BatchUpload.Start();
            //landerist_library.Parse.Listing.OpenAI.Batch.BatchDownload.Start();
            //landerist_library.Parse.Listing.OpenAI.Batch.BatchDownload.Test();
            //landerist_library.Parse.Listing.OpenAI.Batch.BatchTasks.Start();            
            //landerist_library.Parse.Listing.OpenAI.Batch.BatchCleaner.DeleteAllRemoteFiles();
            //landerist_library.Parse.Listing.OpenAI.Batch.BatchClient.DeleteFile("dd");

            //landerist_library.Parse.ListingParser.VertexAI.Batch.VertexAIBatch.Test();
            //landerist_library.Parse.ListingParser.VertexAI.Batch.VertexAIBatch.ListAllPredictionJobs();
            //VertexAIBatchCleaner.RemoveFiles();


            #endregion

            #region Index

            //website.SetSitemap();

            #endregion

            #region Backup 

            //Csv.Export(true);
            //landerist_library.Export.Json.Export("es_listings_full.json", true);
            //landerist_library.Landerist_com.FilesUpdater.UpdateFiles();
            //new landerist_library.Landerist_com.DownloadsPage().Update();            
            //landerist_library.Landerist_com.StatisticsPage.Update();
            //landerist_library.Landerist_com.Landerist_com.InvalidateCloudFront();            

            //landerist_library.Database.Backup.Update();
            //Backup.DeleteRemoteOldBackups();
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

            //landerist_library.Insert.FtAgencies.InsertFtUrls.GetProvincesList();
            //landerist_library.Insert.FtAgencies.FtAgenciesInsertUrls.Start();
            //landerist_library.Insert.FtAgencies.FtAgenciesSetAgenciesUrls.Start();
            //landerist_library.Insert.FtAgencies.FtAgenciesExport.Start();
            //landerist_library.Insert.FtAgencies.FtAgenciesInsertWebsites.Start();

            //landerist_library.Insert.BancoDeDatos.InsertBancoDeDatos.Start();
            //landerist_library.Insert.BaseDeDatosEmpresas.InsertBaseDeDatosEmpresas.Start();
            //landerist_library.Insert.IdAgencies.IdAgenciesInsertWebsites.Start();

            //WebsitesInserter.DeleteAndInsert(uri);return;
            //new WebsitesInserter(false).DeleteAndInsert(uri); return;
            //new WebsitesInserter(false).InsertLinksAlternate(uri); return;            


            #endregion

            #region landerist.com

            //landerist_library.Statistics.StatisticsSnapshot.TakeSnapshots();
            //landerist_library.Landerist_com.DownloadFilesUpdater.UpdateFiles();
            //landerist_library.Landerist_com.DownloadFilesUpdater.UpdateUpdates();
            //landerist_library.Landerist_com.Landerist_com.UpdateDownloadsAndStatisticsPages();
            //landerist_library.Landerist_com.Landerist_com.UpdateDownloadsAndStatisticsPages();
            //landerist_library.Landerist_com.StatisticsPage.Update();
            //landerist_library.Landerist_com.Landerist_com.InvalidateCloudFront();


            #endregion

            #region DataBase
            //var d = landerist_library.Database.CountrySpain.Contains(40.4199410000, - 3.6886920000); // true
            //Console.WriteLine(d);

            //var h = landerist_library.Database.CountrySpain.Contains(-3.6886920000, 40.4199410000); // false
            //Console.WriteLine(h);

            #endregion

            #region ServiceTasks

            //ServiceTasks.DailyTask();
            //new ServiceTasks().UpdateAndScrape();
            //ServiceTasks.UpdateAndScrape();
            //ServiceTasks.Update();
            //ServiceTasks.Scrape();
            //ServiceTasks.Start();
            //BatchTasks.Start();
            //BatchUpload.Start();
            //BatchDownload.Start();
            //BatchPredictions.ListAllPredictionJobs();
            //BatchDownload.ReadFileTest();
            //BatchDownload.DownloadVertexAI("projects/942392546193/locations/europe-southwest1/batchPredictionJobs/391166654744100864");
            //VertexAIBatchCleaner.Clean();
            //OpenAIBatchCleaner.RemoveFiles();            

            #endregion

        }
    }
}

