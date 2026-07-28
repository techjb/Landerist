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
using landerist_library.Infrastructure.Administration;
using landerist_library.Infrastructure.Backup;
using landerist_library.Infrastructure.Distribution;
using landerist_library.Infrastructure.Downloaders;
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
using landerist_library.Infrastructure.Runtime;
using Microsoft.Extensions.DependencyInjection;

namespace landerist_console;

internal static class LanderistServiceComposition
{
    public static TasksService CreateTasksService(
        LanderistRuntimeOptions runtimeOptions,
        IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(runtimeOptions);
        ArgumentNullException.ThrowIfNull(services);
        runtimeOptions.Validate();
        LanderistDatabaseAdapterFactory databaseAdapters =
            services.GetRequiredService<LanderistDatabaseAdapterFactory>();
        HttpClientTransportFactory httpClients =
            services.GetRequiredService<HttpClientTransportFactory>();
        LegacyApplicationLogger logger =
            services.GetRequiredService<LegacyApplicationLogger>();
        GoolzoomApi goolzoom = services.GetRequiredService<GoolzoomApi>();
        WebsiteNetworkService websiteNetwork =
            services.GetRequiredService<WebsiteNetworkService>();
        PagePersistenceService pagePersistence = new(services.GetRequiredService<PageRepository>(), logger);
        WebsitePersistenceService websitePersistence = new(services.GetRequiredService<WebsiteRepository>());
        SqlListingStore listingStore = databaseAdapters.CreateListingStore(
            services.GetRequiredService<GlobalStatisticsRepository>(),
            logger);
        SqlListingQueryService listingQueries = new(
            services.GetRequiredService<ListingQueryRepository>(),
            services.GetRequiredService<MediaRepository>(),
            services.GetRequiredService<SourceRepository>());
        SqlListingMaintenanceService listingMaintenance = new(
            services.GetRequiredService<ListingRepository>(),
            services.GetRequiredService<MediaRepository>(),
            services.GetRequiredService<SourceRepository>());
        SqlNotListingCacheService notListingCache =
            databaseAdapters.CreateNotListingCache(
                Config.NOT_LISTING_CACHE_ENABLED);
        PageQueryOptions pageQueryOptions = services.GetRequiredService<PageQueryOptions>();
        SqlPageCatalog pageCatalog = new(
            services.GetRequiredService<PageQueryRepository>());
        SqlPageQueryService pageQueries = new(
            services.GetRequiredService<PageQueryRepository>());
        SqlPageMaintenanceService pageMaintenance = new(
            services.GetRequiredService<PageMaintenanceRepository>());
        WebsiteDeletionService websiteDeletion = new(
            pageCatalog,
            new OrelsListingDeletionService(listingMaintenance),
            new SqlPageDeletionService(services.GetRequiredService<PageMaintenanceRepository>()),
            websitePersistence);
        SqlPageWaitingStatusService waitingStatus = new(
            services.GetRequiredService<PageMaintenanceRepository>());
        PageStatisticsRepository pageStatistics = services.GetRequiredService<PageStatisticsRepository>();
        WebsiteQueryRepository websiteQueries = services.GetRequiredService<WebsiteQueryRepository>();
        SqlWebsiteCatalog websiteCatalog = new(websiteQueries);
        SqlWebsiteMaintenanceService websiteMaintenance = new(websiteQueries);
        WebsiteMetricsService websiteMetrics = new(
            services.GetRequiredService<WebsitePageMetricsRepository>(),
            services.GetRequiredService<ListingStatisticsRepository>(),
            Config.MAX_PAGES_PER_WEBSITE);
        WebsiteRobotsPolicy robotsPolicy =
            services.GetRequiredService<WebsiteRobotsPolicy>();
        WebsiteAccessServices websiteAccess =
            services.GetRequiredService<WebsiteAccessServices>();
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
            services.GetRequiredService<GlobalStatisticsRepository>(),
            persistenceEnabled: !Config.IsConfigurationLocal());
        HostStatistics hostStatistics = new(
            services.GetRequiredService<HostStatisticsRepository>(),
            websiteCatalog,
            persistenceEnabled: !Config.IsConfigurationLocal());
        SqlPageLinkService pageLinks = new(
            pagePersistence,
            services.GetRequiredService<WebsitePageMetricsRepository>(),
            robotsPolicy,
            Config.MAX_PAGES_PER_WEBSITE);
        ListingLifecycleService listingLifecycle = new(
            listingStore,
            notListingCache,
            pageLinks,
            databaseAdapters.CreateListingEnricher(goolzoom),
            new LegacyListingUnpublishPolicy(listingQueries),
            logger,
            new HtmlPageContentInspector());
        PageScrapePipelineServices pageScraping = new(
            new PageAcquisitionService(
                services.GetRequiredService<PooledPageDownloader>(),
                services.GetRequiredService<HttpConditionalPageHeaderService>(),
                databaseAdapters.CreateScrapeMetrics(),
                conditionalHeadersEnabled: !Config.IsConfigurationLocal()),
            new PageContentClassifier(
                Config.IsConfigurationProduction(),
                notListingCache,
                databaseAdapters.CreatePageClassificationMetrics(),
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
            databaseAdapters.CreatePageSelectionRepository(
                Config.MACHINE_NAME,
                pageQueryOptions),
            new PageSelectionOptions(
                Config.MAX_PAGES_PER_SCRAPE,
                Config.MAX_PAGES_PER_HOST_PER_SCRAPE,
                Config.MIN_PAGES_PER_SCRAPE,
                enforceMinimumPages: Config.IsConfigurationProduction()));
        ScrapeBatchServices batchScraping = new(
            databaseAdapters.CreateWebsiteThrottle(robotsPolicy),
            services.GetRequiredService<ScrapeBrowserManager>(),
            databaseAdapters.CreatePageLockManager(Config.MACHINE_NAME),
            databaseAdapters.CreateScrapeBatchMetrics(),
            databaseAdapters.CreateScrapePageSource(listingStore),
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
        TasksExecutionMode executionMode = runtimeOptions.Role switch
        {
            LanderistExecutionRole.LocalAi => TasksExecutionMode.LocalAi,
            LanderistExecutionRole.Principal => TasksExecutionMode.Principal,
            LanderistExecutionRole.Scraper => TasksExecutionMode.Scraper,
            _ => throw new ArgumentOutOfRangeException(
                nameof(runtimeOptions),
                runtimeOptions.Role,
                "Unknown execution role.")
        };

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
        BatchProvider batchProvider = Config.LLM_PROVIDER switch
        {
            LLMProvider.OpenAI => BatchProvider.OpenAI,
            LLMProvider.VertexAI => BatchProvider.VertexAI,
            _ => throw new InvalidOperationException(
                $"Batch upload is not supported for {Config.LLM_PROVIDER}.")
        };
        BatchUploadOptions batchUploadOptions = new(
            batchProvider,
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
            batchUploadProviders.GetRequired(batchProvider);
        IBatchInputWriter batchInputWriter = new JsonlBatchInputWriter(
            new BatchInputWriterOptions(
                batchProvider,
                Config.BATCH_DIRECTORY ?? throw new InvalidOperationException(
                    "Batch directory is not configured."),
                batchMaxFileSize,
                Config.MIN_PAGES_PER_BATCH),
            batchUploadProvider,
            new PageListingInputPreparer(),
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
                waitingStatus,
                pageCatalog,
                pagePersistence,
                new LegacyLocalAiListingParser(listingParser, hostStatistics),
                new PageListingInputPreparer(),
                new LocalAiParsingTaskOptions(
                    modelMaxTokens: Config.LOCAL_AI_MAX_MODEL_LEN,
                    runSequentially: Config.IsConfigurationLocal(),
                    updateWaitingStatusOnStart: Config.IsConfigurationProduction()),
                new LegacyLocalAiTokenBudget(
                    new Tokenizer(TokenizerOptions.ForProvider(LLMProvider.LocalAI))),
                logger)),
            new TenMinuteTaskJob(
                new TaskBatchDownload(
                    parsedClassification,
                    databaseAdapters.CreateBatchStore(),
                    globalStatistics,
                    pageCatalog,
                    pagePersistence,
                    new BatchDownloadProviderCatalog(
                    [
                        new BatchDownloadProvider(BatchProvider.OpenAI, new OpenAIBatchDownload()),
                        new BatchDownloadProvider(BatchProvider.VertexAI, new VertexAIBatchDownload())
                    ]),
                    new LegacyBatchListingResponseParser(listingParser),
                    new BatchDownloadOptions(
                        Config.PARALLELOPTIONS1INLOCAL.MaxDegreeOfParallelism),
                    logger),
                new TaskBatchUpload(
                    databaseAdapters.CreateBatchRegistrationStore(),
                    waitingStatus,
                    pagePersistence,
                    batchUploadOptions,
                    batchUploadProviders,
                    batchInputWriter,
                    logger)),
            new HourlyTaskJob(
                new WebsiteRefreshService(websiteCatalog, websitePersistence, websiteNetwork, websiteSitemaps),
                new TaskBatchCleaner(
                    databaseAdapters.CreateBatchStore(),
                    new BatchCleanupOptions(Config.BATCH_DIRECTORY),
                    new LegacyVertexAiBatchArtifactCleaner())),
            new DailyTaskJob(
                databaseAdapters.CreateAddressDataMaintenance(),
                notListingCache,
                databaseAdapters.CreateDatabaseBackupService(),
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
                        services.GetRequiredService<ListingRepository>(),
                        services.GetRequiredService<ListingQueryRepository>(),
                        services.GetRequiredService<ListingStatisticsRepository>(),
                        services.GetRequiredService<MediaRepository>(),
                        services.GetRequiredService<SourceRepository>(),
                        logger)),
                logger),
            TimeProvider.System);
    }
}
