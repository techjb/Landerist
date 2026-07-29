using landerist_library.Application.Listings;
using landerist_library.Application.Logging;
using landerist_library.Application.Persistence;
using landerist_library.Application.Scraping;
using landerist_library.Application.Statistics;
using landerist_library.Infrastructure.Listings;
using landerist_library.Infrastructure.Logging;
using landerist_library.Infrastructure.Parsing;
using landerist_library.Infrastructure.Sql;

namespace landerist_console;

internal sealed record LanderistScrapingPipeline(
    Scraper Scraper,
    ScrapeBatchServices BatchServices,
    ParsedPageClassificationService ParsedClassification);

internal sealed class LanderistScrapingPipelineFactory(
    LanderistPageScrapingComposition pageComposition,
    LanderistScrapeExecutionComposition executionComposition,
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
        PageScrapePipelineServices pageScraping = pageComposition.Create(
            notListingCache,
            hostStatistics,
            listingParser,
            pageLinks,
            listingStore);
        LanderistScrapeExecution execution = executionComposition.Create(
            listingStore,
            pageQueryOptions);
        ParsedPageClassificationService parsedClassification = new(
            pagePersistence,
            listingLifecycle);
        Scraper scraper = new(
            pagePersistence,
            logger,
            listingLifecycle,
            pageScraping,
            execution.PageBatchSelector,
            execution.BatchServices,
            new ConsoleScrapeProgressReporter());

        return new LanderistScrapingPipeline(
            scraper,
            execution.BatchServices,
            parsedClassification);
    }
}