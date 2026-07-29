using landerist_library.Application.Listings;
using landerist_library.Application.Scraping;
using landerist_library.Infrastructure.Listings;
using landerist_library.Infrastructure.Runtime;
using landerist_library.Infrastructure.Scraping;
using landerist_library.Infrastructure.Sql;
using landerist_library.Infrastructure.WebsiteServices;
using landerist_library.Websites;

namespace landerist_console;

internal sealed record LanderistScrapeExecution(
    PageBatchSelector PageBatchSelector,
    ScrapeBatchServices BatchServices);

internal sealed class LanderistScrapeExecutionComposition(
    LanderistRuntimeOptions runtimeOptions,
    LanderistDatabaseAdapterFactory databaseAdapters,
    ScrapeBrowserManager browser,
    WebsiteRobotsPolicy robotsPolicy)
{
    public LanderistScrapeExecution Create(
        SqlListingStore listingStore,
        PageQueryOptions pageQueryOptions)
    {
        PageBatchSelector pageBatchSelector = new(
            databaseAdapters.CreatePageSelectionRepository(
                runtimeOptions.Execution.MachineName,
                pageQueryOptions),
            new PageSelectionOptions(
                runtimeOptions.Scraping.MaxPagesPerScrape,
                runtimeOptions.Scraping.MaxPagesPerHostPerScrape,
                runtimeOptions.Scraping.MinPagesPerScrape,
                enforceMinimumPages:
                    runtimeOptions.Execution.IsProduction));
        ScrapeBatchServices batchServices = new(
            databaseAdapters.CreateWebsiteThrottle(robotsPolicy),
            browser,
            databaseAdapters.CreatePageLockManager(
                runtimeOptions.Execution.MachineName),
            databaseAdapters.CreateScrapeBatchMetrics(),
            databaseAdapters.CreateScrapePageSource(listingStore),
            robotsPolicy,
            new ScraperExecutionOptions(
                runtimeOptions.Execution.IsProduction,
                runtimeOptions.Execution.IsLocal,
                runtimeOptions.Scraping.MaxDegreeOfParallelism));

        return new LanderistScrapeExecution(
            pageBatchSelector,
            batchServices);
    }
}