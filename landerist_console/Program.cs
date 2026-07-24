using landerist_library.Infrastructure.Statistics;
using landerist_library.Infrastructure.Parsing.OpenAI;
using landerist_library.Infrastructure.Parsing.VertexAI;
using landerist_library.Infrastructure.Parsing;
using landerist_library.Configuration;
using landerist_library.Application.Listings;
using landerist_library.Application.Persistence;
using landerist_library.Application.Scraping;
using landerist_library.Application.Tasks;
using landerist_library.Application.Websites;
using landerist_library.Database;
using landerist_library.Infrastructure.Backup;
using landerist_library.Infrastructure.Distribution;
using landerist_library.Infrastructure.Logging;
using landerist_library.Infrastructure.PageServices;
using landerist_library.Infrastructure.Listings;
using landerist_library.Infrastructure.Sql;
using landerist_library.Infrastructure.Scraping;
using landerist_library.Infrastructure.Tasks;
using landerist_library.Infrastructure.WebsiteServices;
using landerist_library.Logs;
using landerist_library.Application.Statistics;

namespace landerist_console
{
    partial class Program
    {
        private static DateTime? DateStart = null;
        private static TasksService? _serviceTasks;
        private static TasksService ServiceTasks =>
            _serviceTasks ?? throw new InvalidOperationException("Tasks service has not been initialized.");

        private delegate bool ConsoleEventDelegate(int eventType);
        private static readonly ManualResetEvent ManualResetEvent = new(false);
        public delegate void KeyPressedHandler(ConsoleKeyInfo key);
        public static event KeyPressedHandler? OnKeyPressed;

        static void Main()
        {
            Config.SetToProduction();
            _serviceTasks = ConfigureApplicationServices();
            Console.Title = "Landerist Console " + Config.VERSION;
            Start();
            Run();
        }

        private static TasksService ConfigureApplicationServices()
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
            BatchRepository batches = new(databaseFactory.Create());
            SqlPageCatalog pageCatalog = new(
                new PageQueryRepository(databaseFactory.Create()));
            SqlPageQueryService pageQueries = new(
                new PageQueryRepository(databaseFactory.Create()));
            SqlPageMaintenanceService pageMaintenance = new(
                new PageMaintenanceRepository(databaseFactory.Create()));
            WebsiteDeletionService websiteDeletion = new(
                pageCatalog,
                new OrelsListingDeletionService(listingMaintenance),
                new SqlPageDeletionService(new PageMaintenanceRepository(databaseFactory.Create())),
                websitePersistence);
            SqlPageWaitingStatusService waitingStatus = new(
                new PageMaintenanceRepository(databaseFactory.Create()));
            PageStatisticsRepository pageStatistics = new(databaseFactory.Create());
            WebsiteQueryRepository websiteQueries = new(databaseFactory.Create());
            SqlWebsiteCatalog websiteCatalog = new(websiteQueries);
            SqlWebsiteMaintenanceService websiteMaintenance = new(websiteQueries);
            WebsiteMetricsService websiteMetrics = new(
                new WebsitePageMetricsRepository(databaseFactory.Create()),
                new ListingStatisticsRepository(databaseFactory.Create()),
                Config.MAX_PAGES_PER_WEBSITE);
            GlobalStatistics globalStatistics = new(
                new GlobalStatisticsRepository(databaseFactory.Create()),
                persistenceEnabled: !Config.IsConfigurationLocal());
            HostStatistics hostStatistics = new(
                new HostStatisticsRepository(databaseFactory.Create()),
                websiteCatalog,
                persistenceEnabled: !Config.IsConfigurationLocal());
            SqlPageLinkService pageLinks = new(
                pagePersistence,
                new WebsitePageMetricsRepository(databaseFactory.Create()),
                Config.MAX_PAGES_PER_WEBSITE);
            ListingLifecycleService listingLifecycle = new(
                listingStore,
                notListingCache,
                pageLinks,
                new SqlListingEnricher(databaseFactory.Create()),
                new LegacyListingUnpublishPolicy(listingQueries),
                logger);
            PageScrapePipelineServices pageScraping = new(
                new PageAcquisitionService(
                    new PooledPageDownloader(),
                    new HttpConditionalPageHeaderService(),
                    new SqlScrapeMetrics(databaseFactory.Create()),
                    conditionalHeadersEnabled: !Config.IsConfigurationLocal()),
                new PageContentClassifier(
                    Config.IsConfigurationProduction(),
                    notListingCache,
                    new SqlPageClassificationMetrics(databaseFactory.Create()),
                    new LegacyListingPageParser(hostStatistics),
                    new LegacyPageTokenLimitPolicy()),
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
                new ScrapeBrowserManager(),
                new SqlPageLockManager(databaseFactory.Create(), Config.MACHINE_NAME),
                new SqlScrapeBatchMetrics(databaseFactory.Create()),
                new SqlScrapePageSource(databaseFactory.Create(), listingStore),
                new ScraperExecutionOptions(
                    Config.IsConfigurationProduction(),
                    Config.IsConfigurationLocal(),
                    Config.MAX_DEGREE_OF_PARALLELISM_SCRAPER));
            ParsedPageClassificationService parsedClassification = new(
                pagePersistence,
                listingLifecycle);

            Scraper scraper = new(
                pagePersistence,
                logger,
                listingLifecycle,
                pageScraping,
                pageBatchSelector,
                batchScraping,
                new ConsoleScrapeProgressReporter());
            TasksExecutionMode executionMode =
                Config.IsLocalAIMachine() || Config.IsConfigurationLocal()
                    ? TasksExecutionMode.LocalAi
                    : Config.IsPrincipalMachine()
                        ? TasksExecutionMode.Principal
                        : TasksExecutionMode.Scraper;

            return new TasksService(
                new TasksServiceOptions(executionMode),
                new SystemRecurringTaskScheduler(),
                logger,
                new ScrapeTaskJob(scraper, batchScraping.Browser),
                new LocalAiTaskJob(() => new TaskLocalAIParsing(parsedClassification, globalStatistics, hostStatistics, waitingStatus, pageCatalog, pagePersistence)),
                new TenMinuteTaskJob(
                    new TaskBatchDownload(parsedClassification, batches, globalStatistics, pageCatalog, pagePersistence, new OpenAIBatchDownload(), new VertexAIBatchDownload()),
                    new TaskBatchUpload(batches, waitingStatus, pagePersistence)),
                new HourlyTaskJob(
                    new WebsiteRefreshService(websiteCatalog, websitePersistence, pagePersistence, websiteMetrics),
                    new TaskBatchCleaner(batches)),
                new DailyTaskJob(
                    databaseFactory.Create(),
                    notListingCache,
                    new SqlDatabaseBackupService(databaseFactory.Create()),
                    globalStatistics,
                    hostStatistics,
                    new DistributionPublisher(
                        globalStatistics,
                        hostStatistics,
                        pageStatistics,
                        websiteMetrics,
                        websiteCatalog,
                        websiteQueries)),
                TimeProvider.System);
        }

        private static void Start()
        {
            if (Config.IsPrincipalMachine())
            {
                Console.WriteLine("Ctrl+D to daily tasks.");
                DateStart = DateTime.Now;
                SetCtrlDListener();
            }
            Console.CancelKeyPress += (s, e) =>
            {
                ManualResetEvent.Set();
                End();
            };
            Console.WriteLine("Press Ctrl+C to exit.");
            //DateStart = DateTime.Now; // not working in linux            
            Console.WriteLine("Deleting logs..");
            Log.DeleteCurentMachineLogs();
            Log.WriteInfo("landerist_console", "Started. Machine: " + Config.MACHINE_NAME + " Version: " + Config.VERSION);
        }

        static void SetCtrlDListener()
        {
            if (Config.IsLocalAIMachine())
            {
                return;
            }

            OnKeyPressed += keyInfo =>
            {
                if ((keyInfo.Modifiers & ConsoleModifiers.Control) != 0 &&
                    keyInfo.Key == ConsoleKey.D)
                {
                    ServiceTasks.PerformDailyTask(null);
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

        private static void Run()
        {
            ServiceTasks.Start();
            ManualResetEvent.WaitOne();
        }

        private static void End()
        {
            Log.WriteInfo("landerist_console", "Stopping Version: " + Config.VERSION + " ..");
            ServiceTasks.Stop();

            if (DateStart is null)
            {
                return;
            }
            var duration = (DateTime.Now - (DateTime)DateStart).ToString(@"dd\:hh\:mm\:ss\.fff");
            Log.WriteInfo("landerist_console", "Stopped. Version: " + Config.VERSION + " Duration: " + duration);
        }
    }
}