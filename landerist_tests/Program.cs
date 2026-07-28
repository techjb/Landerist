using landerist_library.Parse.ListingParser.StructuredOutputs;
using landerist_library.Parse.ListingParser;
using landerist_library.Infrastructure.Browser;
using landerist_library.Downloaders.Puppeteer;
using landerist_library.Downloaders.Multiple;
using landerist_library.Parse.Location.Providers.Goolzoom;
using landerist_library.Websites;
using landerist_library.Infrastructure.Statistics;
using landerist_library.Infrastructure.Parsing;
using landerist_library.Configuration;
using landerist_library.Application;
using landerist_library.Application.Listings;
using landerist_library.Application.Persistence;
using landerist_library.Application.Scraping;
using landerist_library.Application.Websites;
using landerist_library.Database;
using landerist_library.Infrastructure.Logging;
using landerist_library.Infrastructure.Downloaders;
using landerist_library.Infrastructure.Http;
using landerist_library.Infrastructure.Listings;
using landerist_library.Infrastructure.PageServices;
using landerist_library.Infrastructure.Sql;
using landerist_library.Infrastructure.Scraping;
using landerist_library.Infrastructure.WebsiteServices;
using landerist_library.Logs;
using landerist_library.Application.Statistics;
using System.Runtime.InteropServices;


namespace landerist_tests
{
    partial class Program
    {
        private static DateTime DateStart;
        private static int IsEnding;
        private static readonly ManualResetEventSlim ExitSignal = new(false);

        private delegate bool ConsoleEventDelegate(int eventType);
        private static readonly ConsoleEventDelegate Handler = new(ConsoleEventHandler);
        public delegate void KeyPressedHandler(ConsoleKeyInfo key);
        public static event KeyPressedHandler? OnKeyPressed;
        private static Scraper? Scraper;

        static void Main()
        {
            Console.Title = "Landerist Tests";
            Config.SetToTest();
            Scraper = ConfigureApplicationServices();
            Start();
            Run();
            //ExitSignal.Wait();
            End();
        }

        private static Scraper ConfigureApplicationServices()
        {
            SqlDatabaseFactory databaseFactory = new(
                new SqlDatabaseOptions(
                    Config.DATASOURCE
                        ?? throw new InvalidOperationException("Database data source is not configured."),
                    Config.DATABASE_USER,
                    Config.DATABASE_PW,
                    Config.DATABASE_NAME,
                    Config.DATABASE_ENCRYPT,
                    Config.DATABASE_TRUST_SERVER_CERTIFICATE));
            LegacyDatabase.Configure(databaseFactory);
            LanderistSettings settings = LanderistSettings.Current;
            HttpClientTransportFactory httpClients = new(
                new HttpTransportOptions(
                    settings.GetString("PROXY_HOST"),
                    settings.GetInt32("PROXY_PORT"),
                    settings.GetBoolean("PROXY_RANDOMIZE_STICKY_PORTS"),
                    settings.GetInt32("PROXY_STICKY_PORT_MIN"),
                    settings.GetInt32("PROXY_STICKY_PORT_MAX"),
                    settings.GetString("PROXY_USERNAME"),
                    settings.GetString("PROXY_PASSWORD")));
            PuppeteerBrowserOptions browserOptions = new(
                Config.HEADLESS_BROWSER,
                Config.IsConfigurationLocal(),
                Config.HTTPCLIENT_SECONDS_TIMEOUT * 1000,
                settings.GetString("PROXY_HOST"),
                settings.GetInt32("PROXY_PORT"),
                settings.GetBoolean("PROXY_RANDOMIZE_STICKY_PORTS"),
                settings.GetInt32("PROXY_STICKY_PORT_MIN"),
                settings.GetInt32("PROXY_STICKY_PORT_MAX"),
                settings.GetString("PROXY_USERNAME"),
                settings.GetString("PROXY_PASSWORD"));
            DownloadersPool downloaders = new(
                Config.MAX_DEGREE_OF_PARALLELISM_SCRAPER,
                new PuppeteerDownloaderFactory(browserOptions));
            LegacyApplicationLogger logger = new();
            LegacyDownloadersPoolAdapter downloaderPool = new(downloaders);
            ChromeMaintenanceService chrome = new(
                new ChromeMaintenanceOptions(
                    ProcessCleanupEnabled: Config.IsConfigurationProduction(),
                    UseTaskKillFallback: Config.IsPrincipalMachine()),
                new SystemChromeProcessController(logger),
                new PuppeteerChromeBrowserInstaller());
            GoolzoomApi goolzoom = new(
                httpClients,
                new GoolzoomOptions(
                    settings.GetString("GOOLZOOM_API"),
                    TimeSpan.FromSeconds(Config.HTTPCLIENT_SECONDS_TIMEOUT),
                    MaxRetryAttempts: 3));
            PagePersistenceService pagePersistence = new(new PageRepository(databaseFactory.Create()), logger);
            WebsitePersistenceService websitePersistence = new(new WebsiteRepository(databaseFactory.Create()));
            SqlListingStore listingStore = new(
                databaseFactory.Create(),
                new GlobalStatisticsRepository(databaseFactory.Create()),
                logger);
            SqlListingQueryService listingQueries = new(
                new ListingQueryRepository(databaseFactory.Create()),
                new MediaRepository(databaseFactory.Create()),
                new SourceRepository(databaseFactory.Create()));
            SqlListingMaintenanceService listingMaintenance = new(
                new ListingRepository(databaseFactory.Create()),
                new MediaRepository(databaseFactory.Create()),
                new SourceRepository(databaseFactory.Create()));
            SqlNotListingCacheService notListingCache = new(
                databaseFactory.Create(),
                Config.NOT_LISTING_CACHE_ENABLED);
            HostStatistics hostStatistics = new(
                new HostStatisticsRepository(databaseFactory.Create()),
                new SqlWebsiteCatalog(new WebsiteQueryRepository(databaseFactory.Create())),
                persistenceEnabled: !Config.IsConfigurationLocal());
            WebsiteRobotsPolicy robotsPolicy = new();
            WebsiteAccessServices websiteAccess = new(robotsPolicy, httpClients);
            ListingParsingServices parsingServices = new(
                ListingMaterializationRules.Default,
                websiteAccess,
                TimeProvider.System);
            ListingParserClientCatalog listingParserClients = new(
            [
                new OpenAIListingParserClient(),
                new VertexAIListingParserClient(),
                new LocalAIListingParserClient()
            ]);
            ParseListing listingParser = new(
                new ListingParserOrchestrationOptions(
                    Config.BATCH_ENABLED,
                    Config.LLM_PROVIDER),
                listingParserClients,
                parsingServices);
            SqlPageLinkService pageLinks = new(
                pagePersistence,
                new WebsitePageMetricsRepository(databaseFactory.Create()),
                robotsPolicy,
                Config.MAX_PAGES_PER_WEBSITE);
            ListingLifecycleService listingLifecycle = new(
                listingStore,
                notListingCache,
                pageLinks,
                new SqlListingEnricher(
                    databaseFactory.Create(),
                    new LegacyListingLocationEnricher(
                        databaseFactory.Create(),
                        goolzoom)),
                new LegacyListingUnpublishPolicy(listingQueries),
                logger,
                new HtmlPageContentInspector());
            PageScrapePipelineServices pageScraping = new(
                new PageAcquisitionService(
                    new PooledPageDownloader(downloaderPool),
                    new HttpConditionalPageHeaderService(httpClients),
                    new SqlScrapeMetrics(databaseFactory.Create()),
                    conditionalHeadersEnabled: !Config.IsConfigurationLocal()),
                new PageContentClassifier(
                    Config.IsConfigurationProduction(),
                    notListingCache,
                    new SqlPageClassificationMetrics(databaseFactory.Create()),
                    new LegacyListingPageParser(hostStatistics, listingParser),
                    new LegacyPageTokenLimitPolicy(new Tokenizer(TokenizerOptions.ForProvider(Config.LLM_PROVIDER))),
                new HtmlPageContentInspector(),
                new PageListingInputPreparer()),
                new PageIndexingService(
                Config.INDEXER_ENABLED,
                pageLinks,
                new HtmlPageLinkExtractor()),
                new SqlPageSchedulingService(listingStore),
                Config.INDEXER_ENABLED);
            PageBatchSelector pageBatchSelector = new(
                new SqlPageSelectionRepository(
                    databaseFactory.Create(),
                    Config.MACHINE_NAME,
                    new PageQueryOptions(
                        Config.IsConfigurationLocal() ? null : Config.MACHINE_NAME,
                        Config.MAX_PAGES_PER_HOST_PER_SCRAPE)),
                new PageSelectionOptions(
                    Config.MAX_PAGES_PER_SCRAPE,
                    Config.MAX_PAGES_PER_HOST_PER_SCRAPE,
                    Config.MIN_PAGES_PER_SCRAPE,
                    enforceMinimumPages: Config.IsConfigurationProduction()));
            ScrapeBatchServices batchScraping = new(
                new SqlWebsiteThrottleService(databaseFactory.Create(), robotsPolicy),
                new ScrapeBrowserManager(downloaderPool, chrome, logger),
                new SqlPageLockManager(databaseFactory.Create(), Config.MACHINE_NAME),
                new SqlScrapeBatchMetrics(databaseFactory.Create()),
                new SqlScrapePageSource(databaseFactory.Create(), listingStore),
                robotsPolicy,
                new ScraperExecutionOptions(
                    Config.IsConfigurationProduction(),
                    Config.IsConfigurationLocal(),
                    Config.MAX_DEGREE_OF_PARALLELISM_SCRAPER));

            return new Scraper(
                pagePersistence,
                logger,
                listingLifecycle,
                pageScraping,
                pageBatchSelector,
                batchScraping,
                new ConsoleScrapeProgressReporter());
        }

        private static void Start()
        {
            //RegisterExitEvents();
            //SetCtrlDListener();

            DateStart = DateTime.Now;            
            Log.DeleteCurentMachineLogs();
            Log.Console("Started. Machine: " + Config.MACHINE_NAME + " Version: " + Config.VERSION);
        }

        private static void RegisterExitEvents()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                SetConsoleCtrlHandler(Handler, true);
            }

            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                ExitSignal.Set();
                End();
            };

            AppDomain.CurrentDomain.ProcessExit += (_, _) => End();
        }

        static void SetCtrlDListener()
        {
            OnKeyPressed += keyInfo =>
            {
                if ((keyInfo.Modifiers & ConsoleModifiers.Control) != 0 &&
                    keyInfo.Key == ConsoleKey.D)
                {
                    Console.WriteLine("ÃƒÆ’Ã†â€™Ãƒâ€ Ã¢â‚¬â„¢ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬Ãƒâ€¦Ã‚Â¡ÃƒÆ’Ã†â€™ÃƒÂ¢Ã¢â€šÂ¬Ã…Â¡ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â¡Ctrl + D detectado!");
                    ExitSignal.Set();
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
                {
                    ExitSignal.Set();
                    return;
                }
            }
        }

        private static bool ConsoleEventHandler(int eventType)
        {
            Console.WriteLine(eventType);
            if (eventType is 0 or 2 or 5 or 6)
            {
                ExitSignal.Set();
                End();
            }
            return false;
        }

        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);

        private static void End()
        {
            if (Interlocked.Exchange(ref IsEnding, 1) == 1)
            {
                return;
            }

            //ServiceTasks.Stop();
            Scraper?.Stop();
            var duration = (DateTime.Now - DateStart).ToString(@"dd\:hh\:mm\:ss\.fff");
            Log.Console("Stopped. Version: " + Config.VERSION + " Duration: " + duration);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Console.Beep(500, 500);
            }
        }

        private static void Run()
        {
            UrlsTests.Run();
            WebsitesTests.Run();
            PagesTests.Run();
            ScraperTests.Run();
            DownloadersTests.Run();
            ListingParserTests.Run();
            LocalAITests.Run();
            LocationParserTests.Run();
            IndexTests.Run();
            BackupTests.Run();
            StatisticsTests.Run();
            ListingsTests.Run();
            InsertTests.Run();
            DistributionTests.Run();
            DataBaseTests.Run();
            TasksTests.Run();
            ToolsTests.Run();
            LocalIsListingTests.Run();
        }
    }
}

