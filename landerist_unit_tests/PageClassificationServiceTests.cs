using landerist_library.Application.Listings;
using landerist_library.Application.Logging;
using landerist_library.Application.Persistence;
using landerist_library.Application.Scraping;
using landerist_library.Pages;
using landerist_library.Websites;
using landerist_orels.ES;

namespace landerist_unit_tests;

public sealed class PageClassificationServiceTests
{
    [Fact]
    public void Constructor_RejectsNullPersistenceService()
    {
        Page page = CreatePage();

        Assert.Throws<ArgumentNullException>(() =>
            new PageScraper(
                page,
                (IPagePersistenceService)null!,
                new RecordingApplicationLogger(),
                new RecordingListingLifecycleService(),
                CreatePipeline()));
    }

    [Fact]
    public void Constructor_RejectsNullLogger()
    {
        Page page = CreatePage();

        Assert.Throws<ArgumentNullException>(() =>
            new PageScraper(
                page,
                new RecordingPagePersistenceService(),
                (IApplicationLogger)null!,
                new RecordingListingLifecycleService(),
                CreatePipeline()));
    }

    [Fact]
    public void Constructor_RejectsNullListingLifecycle()
    {
        Page page = CreatePage();

        Assert.Throws<ArgumentNullException>(() =>
            new PageScraper(
                page,
                new RecordingPagePersistenceService(),
                new RecordingApplicationLogger(),
                (IListingLifecycleService)null!,
                CreatePipeline()));
    }

    [Fact]
    public void Constructor_RejectsNullPipeline()
    {
        Page page = CreatePage();

        Assert.Throws<ArgumentNullException>(() =>
            new PageScraper(
                page,
                new RecordingPagePersistenceService(),
                new RecordingApplicationLogger(),
                new RecordingListingLifecycleService(),
                (PageScrapePipelineServices)null!));
    }

    [Fact]
    public void PreClassification_WhenHtmlIndexingIsEnabled_DoesNotPersist()
    {
        Website website = new(new Uri("https://example.com"))
        {
            HtmlIndexingEnabled = true
        };
        Page page = new(website, new Uri("https://example.com/listing/1"));
        RecordingPagePersistenceService persistence = new();
        RecordingApplicationLogger logger = new();
        RecordingListingLifecycleService listingLifecycle = new();
        RecordingPageSchedulingService scheduling = new();
        PageScraper scraper = new(
            page,
            persistence,
            logger,
            listingLifecycle,
            CreatePipeline(scheduling));

        bool result = scraper.TryApplyPreClassificationBeforeDownload();

        Assert.False(result);
        Assert.Equal(0, persistence.UpdateCalls);
        Assert.Equal(0, listingLifecycle.ApplyCalls);
        Assert.Equal(0, scheduling.TotalCalls);
    }

    [Fact]
    public void ParsedMaybeListing_DoesNotPersist()
    {
        Page page = CreatePage();
        RecordingPagePersistenceService persistence = new();
        RecordingApplicationLogger logger = new();
        RecordingListingLifecycleService listingLifecycle = new();
        PageScraper scraper = new(
            page,
            persistence,
            logger,
            listingLifecycle,
            CreatePipeline());

        bool result = scraper.ApplyParsedClassificationAfterParsing(
            PageType.MayBeListing,
            listing: null);

        Assert.False(result);
        Assert.Equal(0, persistence.UpdateCalls);
        Assert.Equal(0, listingLifecycle.ApplyCalls);
    }

    [Fact]
    public void ParsedClassification_DelegatesListingLifecycleAndPersists()
    {
        Page page = CreatePage();
        Listing listing = new();
        RecordingPagePersistenceService persistence = new();
        RecordingApplicationLogger logger = new();
        RecordingListingLifecycleService listingLifecycle = new();
        PageScraper scraper = new(
            page,
            persistence,
            logger,
            listingLifecycle,
            CreatePipeline());

        bool result = scraper.ApplyParsedClassificationAfterParsing(
            PageType.Listing,
            listing);

        Assert.True(result);
        Assert.Equal(1, persistence.UpdateCalls);
        Assert.Equal(1, listingLifecycle.ApplyCalls);
        Assert.Same(page, listingLifecycle.LastPage);
        Assert.Same(listing, listingLifecycle.LastListing);
    }

    [Fact]
    public void WaitingAiRequestWithoutResponseBody_LogsErrorWithoutPersisting()
    {
        Page page = CreatePage();
        RecordingPagePersistenceService persistence = new();
        RecordingApplicationLogger logger = new();
        RecordingListingLifecycleService listingLifecycle = new();
        PageScraper scraper = new(
            page,
            persistence,
            logger,
            listingLifecycle,
            CreatePipeline());

        bool result = scraper.ApplyClassificationResultAfterDownload(
            PageType.MayBeListing,
            newListing: null,
            waitingAIRequest: true);

        Assert.False(result);
        Assert.Equal(0, persistence.UpdateCalls);
        Assert.Equal(0, listingLifecycle.ApplyCalls);
        var error = Assert.Single(logger.Errors);
        Assert.Equal("PageScraper SetPageType", error.Source);
        Assert.Equal("Failed to set response body zipped", error.Message);
    }

    [Fact]
    public void Scraper_AcceptsInjectedServices()
    {
        RecordingPagePersistenceService persistence = new();
        RecordingApplicationLogger logger = new();
        RecordingListingLifecycleService listingLifecycle = new();

        Scraper scraper = new(
            persistence,
            logger,
            listingLifecycle,
            CreatePipeline(),
            new NullPageBatchSelector(),
            ScrapeBatchTestFactory.Create(),
            new NullScrapeProgressReporter());

        Assert.NotNull(scraper);
    }

    private static PageScrapePipelineServices CreatePipeline(
        IPageSchedulingService? scheduling = null) =>
        new(
            new NullPageAcquisitionService(),
            new NullPageContentClassifier(),
            new NullPageIndexingService(),
            scheduling ?? new RecordingPageSchedulingService(),
            indexerEnabled: true);

    private static Page CreatePage() =>
        new(
            new Website(new Uri("https://example.com")),
            new Uri("https://example.com/listing/1"));

    private sealed class RecordingPagePersistenceService : IPagePersistenceService
    {
        public int UpdateCalls { get; private set; }

        public bool Insert(Page page) => true;

        public bool Update(Page page)
        {
            UpdateCalls++;
            return true;
        }

        public bool UpdateNextScrape(Page page) => true;

        public bool Delete(Page page) => true;

        public bool ListingParserInputExistsOnAnotherListing(Page page) => false;
    }

    private sealed class RecordingApplicationLogger : IApplicationLogger
    {
        public List<(string Source, string Message)> Errors { get; } = [];

        public List<(string Source, string Message)> Infos { get; } = [];

        public void WriteError(string source, string message) =>
            Errors.Add((source, message));

        public void WriteInfo(string source, string message) =>
            Infos.Add((source, message));
    }

    private sealed class RecordingListingLifecycleService : IListingLifecycleService
    {
        public int ApplyCalls { get; private set; }

        public Page? LastPage { get; private set; }

        public Listing? LastListing { get; private set; }

        public void Apply(Page page, Listing? listing)
        {
            ApplyCalls++;
            LastPage = page;
            LastListing = listing;
        }
    }

    private sealed class NullPageAcquisitionService : IPageAcquisitionService
    {
        public PageAcquisitionStatus Acquire(Page page, bool useProxy) =>
            PageAcquisitionStatus.DownloadFailed;
    }

    private sealed class NullPageContentClassifier : IPageContentClassifier
    {
        public PageClassificationResult Classify(Page page) =>
            new(null, null, false);
    }

    private sealed class NullPageIndexingService : IPageIndexingService
    {
        public void Index(Page page)
        {
        }
    }

    private sealed class RecordingPageSchedulingService : IPageSchedulingService
    {
        public int TotalCalls { get; private set; }

        public void SetNextScrape(Page page) => TotalCalls++;

        public void SetNextScrapeFromNow(Page page) => TotalCalls++;
    }
    private sealed class NullPageBatchSelector : IPageBatchSelector
    {
        public IReadOnlyList<Page> Select() => [];
    }
}
