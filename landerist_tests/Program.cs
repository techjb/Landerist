using landerist_library.Configuration;
using landerist_library.Application;
using landerist_library.Application.Listings;
using landerist_library.Application.Persistence;
using landerist_library.Application.Scraping;
using landerist_library.Application.Websites;
using landerist_library.Database;
using landerist_library.Infrastructure.Logging;
using landerist_library.Infrastructure.Listings;
using landerist_library.Infrastructure.PageServices;
using landerist_library.Infrastructure.Sql;
using landerist_library.Infrastructure.Scraping;
using landerist_library.Infrastructure.WebsiteServices;
using landerist_library.Logs;
using landerist_library.Statistics;
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
            LegacyApplicationLogger logger = new();
            PagePersistenceService pagePersistence = new(new PageRepository(databaseFactory.Create()));
            WebsitePersistenceService websitePersistence = new(new WebsiteRepository(databaseFactory.Create()));
            SqlListingStore listingStore = new(databaseFactory.Create(), logger);
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
                new SqlWebsiteCatalog(new WebsiteQueryRepository(databaseFactory.Create())));
            SqlPageLinkService pageLinks = new(
                pagePersistence,
                new WebsitePageMetricsRepository(databaseFactory.Create()),
                Config.MAX_PAGES_PER_WEBSITE);
            ListingLifecycleService listingLifecycle = new(
                listingStore,
                notListingCache,
                pageLinks,
                new SqlListingEnricher(databaseFactory.Create()),
                new LegacyListingUnpublishPolicy(),
                logger);
            PageScrapePipelineServices pageScraping = new(
                new PageAcquisitionService(
                    new LegacyPageDownloader(),
                    new LegacyConditionalPageHeaderService(),
                    new SqlScrapeMetrics(databaseFactory.Create()),
                    conditionalHeadersEnabled: !Config.IsConfigurationLocal()),
                new PageContentClassifier(
                    Config.IsConfigurationProduction(),
                    notListingCache,
                    new SqlPageClassificationMetrics(databaseFactory.Create()),
                    hostStatistics),
                new PageIndexingService(Config.INDEXER_ENABLED, pageLinks),
                new SqlPageSchedulingService(listingStore),
                Config.INDEXER_ENABLED);
            PageBatchSelector pageBatchSelector = new(
                new SqlPageSelectionRepository(databaseFactory.Create(), Config.MACHINE_NAME),
                new PageSelectionOptions(
                    Config.MAX_PAGES_PER_SCRAPE,
                    Config.MAX_PAGES_PER_HOST_PER_SCRAPE,
                    Config.MIN_PAGES_PER_SCRAPE,
                    enforceMinimumPages: Config.IsConfigurationProduction()));
            ScrapeBatchServices batchScraping = new(
                new SqlWebsiteThrottleService(databaseFactory.Create()),
                new SqlScrapeResourceManager(databaseFactory.Create(), Config.MACHINE_NAME),
                new SqlScrapeBatchMetrics(databaseFactory.Create()),
                new SqlScrapePageSource(databaseFactory.Create(), listingStore),
                new ScraperExecutionOptions(
                    Config.IsConfigurationProduction(),
                    Config.IsConfigurationLocal(),
                    Config.MAX_DEGREE_OF_PARALLELISM_SCRAPER));

            SqlPageCatalog pageCatalog = new(
                new PageQueryRepository(databaseFactory.Create()));
            WebsiteDeletionService websiteDeletion = new(
                pageCatalog,
                new OrelsListingDeletionService(listingMaintenance),
                new SqlPageDeletionService(new PageMaintenanceRepository(databaseFactory.Create())),
                websitePersistence);
            LanderistApplication.Configure(new LanderistApplicationServices(
                pagePersistence,
                websitePersistence,
                websiteDeletion,
                listingQueries: listingQueries,
                listingMaintenance: listingMaintenance));

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
                    Console.WriteLine("Ã‚Â¡Ctrl + D detectado!");
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
            LanderistComTests.Run();
            DataBaseTests.Run();
            TasksTests.Run();
            ToolsTests.Run();
            LocalIsListingTests.Run();
        }
    }
}

