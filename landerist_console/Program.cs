using Amazon.Auth.AccessControlPolicy.ActionIdentifiers;
using landerist_library.Configuration;
using landerist_library.Logs;
using landerist_library.Scrape;
using landerist_library.Tasks;
using landerist_library.Websites;
using System.Runtime.InteropServices;
using static System.Net.Mime.MediaTypeNames;

namespace landerist_console
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
            Console.Title = "Landerist Console";
            Start();
            Run();
            Console.WriteLine("ReadLine to exit..");
            Console.ReadLine(); 
            End();
        }

        private static void Start()
        {
            SetConsoleCtrlHandler(Handler, true);
            DateStart = DateTime.Now;
            Log.Delete();
            Log.WriteInfo("landerist_console", "Started. Version: " + Config.VERSION);
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
            Log.WriteInfo("landerist_service", "Stopped. Version: " + Config.VERSION + " Duration: " + duration);

#pragma warning disable CA1416 // Validate platform compatibility
            Console.Beep(500, 500);
#pragma warning restore CA1416 // Validate platform compatibility
        }

        private static void Run()
        {
            Config.SetToProduction();
            //Config.SetOnlyDatabaseToProduction();

            #region Urls

            //var uri = new Uri("https://www.viviendasasturias.es/");            

            //var page = new Page("https://baronybaron.com/property/c-san-pedro-cabezon-de-la-sal-179m2/"); //listing
            // var page = new Page("https://empordaimmo.com/característica/persianas/"); // no listing
            //var page = new Page("https://www.inmobiliariasomera.com/tag/ayuda/"); // no listing
            //var page = new Page("https://parba.com/prestación-propiedad/aire-acondicionado/?view=grid"); // no listing
            //var page = new Page("https://servicasainmo.com/feature/lavadora/"); // no listing
            //var page = new Page("https://veovall.com/property/valladolid/"); // no listing

            //var page = new Page("https://www.fincasaldaba.com/propiedad/adosado-llano-samper/foto-16-7-19-10-01-49-copy/");
            //var page = new Page("https://www.12casas.com/inmueble/E869/"); 
            //var page = new Page("https://sanmiguelinmobiliaria.com/caracteristicas/terraza/");
            //var page = new Page("https://www.terramagna.net/detallesdelinmueble/villa-en-venta-chayofa/21282480"); // not canonical
            //var page = new Page("https://vitalcasa.com/feature/double-glazing/"); // no listing
            //var page = new Page("http://www.finquesniu.com/propiedades");
            //var page = new Page("https://www.mardenia-inmobiliaria.com/venta/casa-en-venta-en-sagra-638/");// listing
            //var page = new Page("https://goldacreestates.com/realestate/top/026712-42136"); // listing
            //var page = new Page("https://buscopisos.es/inmueble/venta/piso/cordoba/cordoba/bp01-00250/"); // listing            


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
            //var Timer3 = new Timer(TimerCallback3!, null, 0, 1000);
            //new Scraper().Start();            
            //Thread.Sleep(100000000);
            //Scraper.Scrape(page);
            //new Scraper().DoTest();


            #endregion

            #region Downloaders

            //new HttpClientDownloader().Get(uriPage);
            //PuppeteerDownloader.ReinstallChrome();
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

            //landerist_library.Parse.Listing.VertexAI.ParseListingVertexAI.Test();

            //landerist_library.Parse.Listing.OpenAI.Batch.BatchUpload.Start();
            //landerist_library.Parse.Listing.OpenAI.Batch.BatchDownload.Start();
            //landerist_library.Parse.Listing.OpenAI.Batch.BatchDownload.Test();


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

            //Backup.Update();
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

            #region landerist_com

            //landerist_library.Statistics.StatisticsSnapshot.TakeSnapshots();
            //DownloadFilesUpdater.UpdateFiles();
            //DownloadFilesUpdater.UpdateUpdates();
            //Landerist_com.UpdateDownloadsAndStatisticsPages();
            //StatisticsPage.Update();

            #endregion

            #region DataBase
            //var d = CountrySpain.Contains(40.4199410000, - 3.6886920000); // true
            //Console.WriteLine(d);

            //var h = CountrySpain.Contains(-3.6886920000, 40.4199410000); // false
            //Console.WriteLine(h);

            #endregion

            #region ServiceTasks

            //ServiceTasks.DailyTask();
            //new ServiceTasks().UpdateAndScrape();
            //ServiceTasks.UpdateAndScrape();
            //ServiceTasks.Scrape();
            ServiceTasks.Start();

            #endregion

        }
    }
}