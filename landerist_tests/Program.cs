using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Logs;
using landerist_library.Parse.CadastralReference;
using landerist_library.Parse.Location.GoogleMaps;
using landerist_library.Parse.Location.Goolzoom;
using landerist_library.Tasks;
using landerist_library.Websites;
using landerist_orels.ES;
using System.Reflection;
using System.Runtime.InteropServices;



namespace landerist_tests
{
    partial class Program
    {
        private static DateTime DateStart;
        private static readonly TasksService ServiceTasks = new();

        private delegate bool ConsoleEventDelegate(int eventType);
        private static readonly ConsoleEventDelegate Handler = new(ConsoleEventHandler);
        [LibraryImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, [MarshalAs(UnmanagedType.Bool)] bool add);
        public delegate void KeyPressedHandler(ConsoleKeyInfo key);
        public static event KeyPressedHandler? OnKeyPressed;

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
            SetCtrlDListener();

            DateStart = DateTime.Now;
            //Log.Delete();
            Log.Console("Started. Version: " + Config.VERSION);
        }

        static void SetCtrlDListener()
        {
            OnKeyPressed += keyInfo =>
            {
                Console.WriteLine("si  " + keyInfo.Key + " " + keyInfo.Modifiers);
                if ((keyInfo.Modifiers & ConsoleModifiers.Control) != 0 &&
                    keyInfo.Key == ConsoleKey.D)
                {
                    Console.WriteLine("¡Ctrl + D detectado!");
                }
            };
            Thread inputThread = new(KeyboardListener);
            inputThread.Start();
        }
        static void KeyboardListener()
        {
            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                OnKeyPressed?.Invoke(keyInfo);

                if (keyInfo.Key == ConsoleKey.Escape)
                    Environment.Exit(0);
            }
        }

        private static bool ConsoleEventHandler(int eventType)
        {
            Console.WriteLine(eventType);
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

            //Thread.Sleep(10000);

            #region Urls


            //var page = new Page("https://archigestion.com/manuel-perez-lima-realiza-una-firma-de-libros-en-el-cc-martianez-por-el-apoyo-a-la-lectura/");
            //var page = new Page("https://www.nicehousebyabagyan.com/house/4142004-flat-for-sale-in-lhospitalet-de-llobregat-of-54-m2-248990eur-BAI248/");
            //var page = new Page("http://www.mrbibendo.com/ad/94943189");

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
            //Websites.UpdateRobotsTxt();


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
            //Pages.DeleteUrisLikePrint();
            //Pages.DeleteProhibitedUris();
            //new Page("https://areagestio.com/propiedades/25974555/").Insert();
            

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
            //new Scraper().Scrape(page, false);
            //new landerist_library.Scrape.Scraper().Scrape("https://serenityhouses.es/property/chalet-independiente-en-molino-de-la-hoz-para-entrar-a-vivir/", false);
            //new Scraper().DoTest();
            //landerist_library.Scrape.PageSelector.SelectTop1();
            //Console.WriteLine("Block: " + WebsitesBlocker.Block(page.Website));
            //Console.WriteLine("IsBlocked: " + WebsitesBlocker.IsBlocked(page.Website));        



            #endregion

            #region Downloaders

            //new HttpClientDownloader().Get(uriPage);
            //landerist_library.Downloaders.Puppeteer.PuppeteerDownloader.UpdateChrome();
            //PuppeteerDownloader.KillChrome();
            //PuppeteerDownloader.DoTest();            
            //PuppeteerDownloader.UpdateChrome();

            #endregion

            #region ListingParser

            //landerist_library.GetLatLng.Listing.ListingsParser.Start();
            //landerist_library.GetLatLng.Listing.ListingsParser.ParseListing(page);


            //landerist_library.Index.ProhibitedUrls.FindNewProhibitedStartsWith();
            //landerist_library.GetLatLng.PageType.LastSegment.FindProhibitedEndsSegments();

            //landerist_library.GetLatLng.PageType.PageTypeParser.ResponseBodyValidToIsListing();
            //landerist_library.GetLatLng.PageType.PageTypeParser.ResponseBodyValidToIsListing(page);            

            //new ChatGPTRequest().ListModels();
            //landerist_library.GetLatLng.Location.LauIdParser.SetLauIdToAllListings();

            //landerist_library.GetLatLng.Listing.VertexAI.ParseListingVertexAI.Test();

            //landerist_library.GetLatLng.ListingParser.Listing.OpenAI.Batch.BatchUpload.Start();
            //landerist_library.GetLatLng.Listing.OpenAI.Batch.BatchDownload.Start();
            //landerist_library.GetLatLng.Listing.OpenAI.Batch.BatchDownload.Test();
            //landerist_library.GetLatLng.Listing.OpenAI.Batch.BatchTasks.Start();            
            //landerist_library.GetLatLng.Listing.OpenAI.Batch.BatchCleaner.DeleteAllRemoteFiles();
            //landerist_library.GetLatLng.Listing.OpenAI.Batch.BatchClient.DeleteFile("dd");

            //landerist_library.GetLatLng.ListingParser.VertexAI.Batch.VertexAIBatch.Test();
            //landerist_library.GetLatLng.ListingParser.VertexAI.Batch.VertexAIBatch.ListAllPredictionJobs();
            //VertexAIBatchCleaner.RemoveFiles();


            #endregion

            #region LocationParser

            //var tuple1 = landerist_library.GetLatLng.Location.GoogleMaps.AddressToLatLng.GetLatLng("Av. Domingo Bueno, 126. O Porriño, 36.400 Pontevedra", CountryCode.ES);
            //Console.WriteLine(tuple1);

            //var tuple1 = landerist_library.GetLatLng.Location.Goolzoom.CadastralRefToLatLng.GetLatLng("9441515XM7094A0001FT");
            //Console.WriteLine(tuple1);

            //var tuple2 = landerist_library.GetLatLng.Location.Goolzoom.CadastralRefToLatLng.GetLatLng("9441515XM7094A");
            //Console.WriteLine(tuple2);

            //Console.WriteLine(landerist_library.Tools.Validate.CadastralReference("3979515DD7737H0002LX"));
            //landerist_library.Tools.Validate.RemoveInvalidCatastralReferences();

            //string address = "Calle Castillo de Fuensaldaña 4, Las Rozas de Madrid";
            //string address = "Calle Juan Sebastian Elcano 14 - Local 3, Sevilla";
            //var latLNg = new GoogleMapsApi().GetLatLng(address, CountryCode.ES);
            //var cadastralReference = new AddressToCadastralReference().GetCadastralReference(latLNg.Value.latLng.Item1, latLNg.Value.latLng.Item2, address);

            //Console.WriteLine(d.latLng.ToString() + " " +  d.isAccurate);
            //GoogleMapsApi.UpdateListingsLocationIsAccurate();
            //CadastralRefToLatLng.UpdateLocationFromCadastralRef();
            //Console.WriteLine(new CadastralRefToLatLng().GetLatLng("F239324UK8141N0001HP"));
            //Console.WriteLine(new GoolzoomApi().GetAddrees("7979409YJ1677N0005BE"));
            //GoolzoomApi.UpdateAddressFromCadastralRef();
            //AddressToCadastralReference.UpdateCadastralReferences();            
            //var listing = ES_Listings.GetListing("0074C7FF345F923A06992C15431EA2630A114713CC96D6DDA8DE35372286902A");
            //new landerist_library.GetLatLng.Location.GoogleMaps.GoogleMapsApi().GetLatLng(listing.address);
            //new GoolzoomApi().GetAddresses(40.4243178, -3.7021782, 50);


            #endregion

            #region Index

            //website.SetSitemap();

            #endregion

            #region Backup 
            //landerist_library.Database.Backup.Update();
            //Backup.DeleteRemoteOldBackups();
            #endregion

            #region Statistics
            //landerist_library.Statistics.StatisticsSnapshot.InsertDailyCounter(StatisticsKey.ScrapedSuccess, 23);
            #endregion

            #region Listings
            //var page = new Page("https://www.nerjasolproperty.com/es/apartamento-en-venta-en-torrox-costa/80124/s2");
            //var listing1 = page.GetListing(true, true);

            //var source = new landerist_orels.Source()
            //{
            //    sourceName = "www.nerjasolproperty.com",
            //    sourceUrl = new Uri("https://www.nerjasolproperty.com/es/apartamento-en-venta-en-torrox-costa/80124/s2"),
            //    sourceGuid = "4196"
            //};
            //listing1.AddSource(source);            
            //ES_Listings.InsertUpdate(page.Website, listing1);
            //ES_Sources.FixListingsWhitoutSource();


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
            //landerist_library.Statistics.StatisticsSnapshot.HttpStatusCode();
            //landerist_library.Statistics.StatisticsSnapshot.SnapshotHttpStatusCode7Days();
            //landerist_library.Statistics.StatisticsSnapshot.SnapshotPageType7Days();

            //DownloadFilesUpdater.Update();
            //landerist_library.Landerist_com.FilesUpdater.Update();
            //landerist_library.Landerist_com.FilesUpdater.UpdatePublished();
            //landerist_library.Landerist_com.FilesUpdater.UpdateUnpublished();
            //landerist_library.Landerist_com.FilesUpdater.UpdateUpdates();
            //landerist_library.Landerist_com.FilesUpdater.UpdateWebsites();
            //landerist_library.Landerist_com.Landerist_com.UpdateStatistics();
            //landerist_library.Landerist_com.Landerist_com.UpdateDownloads();
            //landerist_library.Landerist_com.Landerist_com.UpdatePages();
            //landerist_library.Landerist_com.Landerist_com.UpdatePages();
            //landerist_library.Landerist_com.StatisticsPage.Update();
            //landerist_library.Landerist_com.Landerist_com.InvalidateCloudFront();


            #endregion

            #region DataBase
            //var d = landerist_library.Database.CountrySpain.Contains(40.4199410000, - 3.6886920000); // true
            //Console.WriteLine(d);

            //var h = landerist_library.Database.CountrySpain.Contains(-3.6886920000, 40.4199410000); // false
            //Console.WriteLine(h);            

            #endregion

            #region Tasks

            //ServiceTasks.DailyTask();
            //new ServiceTasks().UpdateAndScrape();
            //DownloadFilesUpdater.Update();
            //ServiceTasks.UpdateAndScrape();
            //ServiceTasks.HourlyTasks();
            //ServiceTasks.Scrape();
            //ServiceTasks.Start();
            //BatchTasks.Start();            
            //TaskBatchDownload.Start();
            //TaskBatchUpload.Start(false);
            //BatchPredictions.ListAllPredictionJobs();
            //BatchDownload.ReadFileTest();
            //BatchDownload.DownloadVertexAI("projects/942392546193/locations/europe-southwest1/batchPredictionJobs/391166654744100864");
            //VertexAIBatchCleaner.Clean();
            //OpenAIBatchCleaner.RemoveFiles();
            //FilesUpdater.UpdateWebsites();
            //var page = Pages.GetPage("850E272404903B49361120C9F468694C4C0F1975C141111CF8334C8F04A75727");
            //TaskBatchUpload.GetJson(page);


            #endregion

        }
    }
}

