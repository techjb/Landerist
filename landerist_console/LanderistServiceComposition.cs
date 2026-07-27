using landerist_library.Parse.ListingParser.StructuredOutputs;
using landerist_library.Parse.ListingParser;
using landerist_library.Infrastructure.Browser;
using landerist_library.Downloaders.Puppeteer;
using landerist_library.Downloaders.Multiple;
using landerist_library.Parse.Location.Providers.Goolzoom;
using landerist_library.Websites;
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
using landerist_library.Infrastructure.Http;
using landerist_library.Infrastructure.Indexing;
using landerist_library.Infrastructure.PageServices;
using landerist_library.Infrastructure.Listings;
using landerist_library.Infrastructure.Sql;
using landerist_library.Infrastructure.Scraping;
using landerist_library.Infrastructure.Tasks;
using landerist_library.Infrastructure.WebsiteServices;
using landerist_library.Logs;
using landerist_library.Application.Statistics;

namespace landerist_console;

internal static class LanderistServiceComposition
{
    public static TasksService CreateTasksService()
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
        WebsiteNetworkService websiteNetwork = new(
            httpClients,
            TimeProvider.System);
        PagePersistenceService pagePersistence = new(new PageRepository(databaseFactory.Create()), logger);
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
        WebsiteRobotsPolicy robotsPolicy = new();
        WebsiteAccessServices websiteAccess = new(robotsPolicy, httpClients);
        ListingParsingServices parsingServices = new(
            ListingMaterializationRules.Default,
            websiteAccess,
            TimeProvider.System);
        WebsiteSitemapService websiteSitemaps = new(
            Config.INDEXER_ENABLED,
            robotsPolicy,
            TimeProvider.System,
            new LegacyWebsiteSitemapIndexerFactory(
                robotsPolicy,
                httpClients,
                pagePersistence,
                websiteMetrics),
            logger);
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
            robotsPolicy,
            Config.MAX_PAGES_PER_WEBSITE);
        ListingLifecycleService listingLifecycle = new(
            listingStore,
            notListingCache,
            pageLinks,
            new SqlListingEnricher(databaseFactory.Create(), goolzoom),
            new LegacyListingUnpublishPolicy(listingQueries),
            logger,
            new HtmlPageContentInspector());
        PageScrapePipelineServices pageScraping = new(
            new PageAcquisitionService(
                new PooledPageDownloader(downloaders),
                new HttpConditionalPageHeaderService(httpClients),
                new SqlScrapeMetrics(databaseFactory.Create()),
                conditionalHeadersEnabled: !Config.IsConfigurationLocal()),
            new PageContentClassifier(
                Config.IsConfigurationProduction(),
                notListingCache,
                new SqlPageClassificationMetrics(databaseFactory.Create()),
                new LegacyListingPageParser(hostStatistics, listingParser),
                new LegacyPageTokenLimitPolicy(new Tokenizer(TokenizerOptions.ForProvider(Config.LLM_PROVIDER)))),
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
            new SqlWebsiteThrottleService(databaseFactory.Create(), robotsPolicy),
            new ScrapeBrowserManager(downloaders, chrome),
            new SqlPageLockManager(databaseFactory.Create(), Config.MACHINE_NAME),
            new SqlScrapeBatchMetrics(databaseFactory.Create()),
            new SqlScrapePageSource(databaseFactory.Create(), listingStore),
            robotsPolicy,
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

        Tokenizer batchTokenizer = new(
            TokenizerOptions.ForProvider(Config.LLM_PROVIDER));
        int batchMaxInputTokens =
            TokenizerOptions.ForProvider(Config.LLM_PROVIDER).MaxContextWindow -
            batchTokenizer.CountSystemTokens();
        int batchMaxPages = Config.IsConfigurationLocal()
            ? Config.MAX_PAGES_PER_BATCH_LOCAL
            : Config.LLM_PROVIDER switch
            {
                LLMProvider.OpenAI => Config.MAX_PAGES_PER_BATCH_OPEN_AI,
                LLMProvider.VertexAI => Config.MAX_PAGES_PER_BATCH_VERTEX_AI,
                _ => throw new InvalidOperationException(
                    $"Batch upload is not supported for {Config.LLM_PROVIDER}.")
            };
        long batchMaxFileSize = Config.LLM_PROVIDER switch
        {
            LLMProvider.OpenAI => Config.MAX_BATCH_FILE_SIZE_OPEN_AI * 1024L * 1024L,
            LLMProvider.VertexAI => Config.MAX_BATCH_FILE_SIZE_VERTEX_AI * 1024L * 1024L,
            _ => throw new InvalidOperationException(
                $"Batch upload is not supported for {Config.LLM_PROVIDER}.")
        };
        BatchUploadOptions batchUploadOptions = new(
            Config.LLM_PROVIDER,
            batchMaxPages,
            Config.MIN_PAGES_PER_BATCH,
            batchMaxInputTokens,
            updateWaitingResponse: !Config.IsConfigurationLocal(),
            statusUpdateParallelism:
                Config.PARALLELOPTIONS1INLOCAL.MaxDegreeOfParallelism);
        ListingBatchUploadProviderCatalog batchUploadProviders = new(
        [
            new OpenAIBatchUploadProvider(),
            new VertexAIBatchUploadProvider()
        ]);
        IListingBatchUploadProvider batchUploadProvider =
            batchUploadProviders.GetRequired(Config.LLM_PROVIDER);
        IBatchInputWriter batchInputWriter = new JsonlBatchInputWriter(
            new BatchInputWriterOptions(
                Config.LLM_PROVIDER,
                Config.BATCH_DIRECTORY ?? throw new InvalidOperationException(
                    "Batch directory is not configured."),
                batchMaxFileSize,
                Config.MIN_PAGES_PER_BATCH),
            batchUploadProvider,
            TimeProvider.System,
            logger);
        return new TasksService(
            new TasksServiceOptions(executionMode),
            new SystemRecurringTaskScheduler(),
            logger,
            new ScrapeTaskJob(scraper, batchScraping.Browser),
            new LocalAiTaskJob(() => new TaskLocalAIParsing(
                parsedClassification,
                globalStatistics,
                hostStatistics,
                waitingStatus,
                pageCatalog,
                pagePersistence,
                listingParser,
                new LocalAiParsingTaskOptions(
                    modelMaxTokens: Config.LOCAL_AI_MAX_MODEL_LEN,
                    runSequentially: Config.IsConfigurationLocal(),
                    updateWaitingStatusOnStart: Config.IsConfigurationProduction()),
                new Tokenizer(TokenizerOptions.ForProvider(LLMProvider.LocalAI)))),
            new TenMinuteTaskJob(
                new TaskBatchDownload(parsedClassification, batches, globalStatistics, pageCatalog, pagePersistence, new OpenAIBatchDownload(), new VertexAIBatchDownload(), listingParser),
                new TaskBatchUpload(
                    batches,
                    waitingStatus,
                    pagePersistence,
                    batchUploadOptions,
                    batchUploadProviders,
                    batchInputWriter)),
            new HourlyTaskJob(
                new WebsiteRefreshService(websiteCatalog, websitePersistence, websiteNetwork, websiteSitemaps),
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
                    websiteQueries,
                    new SqlListingAdministrationService(
                        new ListingRepository(databaseFactory.Create()),
                        new ListingQueryRepository(databaseFactory.Create()),
                        new ListingStatisticsRepository(databaseFactory.Create()),
                        new MediaRepository(databaseFactory.Create()),
                        new SourceRepository(databaseFactory.Create())))),
            TimeProvider.System);
    }
}
