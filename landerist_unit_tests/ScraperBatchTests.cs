using landerist_library.Application.Listings;
using landerist_library.Application.Logging;
using landerist_library.Application.Persistence;
using landerist_library.Application.Scraping;
using landerist_library.Pages;
using landerist_library.Websites;
using landerist_orels.ES;

namespace landerist_unit_tests;

public sealed class ScraperBatchTests
{
    [Fact]
    public void RunBatch_WhenPageSucceeds_RecordsMetricsAndReleasesResources()
    {
        TestContext context = CreateContext();

        bool result = context.Scraper.RunBatch();

        Assert.True(result);
        Assert.Equal(
            new ScrapeBatchCounters(1, 1, 1, 0, 0, 0, 0, 0),
            Assert.Single(context.Metrics.Records));
        Assert.Equal(1, context.Throttle.CleanCalls);
        Assert.Equal(1, context.Throttle.AcquireCalls);
        Assert.Equal(2, context.Resources.ClearDownloadersCalls);
        Assert.Equal(1, context.Resources.KillChromeCalls);
    }

    [Fact]
    public async Task RunBatchAsync_CleansThrottleAsynchronously()
    {
        TestContext context = CreateContext();

        bool result = await context.Scraper.RunBatchAsync(CancellationToken.None);

        Assert.True(result);
        Assert.Equal(0, context.Throttle.CleanCalls);
        Assert.Equal(1, context.Throttle.CleanAsyncCalls);
        Assert.Equal(0, context.Throttle.AcquireCalls);
        Assert.Equal(1, context.Throttle.IsBlockedAsyncCalls);
        Assert.Equal(1, context.Throttle.AcquireAsyncCalls);
        Assert.Equal(0, context.Acquisition.Calls);
        Assert.Equal(1, context.Acquisition.AsyncCalls);
        Assert.Single(context.Metrics.Records);
    }
    [Fact]
    public async Task RunBatchAsync_WhenAlreadyCancelled_DoesNotSelectOrScrapePages()
    {
        TestContext context = CreateContext();
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        bool result = await context.Scraper.RunBatchAsync(cancellation.Token);

        Assert.False(result);
        Assert.Equal(0, context.Throttle.CleanAsyncCalls);
        Assert.Equal(0, context.Throttle.IsBlockedAsyncCalls);
        Assert.Equal(0, context.Throttle.AcquireAsyncCalls);
        Assert.Empty(context.Metrics.Records);
    }
    [Fact]
    public void Stop_ReleasesBrowserResourcesAndCleansPageLocks()
    {
        TestContext context = CreateContext();

        context.Scraper.Stop();

        Assert.Equal(1, context.Resources.ClearDownloadersCalls);
        Assert.Equal(1, context.Resources.CleanPageLocksCalls);
        Assert.Equal(1, context.Resources.KillChromeCalls);
    }
    [Fact]
    public async Task StopAsync_CleansPageLocksAsynchronouslyAndReleasesBrowserResources()
    {
        TestContext context = CreateContext();

        await context.Scraper.StopAsync(CancellationToken.None);

        Assert.Equal(1, context.Resources.ClearDownloadersCalls);
        Assert.Equal(0, context.Resources.CleanPageLocksCalls);
        Assert.Equal(1, context.Resources.CleanPageLocksAsyncCalls);
        Assert.Equal(1, context.Resources.KillChromeCalls);
    }

    [Fact]
    public async Task StopAsync_WhenPageLockCleanupFails_StillKillsChrome()
    {
        TestContext context = CreateContext();
        context.Resources.CleanPageLocksAsyncException =
            new InvalidOperationException("cleanup failed");

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => context.Scraper.StopAsync(CancellationToken.None));

        Assert.Equal("cleanup failed", exception.Message);
        Assert.Equal(1, context.Resources.ClearDownloadersCalls);
        Assert.Equal(1, context.Resources.CleanPageLocksAsyncCalls);
        Assert.Equal(1, context.Resources.KillChromeCalls);
    }
    [Fact]
    public void RunBatch_WhenWebsiteIsBlocked_SkipsDownload()
    {
        TestContext context = CreateContext();
        context.Throttle.Blocked = true;

        context.Scraper.RunBatch();

        ScrapeBatchCounters counters = Assert.Single(context.Metrics.Records);
        Assert.Equal(1, counters.SkippedByBlockedWebsite);
        Assert.Equal(0, counters.Processed);
        Assert.Equal(0, context.Acquisition.Calls);
        Assert.Equal(0, context.Throttle.AcquireCalls);
    }

    [Fact]
    public void RunBatch_WhenThrottleCannotBeAcquiredInProduction_SkipsPage()
    {
        TestContext context = CreateContext(isProduction: true);
        context.Throttle.CanAcquire = false;

        context.Scraper.RunBatch();

        ScrapeBatchCounters counters = Assert.Single(context.Metrics.Records);
        Assert.Equal(1, counters.SkippedByBlockedWebsite);
        Assert.Equal(0, counters.Processed);
        Assert.Equal(0, context.Acquisition.Calls);
    }

    [Fact]
    public void RunBatch_WhenDownloadFails_RecordsCrash()
    {
        TestContext context = CreateContext();
        context.Acquisition.Status = PageAcquisitionStatus.DownloadFailed;

        context.Scraper.RunBatch();

        ScrapeBatchCounters counters = Assert.Single(context.Metrics.Records);
        Assert.Equal(1, counters.Processed);
        Assert.Equal(1, counters.Crashed);
        Assert.Equal(0, counters.ScrapedSuccess);
    }

    [Fact]
    public void RunBatch_WhenResponseIsForbidden_ReportsThrottleAndDownloadError()
    {
        TestContext context = CreateContext();
        context.Acquisition.OnAcquire = page => page.HttpStatusCode = 403;
        context.Classifier.PageType = PageType.HttpStatusCodeOtherNotOK;

        context.Scraper.RunBatch();

        ScrapeBatchCounters counters = Assert.Single(context.Metrics.Records);
        Assert.Equal(1, counters.Processed);
        Assert.Equal(1, counters.DownloadErrors);
        Assert.Equal(1, context.Throttle.ReportForbiddenCalls);
        Assert.Equal(0, context.Throttle.ReportSuccessCalls);
    }

    [Fact]
    public async Task RunBatchAsync_WhenResponseIsForbidden_ReportsThrottleAsynchronously()
    {
        TestContext context = CreateContext();
        context.Acquisition.OnAcquire = page => page.HttpStatusCode = 403;
        context.Classifier.PageType = PageType.HttpStatusCodeOtherNotOK;

        await context.Scraper.RunBatchAsync(CancellationToken.None);

        ScrapeBatchCounters counters = Assert.Single(context.Metrics.Records);
        Assert.Equal(1, counters.DownloadErrors);
        Assert.Equal(0, context.Throttle.ReportForbiddenCalls);
        Assert.Equal(1, context.Throttle.ReportForbiddenAsyncCalls);
        Assert.Equal(0, context.Throttle.ReportSuccessAsyncCalls);
    }
    private static TestContext CreateContext(bool isProduction = false)
    {
        Page page = new(
            new Website(new Uri("https://example.com")),
            new Uri("https://example.com/listing/1"));
        RecordingPageAcquisitionService acquisition = new();
        RecordingPageContentClassifier classifier = new();
        RecordingWebsiteThrottleService throttle = new();
        RecordingScrapeResourceManager resources = new();
        RecordingScrapeBatchMetrics metrics = new();
        PageScrapePipelineServices pipeline = new(
            acquisition,
            classifier,
            new NullPageIndexingService(),
            new NullPageSchedulingService(),
            indexerEnabled: true);
        ScrapeBatchServices batchServices = new(
            throttle,
            resources,
            resources,
            metrics,
            new NullScrapePageSource(),
            new StubWebsiteRobotsPolicy(),
            new ScraperExecutionOptions(
                isProduction,
                isLocal: true,
                maximumDegreeOfParallelism: 1));
        Scraper scraper = new(
            new RecordingPagePersistenceService(),
            new NullApplicationLogger(),
            new NullListingLifecycleService(),
            pipeline,
            new RecordingPageBatchSelector([page]),
            batchServices,
            new NullScrapeProgressReporter());

        return new TestContext(
            scraper,
            acquisition,
            classifier,
            throttle,
            resources,
            metrics);
    }

    private sealed record TestContext(
        Scraper Scraper,
        RecordingPageAcquisitionService Acquisition,
        RecordingPageContentClassifier Classifier,
        RecordingWebsiteThrottleService Throttle,
        RecordingScrapeResourceManager Resources,
        RecordingScrapeBatchMetrics Metrics);

    private sealed class RecordingPageBatchSelector(IReadOnlyList<Page> pages) : IPageBatchSelector
    {
        public IReadOnlyList<Page> Select() => pages;
    }

    private sealed class RecordingPageAcquisitionService : IPageAcquisitionService
    {
        public PageAcquisitionStatus Status { get; set; } = PageAcquisitionStatus.Downloaded;

        public Action<Page>? OnAcquire { get; set; }

        public int Calls { get; private set; }

        public int AsyncCalls { get; private set; }

        public PageAcquisitionStatus Acquire(Page page, bool useProxy)
        {
            Calls++;
            OnAcquire?.Invoke(page);
            return Status;
        }

        public Task<PageAcquisitionStatus> AcquireAsync(
            Page page,
            bool useProxy,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            AsyncCalls++;
            OnAcquire?.Invoke(page);
            return Task.FromResult(Status);
        }
    }
    private sealed class RecordingPageContentClassifier : IPageContentClassifier
    {
        public PageType PageType { get; set; } = PageType.MainPage;

        public PageClassificationResult Classify(Page page) =>
            new(PageType, null, false);
    }

    private sealed class RecordingWebsiteThrottleService : IWebsiteThrottleService
    {
        public bool Blocked { get; set; }

        public bool CanAcquire { get; set; } = true;

        public int CleanCalls { get; private set; }

        public int CleanAsyncCalls { get; private set; }

        public int AcquireCalls { get; private set; }

        public int IsBlockedAsyncCalls { get; private set; }

        public int AcquireAsyncCalls { get; private set; }

        public int ReportForbiddenCalls { get; private set; }

        public int ReportSuccessCalls { get; private set; }

        public int ReportForbiddenAsyncCalls { get; private set; }

        public int ReportSuccessAsyncCalls { get; private set; }

        public bool Clean()
        {
            CleanCalls++;
            return true;
        }

        public Task<bool> CleanAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CleanAsyncCalls++;
            return Task.FromResult(true);
        }

        public bool IsBlocked(Website website) => Blocked;

        public Task<bool> IsBlockedAsync(
            Website website,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            IsBlockedAsyncCalls++;
            return Task.FromResult(Blocked);
        }

        public bool TryAcquire(Website website)
        {
            AcquireCalls++;
            return CanAcquire;
        }

        public Task<bool> TryAcquireAsync(
            Website website,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            AcquireAsyncCalls++;
            return Task.FromResult(CanAcquire);
        }

        public bool ReportForbidden(Website website)
        {
            ReportForbiddenCalls++;
            return true;
        }

        public Task<bool> ReportForbiddenAsync(
            Website website,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ReportForbiddenAsyncCalls++;
            return Task.FromResult(true);
        }

        public bool ReportSuccess(Website website)
        {
            ReportSuccessCalls++;
            return true;
        }

        public Task<bool> ReportSuccessAsync(
            Website website,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ReportSuccessAsyncCalls++;
            return Task.FromResult(true);
        }
    }

    private sealed class RecordingScrapeResourceManager : IScrapeBrowserManager, IPageLockManager
    {
        public int ClearDownloadersCalls { get; private set; }

        public int KillChromeCalls { get; private set; }

        public int CleanPageLocksCalls { get; private set; }

        public int CleanPageLocksAsyncCalls { get; private set; }

        public Exception? CleanPageLocksAsyncException { get; set; }

        public void ClearDownloaders() => ClearDownloadersCalls++;

        public void CleanPageLocks() => CleanPageLocksCalls++;

        public Task CleanPageLocksAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CleanPageLocksAsyncCalls++;
            return CleanPageLocksAsyncException is null
                ? Task.CompletedTask
                : Task.FromException(CleanPageLocksAsyncException);
        }

        public void KillChrome() => KillChromeCalls++;

        public void UpdateChrome()
        {
        }
    }

    private sealed class RecordingScrapeBatchMetrics : IScrapeBatchMetrics
    {
        public List<ScrapeBatchCounters> Records { get; } = [];

        public void Record(ScrapeBatchCounters counters) => Records.Add(counters);
    }

    private sealed class RecordingPagePersistenceService : IPagePersistenceService
    {
        public bool Insert(Page page) => true;

        public bool Update(Page page) => true;

        public bool UpdateNextScrape(Page page) => true;

        public bool Delete(Page page) => true;

        public bool ListingParserInputExistsOnAnotherListing(Page page) => false;
    }

    private sealed class NullApplicationLogger : IApplicationLogger
    {
        public void WriteError(string source, string message)
        {
        }

        public void WriteInfo(string source, string message)
        {
        }
    }

    private sealed class NullListingLifecycleService : IListingLifecycleService
    {
        public void Apply(Page page, Listing? listing)
        {
        }
    }

    private sealed class NullPageIndexingService : IPageIndexingService
    {
        public void Index(Page page)
        {
        }
    }

    private sealed class NullPageSchedulingService : IPageSchedulingService
    {
        public void SetNextScrape(Page page)
        {
        }

        public void SetNextScrapeFromNow(Page page)
        {
        }
    }

    private sealed class NullScrapePageSource : IScrapePageSource
    {
        public Page LoadOrCreate(Uri uri) => new(uri);

        public IReadOnlyList<Page> GetPages(Website website) => [];

        public Listing? GetListing(Page page, bool loadMedia, bool loadSources) => null;
    }
}
