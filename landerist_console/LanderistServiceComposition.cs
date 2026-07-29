using landerist_library.Infrastructure.Parsing.Tokenization;
using landerist_domain.Parsing.Tokenization;
using landerist_domain.Parsing.Prompt;
using landerist_library.Infrastructure.ListingMedia;
using landerist_library.Infrastructure.Ai.StructuredOutputs;
using landerist_domain.Parsing.Materialization;
using landerist_domain.Parsing.UserInput;
using landerist_library.Infrastructure.Ai.LocalAI;
using landerist_library.Infrastructure.Ai.OpenAI;
using landerist_library.Parsing;
using landerist_library.Application.Parsing;
using landerist_domain.Parsing.StructuredOutputs;
using landerist_library.Infrastructure.Ai.Vertex;
using landerist_library.Infrastructure.Browser;
using landerist_library.Infrastructure.Downloaders.Puppeteer;
using landerist_library.Infrastructure.Downloaders.Multiple;
using landerist_library.Infrastructure.Location.Providers.Goolzoom;
using landerist_library.Websites;
using landerist_library.Infrastructure.Statistics;
using landerist_library.Infrastructure.Ai.OpenAI.Batch;
using landerist_library.Infrastructure.Parsing;
using landerist_library.Infrastructure.Parsing.UserInput;
using landerist_library.Application.Listings;
using landerist_library.Application.Logging;
using landerist_library.Application.Persistence;
using landerist_library.Application.Scraping;
using landerist_library.Application.Tasks;
using landerist_library.Application.Websites;
using landerist_library.Infrastructure.Administration;
using landerist_library.Infrastructure.Ai;
using landerist_library.Infrastructure.Ai.Batch;
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
        landerist_library.Tools.ScrapingBee.Configure(
            runtimeOptions.Integrations.ScrapingBeeApiKey,
            httpClients);
        landerist_library.Export.S3.Configure(new landerist_library.Export.S3Options(
            runtimeOptions.Integrations.AwsAccessKeyId,
            runtimeOptions.Integrations.AwsSecretAccessKey,
            runtimeOptions.Integrations.AwsDownloadsBucket,
            runtimeOptions.Integrations.AwsWebsiteBucket));
        IApplicationLogger logger =
            services.GetRequiredService<IApplicationLogger>();
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
                runtimeOptions.Scraping.NotListingCacheEnabled);
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
            runtimeOptions.Scraping.MaxPagesPerWebsite);
        WebsiteRobotsPolicy robotsPolicy =
            services.GetRequiredService<WebsiteRobotsPolicy>();
        WebsiteAccessServices websiteAccess =
            services.GetRequiredService<WebsiteAccessServices>();
        ListingParsingServices parsingServices = new(
            ListingMaterializationRules.Default,
            websiteAccess,
            TimeProvider.System);
        WebsiteSitemapService websiteSitemaps = new(
            runtimeOptions.Scraping.IndexerEnabled,
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
            new OpenAIListingParserClient(
                new OpenAIListingParserOptions(runtimeOptions.Ai.OpenAiApiKey),
                SystemPrompt.Text,
                StructuredOutputSchema.GetJsonSchemaString(),
                logger),
            new VertexListingParserClient(
                new VertexListingParserOptions(
                    runtimeOptions.Ai.VertexCredential,
                    runtimeOptions.Ai.VertexProjectId,
                    runtimeOptions.Ai.VertexLocation,
                    runtimeOptions.Ai.VertexPublisher,
                    runtimeOptions.Ai.VertexListingModel),
                SystemPrompt.Text,
                VertexAIResponseSchema.ResponseSchema,
                logger),
            new LocalAIListingParserClient(
                    new LocalAIListingParserOptions(
                        runtimeOptions.Ai.LocalAiHost,
                        ResolveHost: runtimeOptions.Ai.ResolveLocalAiHost),
                    SystemPrompt.Text,
                    StructuredOutputSchema.GetJsonSchemaString(),
                    ListingImageUrlPlaceholderCodec.ReplaceImageUrls,
                    logger)
        ]);
        ParseListing listingParser = new(
            new ListingParserOrchestrationOptions(
                runtimeOptions.Batch.Enabled,
                runtimeOptions.Ai.Provider),
            listingParserClients,
            parsingServices,
            new StructuredOutputMaterializationOperations(
                landerist_library.Tools.Strings.Clean,
                landerist_library.Tools.Strings.RemoveSpaces,
                landerist_library.Tools.Validate.Phone,
                landerist_library.Tools.Validate.Email,
                landerist_library.Tools.Validate.CadastralReference,
                (listing, page, websiteAccess, images) =>
                    new MediaParser(
                            page,
                            websiteAccess,
                            new ImageValidationCacheOperations(
                                landerist_library.Database.ValidInvalidImages.IsValid,
                                landerist_library.Database.ValidInvalidImages.IsInvalid,
                                landerist_library.Database.ValidInvalidImages.InsertValid,
                                landerist_library.Database.ValidInvalidImages.InsertInvalid))
                        .AddMediaImages(listing, images),
                (source, uri, exception) => logger.WriteError(source, uri + Environment.NewLine + exception)),
            logger);
        GlobalStatistics globalStatistics = new(
            services.GetRequiredService<GlobalStatisticsRepository>(),
            persistenceEnabled: !runtimeOptions.Execution.IsLocal);
        HostStatistics hostStatistics = new(
            services.GetRequiredService<HostStatisticsRepository>(),
            websiteCatalog,
            persistenceEnabled: !runtimeOptions.Execution.IsLocal);
        SqlPageLinkService pageLinks = new(
            pagePersistence,
            services.GetRequiredService<WebsitePageMetricsRepository>(),
            robotsPolicy,
            runtimeOptions.Scraping.MaxPagesPerWebsite);
        ListingLifecycleService listingLifecycle = new(
            listingStore,
            notListingCache,
            pageLinks,
            databaseAdapters.CreateListingEnricher(
                goolzoom,
                runtimeOptions.Integrations.GoogleCloudLanderistApiKey,
                CreateVertexAddressSelectorOptions(runtimeOptions),
                logger),
            new LegacyListingUnpublishPolicy(listingQueries),
            logger,
            new HtmlPageContentInspector());
        LanderistScrapingPipeline scrapingPipeline = services
            .GetRequiredService<LanderistScrapingPipelineFactory>()
            .Create(
                pagePersistence,
                listingLifecycle,
                notListingCache,
                hostStatistics,
                listingParser,
                pageLinks,
                listingStore,
                pageQueryOptions);
        Scraper scraper = scrapingPipeline.Scraper;
        ScrapeBatchServices batchScraping = scrapingPipeline.BatchServices;
        ParsedPageClassificationService parsedClassification =
            scrapingPipeline.ParsedClassification;
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
            TokenizerOptions.ForProvider(runtimeOptions.Ai.Provider));
        int batchMaxInputTokens =
            TokenizerOptions.ForProvider(runtimeOptions.Ai.Provider).MaxContextWindow -
            batchTokenizer.CountSystemTokens();
        BatchProvider batchProvider = runtimeOptions.Ai.Provider switch
        {
            LLMProvider.OpenAI => BatchProvider.OpenAI,
            LLMProvider.VertexAI => BatchProvider.VertexAI,
            _ => throw new InvalidOperationException(
                $"Batch upload is not supported for {runtimeOptions.Ai.Provider}.")
        };
        BatchUploadOptions batchUploadOptions = new(
            batchProvider,
            runtimeOptions.Batch.MaxPages,
            runtimeOptions.Batch.MinPages,
            batchMaxInputTokens,
            updateWaitingResponse: runtimeOptions.Batch.UpdateWaitingResponse,
            statusUpdateParallelism:
                runtimeOptions.Batch.StatusUpdateParallelism);
        VertexBatchOptions vertexBatchOptions = new(
            runtimeOptions.Ai.VertexCredential,
            runtimeOptions.Ai.VertexProjectId,
            runtimeOptions.Ai.VertexLocation,
            runtimeOptions.Ai.VertexListingModel,
            runtimeOptions.Batch.VertexBucketName,
            runtimeOptions.Batch.Directory);
        VertexBatchJobClient vertexBatchJobs = new(vertexBatchOptions, logger);
        VertexCloudStorageClient vertexStorage = new(vertexBatchOptions, logger);
        OpenAIBatchOptions openAIBatchOptions = new(
            runtimeOptions.Ai.OpenAiApiKey,
            OpenAIListingParserOptions.DefaultModel,
            runtimeOptions.Batch.Directory);
        OpenAIBatchClient openAIBatchClient = new(openAIBatchOptions, logger);
        OpenAIBatchUpload openAIBatchUpload = new(
            openAIBatchOptions,
            SystemPrompt.Text,
            StructuredOutputSchema.GetJsonSchemaString());
        ListingBatchUploadProviderCatalog batchUploadProviders = new(
        [
            new OpenAIBatchUploadProvider(openAIBatchUpload, openAIBatchClient),
            new VertexAIBatchUploadProvider(
                SystemPrompt.Text,
                OpenApiSchemaSerializer.Serialize(VertexAIResponseSchema.ResponseSchema),
                vertexStorage.Upload,
                vertexBatchJobs.Create)
        ]);
        IListingBatchUploadProvider batchUploadProvider =
            batchUploadProviders.GetRequired(batchProvider);
        IBatchInputWriter batchInputWriter = new JsonlBatchInputWriter(
            new BatchInputWriterOptions(
                batchProvider,
                runtimeOptions.Batch.Directory,
                runtimeOptions.Batch.MaxFileSizeBytes,
                runtimeOptions.Batch.MinPages),
            batchUploadProvider,
            new PageListingInputPreparer(logger),
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
                new PageListingInputPreparer(logger),
                new LocalAiParsingTaskOptions(
                    modelMaxTokens: runtimeOptions.Execution.LocalAiMaxModelLength,
                    runSequentially: runtimeOptions.Execution.IsLocal,
                    updateWaitingStatusOnStart: runtimeOptions.Execution.IsProduction),
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
                        new BatchDownloadProvider(
                            BatchProvider.OpenAI,
                            new OpenAIBatchDownload(openAIBatchClient, logger)),
                        new BatchDownloadProvider(BatchProvider.VertexAI, new VertexAIBatchDownload(
                            vertexBatchJobs,
                            vertexStorage,
                            logger))
                    ]),
                    new LegacyBatchListingResponseParser(listingParser),
                    new BatchDownloadOptions(
                        runtimeOptions.Batch.StatusUpdateParallelism),
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
                    new BatchCleanupOptions(runtimeOptions.Batch.Directory),
                    new VertexBatchArtifactCleaner(
                        vertexBatchJobs,
                        vertexStorage,
                        runtimeOptions.Batch.CleanupAfterDays,
                        TimeProvider.System))),
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
    private static VertexAddressSelectorOptions CreateVertexAddressSelectorOptions(
        LanderistRuntimeOptions runtimeOptions) =>
        new(
            runtimeOptions.Ai.VertexCredential,
            runtimeOptions.Ai.VertexProjectId,
            runtimeOptions.Ai.VertexLocation,
            runtimeOptions.Ai.VertexPublisher,
            runtimeOptions.Ai.VertexAddressModel);

}
