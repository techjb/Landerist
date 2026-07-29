using landerist_library.Infrastructure.Parsing.Tokenization;
using landerist_domain.Parsing.Tokenization;
using landerist_library.Application.Listings;
using landerist_library.Application.Logging;
using landerist_library.Application.Persistence;
using landerist_library.Application.Scraping;
using landerist_library.Application.Statistics;
using landerist_library.Infrastructure.Runtime;
using landerist_library.Infrastructure.Listings;
using landerist_library.Infrastructure.Logging;
using landerist_library.Infrastructure.Parsing;
using landerist_library.Infrastructure.Parsing.UserInput;
using landerist_library.Infrastructure.Scraping;
using landerist_library.Infrastructure.Sql;
using landerist_library.Infrastructure.WebsiteServices;
using landerist_library.Websites;

namespace landerist_console;

internal sealed record LanderistScrapingPipeline(
    Scraper Scraper,
    ScrapeBatchServices BatchServices,
    ParsedPageClassificationService ParsedClassification);

internal sealed class LanderistScrapingPipelineFactory(
    LanderistRuntimeOptions runtimeOptions,
    LanderistDatabaseAdapterFactory databaseAdapters,
    PooledPageDownloader pageDownloader,
    HttpConditionalPageHeaderService conditionalHeaders,
    ScrapeBrowserManager browser,
    WebsiteRobotsPolicy robotsPolicy,
    IApplicationLogger logger)
{
    public LanderistScrapingPipeline Create(
        PagePersistenceService pagePersistence,
        ListingLifecycleService listingLifecycle,
        SqlNotListingCacheService notListingCache,
        HostStatistics hostStatistics,
        ParseListing listingParser,
        SqlPageLinkService pageLinks,
        SqlListingStore listingStore,
        PageQueryOptions pageQueryOptions)
    {
        PageScrapePipelineServices pageScraping = new(
            new PageAcquisitionService(
                pageDownloader,
                conditionalHeaders,
                databaseAdapters.CreateScrapeMetrics(),
                conditionalHeadersEnabled: !runtimeOptions.Execution.IsLocal),
            new PageContentClassifier(
                runtimeOptions.Execution.IsProduction,
                notListingCache,
                databaseAdapters.CreatePageClassificationMetrics(),
                new LegacyListingPageParser(hostStatistics, listingParser),
                new LegacyPageTokenLimitPolicy(
                    new Tokenizer(
                        TokenizerOptions.ForProvider(runtimeOptions.Ai.Provider))),
                new HtmlPageContentInspector(),
                new PageListingInputPreparer(logger)),
            new PageIndexingService(
                runtimeOptions.Scraping.IndexerEnabled,
                pageLinks,
                new HtmlPageLinkExtractor()),
            new SqlPageSchedulingService(listingStore),
            runtimeOptions.Scraping.IndexerEnabled);
        PageBatchSelector pageBatchSelector = new(
            databaseAdapters.CreatePageSelectionRepository(
                runtimeOptions.Execution.MachineName,
                pageQueryOptions),
            new PageSelectionOptions(
                runtimeOptions.Scraping.MaxPagesPerScrape,
                runtimeOptions.Scraping.MaxPagesPerHostPerScrape,
                runtimeOptions.Scraping.MinPagesPerScrape,
                enforceMinimumPages: runtimeOptions.Execution.IsProduction));
        ScrapeBatchServices batchServices = new(
            databaseAdapters.CreateWebsiteThrottle(robotsPolicy),
            browser,
            databaseAdapters.CreatePageLockManager(runtimeOptions.Execution.MachineName),
            databaseAdapters.CreateScrapeBatchMetrics(),
            databaseAdapters.CreateScrapePageSource(listingStore),
            robotsPolicy,
            new ScraperExecutionOptions(
                runtimeOptions.Execution.IsProduction,
                runtimeOptions.Execution.IsLocal,
                runtimeOptions.Scraping.MaxDegreeOfParallelism));
        ParsedPageClassificationService parsedClassification = new(
            pagePersistence,
            listingLifecycle);
        Scraper scraper = new(
            pagePersistence,
            logger,
            listingLifecycle,
            pageScraping,
            pageBatchSelector,
            batchServices,
            new ConsoleScrapeProgressReporter());

        return new LanderistScrapingPipeline(
            scraper,
            batchServices,
            parsedClassification);
    }
}
