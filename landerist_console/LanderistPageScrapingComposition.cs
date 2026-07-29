using landerist_domain.Parsing.Tokenization;
using landerist_library.Application.Listings;
using landerist_library.Application.Logging;
using landerist_library.Application.Scraping;
using landerist_library.Application.Statistics;
using landerist_library.Infrastructure.Listings;
using landerist_library.Infrastructure.Parsing;
using landerist_library.Infrastructure.Parsing.Tokenization;
using landerist_library.Infrastructure.Parsing.UserInput;
using landerist_library.Infrastructure.Runtime;
using landerist_library.Infrastructure.Scraping;
using landerist_library.Infrastructure.WebsiteServices;
using landerist_library.Parsing;

namespace landerist_console;

internal sealed class LanderistPageScrapingComposition(
    LanderistRuntimeOptions runtimeOptions,
    LanderistDatabaseAdapterFactory databaseAdapters,
    PooledPageDownloader pageDownloader,
    HttpConditionalPageHeaderService conditionalHeaders,
    IApplicationLogger logger)
{
    public PageScrapePipelineServices Create(
        SqlNotListingCacheService notListingCache,
        HostStatistics hostStatistics,
        ParseListing listingParser,
        SqlPageLinkService pageLinks,
        SqlListingStore listingStore) => new(
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
                        TokenizerOptions.ForProvider(
                            runtimeOptions.Ai.Provider))),
                new HtmlPageContentInspector(),
                new PageListingInputPreparer(logger)),
            new PageIndexingService(
                runtimeOptions.Scraping.IndexerEnabled,
                pageLinks,
                new HtmlPageLinkExtractor()),
            new SqlPageSchedulingService(listingStore),
            runtimeOptions.Scraping.IndexerEnabled);
}