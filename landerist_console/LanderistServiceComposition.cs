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
        ParseListing listingParser = LanderistAiComposition.CreateListingParser(
            runtimeOptions,
            websiteAccess,
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
                LanderistAiComposition.CreateAddressSelectorOptions(runtimeOptions.Ai),
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

        LanderistBatchTasks batchTasks = LanderistBatchComposition.Create(
            runtimeOptions,
            databaseAdapters,
            logger,
            parsedClassification,
            globalStatistics,
            pageCatalog,
            pagePersistence,
            waitingStatus,
            listingParser);
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
            batchTasks.TenMinute,
            new HourlyTaskJob(
                new WebsiteRefreshService(
                    websiteCatalog,
                    websitePersistence,
                    websiteNetwork,
                    websiteSitemaps),
                batchTasks.Cleaner),
            LanderistDistributionComposition.CreateDailyJob(
                databaseAdapters,
                notListingCache,
                globalStatistics,
                hostStatistics,
                pageStatistics,
                websiteMetrics,
                websiteCatalog,
                websiteQueries,
                services,
                logger),
            TimeProvider.System);
    }
}
