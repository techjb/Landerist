using Amazon.CloudFront.Model;
using Google.Cloud.AIPlatform.V1;
using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Downloaders.Puppeteer;
using landerist_library.Logs;
using landerist_library.Parse.CadastralReference;
using landerist_library.Parse.ListingParser;
using landerist_library.Parse.ListingParser.LocalAI;
using landerist_library.Parse.ListingParser.StructuredOutputs;
using landerist_library.Parse.ListingParser.VertexAI;
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


            new landerist_library.Scrape.Scraper().Scrape("https://dosagui.com/es/property/15746", true);
            //new landerist_library.Scrape.Scraper().Scrape("https://20punto20rb.com/propiedades/propiedad/luminoso-piso-en-don-ramon-de-la-cruz/", false);

            //new landerist_library.Scrape.Scraper().Scrape("https://1mast.com/es/pisos-en-coin/", false);// not listing
            //new landerist_library.Scrape.Scraper().Scrape("https://360mallorcaproperty.com/es/property-location/porto-petro-es/page/2/", false);// not listing


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
            //landerist_library.Parse.Location.LauIdParser.SetLauIdAndLauNameToListings();

            //landerist_library.GetLatLng.Listing.VertexAI.ParseListingVertexAI.GetResponse();

            //landerist_library.GetLatLng.ListingParser.Listing.OpenAI.Batch.BatchUpload.Start();
            //landerist_library.GetLatLng.Listing.OpenAI.Batch.BatchDownload.Start();
            //landerist_library.GetLatLng.Listing.OpenAI.Batch.BatchDownload.GetResponse();
            //landerist_library.GetLatLng.Listing.OpenAI.Batch.BatchTasks.Start();            
            //landerist_library.GetLatLng.Listing.OpenAI.Batch.BatchCleaner.DeleteAllRemoteFiles();
            //landerist_library.GetLatLng.Listing.OpenAI.Batch.BatchClient.DeleteFile("dd");

            //landerist_library.GetLatLng.ListingParser.VertexAI.Batch.VertexAIBatch.GetResponse();
            //landerist_library.GetLatLng.ListingParser.VertexAI.Batch.VertexAIBatch.ListAllPredictionJobs();
            //VertexAIBatchCleaner.RemoveFiles();

            string text = "<!DOCTYPE html><html class=\"no-js\" lang=\"es\" dir=\"ltr\"><head> <meta charset=\"utf-8\"> <meta name=\"title\" content=\"Dosagui\"> <meta name=\"DC.title\" content=\"Dosagui\"> <meta http-equiv=\"title\" content=\"Dosagui\"> <meta name=\"keywords\" content=\"Dosagui\"> <meta name=\"DC.Keywords\" content=\"Dosagui\"> <meta name=\"description\" content=\"Dosagui\"> <meta name=\"DC.Description\" content=\"Dosagui\"> <meta name=\"distribution\" content=\"global\"> <meta name=\"resource-type\" content=\"Document\"> <meta name=\"Robots\" content=\"all\"> <meta http-equiv=\"Pragma\" content=\"cache\"> <meta name=\"Revisit\" content=\"20 days\"> <meta name=\"robots\" content=\"all\"> <meta name=\"language\" content=\"es\"> <meta http-equiv=\"content-language\" content=\"es\"> <meta name=\"viewport\" content=\"user-scalable=no, width=device-width, initial-scale=1\"> <title>Dosagui</title> <!-- FAV-ICON I SITEMAP--> <!--jquery--> <!-- captcha--> <!-- bootstrap --> <!-- SELECTS BOOTSTRAP --> <!-- carousel--> <!-- lightview --> <!--Lightbox--> <!--Magnific--> <!-- google maps i street view--> <!-- COOKIES --> </head><body> <!--datepicker--> <!-- front class js --> <!-- css --> <header> <div class=\"header-capa\"> <div class=\"container-fluid container-marges\"> <section class=\"row\"> <div class=\"col-md-12 col-sm-12\" id=\"header\"> <nav class=\"col-sm-2 col-md-2 col-xs-12 col-logo col-md-offset-0 \"> <a href=\"/\"><img src=\"/images/icons/logo.png\" title=\"logo\" alt=\"Dosagui\" class=\"logo\"></a> <span class=\"lang\"><a href=\"/es/home\"><i class=\"ico_lang es\"></i></a></span> <span class=\"lang\"><a href=\"/en/home\"><i class=\"ico_lang en\"></i></a></span> <span class=\"lang\"><a href=\"/de/home\"><i class=\"ico_lang de\"></i></a></span> </nav> <!-- MENU NAVBAR --> <nav class=\"navbar navbar-default col-sm-10 col-md-10 col-xs-5 \" role=\"navigation\"> <div class=\"navbar-header\"> </div> <div class=\"collapse navbar-collapse navbar-ex1-collapse \"> <div class=\"menu col-sm-12\"> <!-- MENU --> <span class=\"item item-menu\"><a id=\"m_home\" href=\"/es/home\">Inicio</a></span> <span class=\"item item-menu\"><a id=\"m_propiedades_0\" href=\"/es/venta\">Venta</a></span> <span class=\"item item-menu\"><a id=\"m_propiedades_1\" href=\"/es/alquiler\">Alquiler</a></span> <!--span class=\"item item-menu\"><a id=\"m_blog\" href=\"/es/blog\">Blog</a></span--> <!--span class=\"item item-menu\"><a target=\"_blank\" id=\"\" href=\"https://ferienmietedosagui.com/\" >Alquiler vacacional</a></span--> <span class=\"item item-menu\"><a id=\"m_contacto\" href=\"/es/contacto\">Contacto</a></span> <span class=\"item item-menu\"><a id=\"m_wishlist\" href=\"/es/wishlist\">Favoritos</a></span> <!-- FI MENU --> <span class=\"item item-menu langs\"><a href=\"/es/home\"><i class=\"icon lang ico-es\"></i></a></span> <span class=\"item item-menu langs\"><a href=\"/en/home\"><i class=\"icon lang ico-en\"></i></a></span> <span class=\"item item-menu langs\"><a href=\"/de/home\"><i class=\"icon lang ico-de\"></i></a></span> </div> </div> </nav> </div> </section> </div> </div> </header><!--##### PROPIETAT ####--> <section id=\"propietat\"> <div class=\"container-fluid container-marges\"> <div class=\"row primer-row\"> <div class=\"col-md-9 col-sm-12\"> <div class=\"col-sm-12\"> <div class=\"img-top\"> <div class=\"cinta \"> <div class=\"row\"> <div class=\"col-md-3 col-sm-6 col-xs-7\"> <i id=\"botoWish\" class=\"icon-wish\" title=\"Favoritos\"></i> <span class=\"wishlist\">Añadir a Favoritos</span> <!--span>REF. 548</span--> </div> <div class=\"col-md-3 col-sm-6 col-xs-5\">550.000€</div> </div> </div> </div> </div> <div class=\"col-sm-12\"> <!-- #### GALERIA ###--> <div class=\"col-sm-12 galeria owl-carousel owl-theme owl-loaded\" id=\"sliderProperty\"> <div class=\"owl-stage-outer\"><div class=\"owl-stage\"><div class=\"owl-item cloned\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-jfypg.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item cloned\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-4mm02.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item cloned\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-5wp0k.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item active\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-kfjz6.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item active\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-6zv1d.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item active\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-2qwhj.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-5jn3p.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-8y26k.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-vvtyj.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-08dbd.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-62pkk.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-4wmkv.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-mfpm6.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-pmwr8.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-tx4qs.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-xqq7s.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-cmtfh.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-vnq35.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-xn87b.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-f11jm.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-g3vz0.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-kp4pq.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-1bqmc.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-1yn78.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-cqh7s.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-w1m94.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-k2nqr.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-mwzr9.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-kwtt4.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-yfz5t.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-0f3x1.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-kmbsg.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-bxxmj.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-bkbq1.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-99kgk.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-sxrqn.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-w1rtv.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-bqnzb.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-7nq57.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-hx97d.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-3ywgc.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-mqnj5.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-yxx8y.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-ngysz.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-9v0xw.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-0701x.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-gsjqv.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-mtb8t.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-sfq1q.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-yjdzc.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-k5w4y.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-spt8x.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-wqdc8.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-jfypg.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-4mm02.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-5wp0k.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item cloned\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-kfjz6.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item cloned\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-6zv1d.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div><div class=\"owl-item cloned\"><a href=\"https://ferienmietedosagui.com/panel-inmobiliaria/img/rooms/room-15746-2qwhj.jpeg\"> <div class=\"img-property\" title=\"Dosagui\" alt=\"Dosagui\"></div> </a></div></div></div></div> </div> </div> <div class=\"col-md-3 col-sm-12 col-xs-12 \"> <form id=\"form_cercador\" action=\"/es/venta\" method=\"post\"> <div class=\"row\" id=\"cercador\"> <div class=\"col-sm-3 col-md-3 mapa\"> </div> <div class=\"col-md-8 col-md-offset-1 col-sm-9 inputs\"> <div class=\"row\"> <div class=\"col-sm-3\"></div> <div class=\"col-sm-3\"> <div class=\"btn-group bootstrap-select\"></div> </div> <div class=\"col-sm-3\"> <div class=\"btn-group bootstrap-select\"></div> </div> <div class=\"col-sm-3\"> <div class=\"btn-group bootstrap-select\"></div> </div> </div> <div class=\"row\"> <div class=\"col-sm-3 bottom\"> <!--div class=\"info-slider\"> <small>Capacidad</small> <small><span id=\"min-capacidad\">2</span> - <span id=\"max-capacidad\">9</span> habitaciones</small> </div> <input type=\"hidden\" name=\"capacidad\" id=\"capacidad-input\" value=\"2-9\"> <div id=\"capacidad\"></div--> <div class=\"btn-group bootstrap-select\"></div> </div> <div class=\"col-sm-3 bottom\"> <div class=\"col-sm-6 cerca_preu_min\"> <div class=\"btn-group bootstrap-select\"></div> </div> <div class=\"col-sm-6 cerca_preu_max\"> <div class=\"btn-group bootstrap-select\"></div> </div> </div> <div class=\"col-sm-3 bottom\"> <!--div class=\"info-slider\"> <small>Baños</small> <small><span id=\"min-banyos\">2</span> - <span id=\"max-banyos\">9</span> baños</small> </div> <input type=\"hidden\" name=\"banyos\" id=\"banyos-input\" value=\"2-9\"> <div id=\"banyos\"></div--> <div class=\"btn-group bootstrap-select\"></div> </div> <div class=\"col-sm-3\"></div> </div> </div> </div> </form> </div> </div> </div> <div class=\"container-fluid container-marges\"> <div class=\"row\"> <div class=\"col-md-12 col-sm-12 col-xs-12\"> <!-- #### DESC + INFO ###--> <div class=\"row info\"> <div class=\"col-md-9 col-sm-8\"> <h2>Duplex en Cala Ratajada con solarim privado</h2> <small>RF:548</small> <p>Bonito apartamento tipo dúplex de unos104 m² con un generoso solárium con acceso directo desde la segunda planta de la vivienda y vistas despejadas al mar y la montaña, pertenece a una pequeña comunidad de vecinos que funciona perfectamente, y se encuentra ubicada en una zona bastante tranquila y a la vez a escasos minutos caminando hasta el puerto y el paseo marítimo de la bonita localidad turística y pesquera de Cala Ratjada, donde se puede disfrutar de infinidad de locales comerciales dedicados a diferentes tipos de negocios tanto de ocio como de servicios. </p> <p>El inmueble está ubicado en una segunda planta con ascensor y está distribuido en su primera planta en salón comedor, cocina independiente amueblada y totalmente equipada, lavanderia, cuarto de baño con plato de ducha y un balcón; En su segundo piso nos encontramos dos dormitorios dobles uno de ellos con un ámplio vestidor, un cuarto de baño con bañera y la escalera con acceso directo al solárium, un porche cerrado y un pequeño trastero.</p> <p>En la planta baja a pié de calle, le pertenece un garage cerrado de unos 26 m² y una plaza de parking exterior.</p> <small>Características</small> <ul class=\"ul_caracteristiques\"> <li class=\"col-md-4 col-sm-6\">Aire/ Acodicionado</li><li class=\"col-md-4 col-sm-6\">Nevera</li><li class=\"col-md-4 col-sm-6\">Amueblado</li><li class=\"col-md-4 col-sm-6\">Armarios empotrados</li><li class=\"col-md-4 col-sm-6\">Ventanas doble cristal</li><li class=\"col-md-4 col-sm-6\">Balcón</li><li class=\"col-md-4 col-sm-6\">Vistas al mar</li><li class=\"col-md-4 col-sm-6\">Solarium</li><li class=\"col-md-4 col-sm-6\">Ascensor</li><li class=\"col-md-4 col-sm-6\">Horno</li> </ul> </div> <div class=\"col-md-3 col-sm-4\"> <div class=\"caracteristiques\"> <div class=\"info col-sm-12\"> <!--div class=\"contenedorInfoIconos\"> <div class=\"iconos persones\"></div> <div class=\"text personesText\">0</div> </div--> <div class=\"contenedorInfoIconos\"> <div class=\"iconos banyera\"></div> <div class=\"text banysText\">02</div> </div> <div class=\"contenedorInfoIconos\"> <div class=\"iconos llit\"></div> <div class=\"text habitacionsText\">03</div> </div> <div></div> </div> <div class=\"col-sm-12 lista\"> <small>Descripción</small> <div class=\"row\"><div class=\"col-xs-6\">Referencia:</div><div class=\"col-xs-6 dreta\">548</div></div> <div class=\"row\"><div class=\"col-xs-6\">Precio:</div><div class=\"col-xs-6 dreta\">550000€</div></div> <div class=\"row\"><div class=\"col-xs-6\">Tipo de inmueble:</div><div class=\"col-xs-6 dreta\">Áticos</div></div> <!--div class=\"row\"><div class=\"col-xs-6\">Tipo de contrato:</div><div class=\"col-xs-6 dreta\">Venta</div></div--> <div class=\"row\"><div class=\"col-xs-6\">Localidad:</div><div class=\"col-xs-6 dreta\">Cala Ratjada</div></div> <div class=\"row\"><div class=\"col-xs-6\">Baños:</div><div class=\"col-xs-6 dreta\">02</div></div> <div class=\"row\"><div class=\"col-xs-6\">Dormitorios:</div><div class=\"col-xs-6 dreta\">03</div></div> <div class=\"row\"><div class=\"col-xs-6\">Construido:</div><div class=\"col-xs-6 dreta\">104m²</div></div> <div class=\"row\"><div class=\"col-xs-6\">Terreno:</div><div class=\"col-xs-6 dreta\">0m²</div></div> <div class=\"row\"><div class=\"col-xs-6\">Eficiencia energética:</div><div class=\"col-xs-6 dreta\">En tramite</div></div> <h3>550.000€</h3> </div> <!--imprimir casa--> <div class=\"col-sm-12\"> <a href=\"/property-view-print.php?param=15746\"><img class=\"ico-print\" src=\"/images/icons/impresora.png\" title=\"Print\" alt=\"Print\"></a> </div> </div> </div> </div> </div> </div> </div> <div class=\"container-fluid \"> <!-- #### MAPA ###--> <div class=\"row\"> <div class=\"col-sm-12\" id=\"mapa\"></div> </div> </div> </section> <footer> <div class=\"container-fluid container-marges\"> <div class=\"row top\"> <div class=\"col-md-4 col-sm-4 left\"> <h3>CALA RATJADA</h3> <p> C/ Juan Sebastián Elcano 15<br> Esquina Castellet, 07590 Cala Ratjada </p> <h3>Teléfono</h3> <a href=\"tel:+34971563999\"><p>Fijo +34 971 563 999 </p></a> <a href=\"tel:+34651889721\">Móvil +34 651 889 721</a> <h3>E-mail</h3> <a href=\"mailto:dosaguisl@gmail.com \">dosaguisl@gmail.com </a> </div> <div class=\"col-sm-5 col-md-5 center\"> <div class=\"col-sm-12 logos-legals\"> <img src=\"/images/icons/atp.png\" alt=\"ATP\"> <img src=\"/images/icons/abai.png\" alt=\"ABAI\"> </div> <div class=\"col-sm-12\"> <a href=\"/es/home\"> <img class=\"logo\" src=\"/images/icons/logo.png\" title=\"Home\" alt=\"dosagui\"> </a> </div> <div> <a href=\"https://www.facebook.com/inmobiliariadosagui\"> <img class=\"ico-facebook\" src=\"/images/icons/logoFacebook.png\" alt=\"dosagui\"> </a> </div> </div> <div class=\"col-md-3 col-sm-3 right\"> <a href=\"/es/home\">Inicio</a> <a href=\"/es/venta\">Venta</a> <!--a href=\"/es/Blog\">Blog</a--> <a href=\"/es/contacto\">Contacto</a> <a href=\"/es/wishilist\">Favoritos</a> </div> </div> <div class=\"row bottom\"> <div class=\"col-sm-5 col-md-5 col-xs-12 center col-md-offset-4 col-sm-offset-4 \"> Todos los derechos reservados (2016) © · <a href=\"/es/aviso-legal/\">Aviso legal</a> </div> <div class=\"col-md-3 col-sm-3 right\"> <a href=\"http://www.in-off.com\"><img class=\"logoInoff\" src=\"/images/icons/inoff.png\" alt=\"inoff\"></a> </div> </div> </div> </footer> </body></html>";

            //Console.WriteLine(new landerist_library.Parse.ListingParser.LocalAI.LocalAIRequest().GetResponse(text).Result.GetResponseText());            
            //Console.WriteLine(VertexAIResponse.GetResponseText(VertexAIRequest.GetResponse(text).Result));
            //Console.WriteLine(landerist_library.Parse.ListingParser.OpenAI.OpenAIRequest.GetChatResponse(text).FirstChoice.ToString());

            //var response = new LocalAIRequest().GetResponse(text).Result;


            //landerist_library.Parse.ListingParser.LocalAI.LocalAIRequest.PrintOutputSchema();
            //var json_squema_string = StructuredOutputSchema.Serialize();
            //Console.WriteLine(json_squema_string);
            //Console.WriteLine(ParseListingSystem.GetSystemPrompt());



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


            //string address = "Fuengirola, Torreblanca del Sol, Málaga, España, 29640";
            //var latLNg = new GoogleMapsApi().GetLatLng(address, CountryCode.ES);
            //var cadastralReference = new AddressToCadastralReference().GetCadastralReference(latLNg.Value.latLng.Item1, latLNg.Value.latLng.Item2, address);
            //Console.WriteLine(cadastralReference);


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
            //landerist_library.Database.RedirectUrl.Insert("test1", "test2");

            #endregion

            #region Tasks

            //ServiceTasks.DailyTask();
            //new ServiceTasks().UpdateAndScrape();
            //landerist_library.Landerist_com.FilesUpdater.Update();
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

