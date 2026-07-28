using landerist_library.Application.Listings;
using landerist_library.Application.Persistence;
using landerist_library.Application.Scraping;
using landerist_library.Application.Statistics;
using landerist_library.Configuration;
using landerist_library.Infrastructure.Listings;
using landerist_library.Infrastructure.Logging;
using landerist_library.Infrastructure.Parsing;
using landerist_library.Infrastructure.Scraping;
using landerist_library.Infrastructure.Sql;
using landerist_library.Infrastructure.WebsiteServices;
using landerist_library.Parse.ListingParser;
using landerist_library.Websites;

namespace landerist_console;

internal sealed record LanderistScrapingPipeline(
    Scraper Scraper,
    ScrapeBatchServices BatchServices,
    ParsedPageClassificationService ParsedClassification);

internal sealed class LanderistScrapingPipelineFactory(
    LanderistDatabaseAdapterFactory databaseAdapters,
    PooledPageDownloader pageDownloader,
    HttpConditionalPageHeaderService conditionalHeaders,
    ScrapeBrowserManager browser,
    WebsiteRobotsPolicy robotsPolicy,
    LegacyApplicationLogger logger)
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
                conditionalHeadersEnabled: !Config.IsConfigurationLocal()),
            new PageContentClassifier(
                Config.IsConfigurationProduction(),
                notListingCache,
                databaseAdapters.CreatePageClassificationMetrics(),
                new LegacyListingPageParser(hostStatistics, listingParser),
                new LegacyPageTokenLimitPolicy(
                    new Tokenizer(
                        TokenizerOptions.ForProvider(Config.LLM_PROVIDER))),
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
        ScrapeBatchServices batchServices = new(
            databaseAdapters.CreateWebsiteThrottle(robotsPolicy),
            browser,
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
            batchServices,
            new ConsoleScrapeProgressReporter());

        return new LanderistScrapingPipeline(
            scraper,
            batchServices,
            parsedClassification);
    }
}
