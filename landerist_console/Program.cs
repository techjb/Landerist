using landerist_library.Configuration;
using landerist_library.Application;
using landerist_library.Application.Listings;
using landerist_library.Application.Persistence;
using landerist_library.Application.Scraping;
using landerist_library.Database;
using landerist_library.Infrastructure.Logging;
using landerist_library.Infrastructure.Listings;
using landerist_library.Infrastructure.Sql;
using landerist_library.Infrastructure.Scraping;
using landerist_library.Logs;
using landerist_library.Tasks;

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
            ConfigureApplicationServices();
            _serviceTasks = new TasksService();
            Console.Title = "Landerist Console " + Config.VERSION;
            Start();
            Run();
        }

        private static void ConfigureApplicationServices()
        {
            LegacyApplicationLogger logger = new();
            ListingLifecycleService listingLifecycle = new(
                new LegacyListingStore(),
                new LegacyNotListingCacheService(),
                new LegacyPageLinkService(),
                new LegacyListingEnricher(),
                new LegacyListingUnpublishPolicy(),
                logger);
            PageScrapePipelineServices pageScraping = new(
                new PageAcquisitionService(
                    new LegacyPageDownloader(),
                    new LegacyConditionalPageHeaderService(),
                    new LegacyScrapeMetrics(),
                    conditionalHeadersEnabled: !Config.IsConfigurationLocal()),
                new LegacyPageContentClassifier(),
                new LegacyPageIndexingService(),
                new LegacyPageSchedulingService());
            PageBatchSelector pageBatchSelector = new(
                new LegacyPageSelectionRepository(),
                new PageSelectionOptions(
                    Config.MAX_PAGES_PER_SCRAPE,
                    Config.MAX_PAGES_PER_HOST_PER_SCRAPE,
                    Config.MIN_PAGES_PER_SCRAPE,
                    enforceMinimumPages: Config.IsConfigurationProduction()));
            ScrapeBatchServices batchScraping = new(
                new LegacyWebsiteThrottleService(),
                new LegacyScrapeResourceManager(),
                new LegacyScrapeBatchMetrics(),
                new LegacyScrapePageSource(),
                new ScraperExecutionOptions(
                    Config.IsConfigurationProduction(),
                    Config.IsConfigurationLocal(),
                    Config.MAX_DEGREE_OF_PARALLELISM_SCRAPER));
            LanderistApplication.Configure(new LanderistApplicationServices(
                new PagePersistenceService(new PageRepository(new DataBase())),
                new WebsitePersistenceService(new WebsiteRepository(new DataBase())),
                logger,
                listingLifecycle,
                pageScraping,
                pageBatchSelector,
                batchScraping));
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