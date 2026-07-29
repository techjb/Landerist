using landerist_library.Application.Scraping;
using landerist_library.Pages;
using landerist_library.Websites;
using landerist_orels.ES;

namespace landerist_unit_tests;

internal static class ScrapeBatchTestFactory
{
    public static ScrapeBatchServices Create() =>
        new(
            new NullWebsiteThrottleService(),
            new NullScrapeResourceManager(),
            new NullScrapeResourceManager(),
            new NullScrapeBatchMetrics(),
            new NullScrapePageSource(),
            new StubWebsiteRobotsPolicy(),
            new ScraperExecutionOptions(
                isProduction: false,
                isLocal: true,
                maximumDegreeOfParallelism: 1));

    private sealed class NullWebsiteThrottleService : IWebsiteThrottleService
    {
        public bool Clean() => true;

        public Task<bool> CleanAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(true);

        public bool IsBlocked(Website website) => false;

        public Task<bool> IsBlockedAsync(
            Website website,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public bool TryAcquire(Website website) => true;

        public Task<bool> TryAcquireAsync(
            Website website,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(true);

        public bool ReportForbidden(Website website) => true;

        public bool ReportSuccess(Website website) => true;
    }

    private sealed class NullScrapeResourceManager : IScrapeBrowserManager, IPageLockManager
    {
        public void ClearDownloaders()
        {
        }

        public void CleanPageLocks()
        {
        }

        public Task CleanPageLocksAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }

        public void KillChrome()
        {
        }

        public void UpdateChrome()
        {
        }
    }

    private sealed class NullScrapeBatchMetrics : IScrapeBatchMetrics
    {
        public void Record(ScrapeBatchCounters counters)
        {
        }
    }

    private sealed class NullScrapePageSource : IScrapePageSource
    {
        public Page LoadOrCreate(Uri uri) =>
            new(new Website(uri), uri);

        public IReadOnlyList<Page> GetPages(Website website) => [];

        public Listing? GetListing(Page page, bool loadMedia, bool loadSources) => null;
    }
}