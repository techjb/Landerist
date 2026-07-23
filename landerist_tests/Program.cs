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
using landerist_library.Scrape;
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
            PagePersistenceService pagePersistence = new(new PageRepository(new DataBase()));
            WebsitePersistenceService websitePersistence = new(new WebsiteRepository(new DataBase()));

            LanderistApplication.Configure(new LanderistApplicationServices(
                pagePersistence,
                websitePersistence));

            return new Scraper(
                pagePersistence,
                logger,
                listingLifecycle,
                pageScraping,
                pageBatchSelector,
                batchScraping);
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
                    Console.WriteLine("¡Ctrl + D detectado!");
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

