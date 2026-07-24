using landerist_library.Application.Listings;
using landerist_library.Application.Logging;
using landerist_library.Application.Persistence;
using landerist_library.Application.Scraping;
using landerist_library.Pages;
using landerist_library.Scrape;
using landerist_library.Websites;
using landerist_orels.ES;

namespace landerist_unit_tests;

public sealed class PageScraperPipelineTests
{
    [Fact]
    public void Scrape_WhenDownloadFails_StopsBeforeClassification()
    {
        TestContext context = new(PageAcquisitionStatus.DownloadFailed);

        bool result = context.Scraper.Scrape();

        Assert.False(result);
        Assert.Equal(0, context.Classifier.Calls);
        Assert.Equal(0, context.Persistence.UpdateCalls);
        Assert.Equal(0, context.Indexing.Calls);
    }

    [Fact]
    public void Scrape_WhenDownloaded_ClassifiesPersistsAndIndexes()
    {
        Listing listing = new();
        TestContext context = new(PageAcquisitionStatus.Downloaded)
        {
            ClassifierResult = new PageClassificationResult(
                PageType.Listing,
                listing,
                false)
        };

        bool result = context.Scraper.Scrape();

        Assert.True(result);
        Assert.Equal(1, context.Classifier.Calls);
        Assert.Equal(1, context.ListingLifecycle.Calls);
        Assert.Same(listing, context.ListingLifecycle.LastListing);
        Assert.Equal(1, context.Scheduling.SetNextScrapeCalls);
        Assert.Equal(1, context.Persistence.UpdateCalls);
        Assert.Equal(1, context.Indexing.Calls);
    }

    [Fact]
    public void Scrape_WhenPersistenceFails_DoesNotIndex()
    {
        TestContext context = new(PageAcquisitionStatus.Downloaded);
        context.Persistence.UpdateResult = false;

        bool result = context.Scraper.Scrape();

        Assert.False(result);
        Assert.Equal(1, context.Classifier.Calls);
        Assert.Equal(1, context.Persistence.UpdateCalls);
        Assert.Equal(0, context.Indexing.Calls);
    }

    [Fact]
    public void Scrape_WhenNotModified_PersistsExistingClassificationWithoutParsingOrIndexing()
    {
        TestContext context = new(PageAcquisitionStatus.NotModified);
        context.Page.SetPageType(PageType.Listing);

        bool result = context.Scraper.Scrape();

        Assert.True(result);
        Assert.Equal(0, context.Classifier.Calls);
        Assert.Equal(1, context.ListingLifecycle.Calls);
        Assert.Equal(1, context.Scheduling.SetNextScrapeCalls);
        Assert.Equal(1, context.Persistence.UpdateCalls);
        Assert.Equal(0, context.Indexing.Calls);
    }

    [Fact]
    public void Scrape_WhenClassifierWaitsForAiWithoutBody_DoesNotPersistOrIndex()
    {
        TestContext context = new(PageAcquisitionStatus.Downloaded)
        {
            ClassifierResult = new PageClassificationResult(
                PageType.MayBeListing,
                null,
                true)
        };

        bool result = context.Scraper.Scrape();

        Assert.False(result);
        Assert.Equal(0, context.Persistence.UpdateCalls);
        Assert.Equal(0, context.Indexing.Calls);
        Assert.Single(context.Logger.Errors);
    }

    private sealed class TestContext
    {
        private readonly RecordingPageContentClassifier _classifier = new();

        public TestContext(PageAcquisitionStatus acquisitionStatus)
        {
            Page = new Page(
                new Website(new Uri("https://example.com")),
                new Uri("https://example.com/listing/1"));
            Acquisition.Status = acquisitionStatus;
            PageScrapePipelineServices pipeline = new(
                Acquisition,
                _classifier,
                Indexing,
                Scheduling,
                indexerEnabled: true);
            Scraper = new PageScraper(
                Page,
                Persistence,
                Logger,
                ListingLifecycle,
                pipeline);
        }

        public Page Page { get; }

        public RecordingPageAcquisitionService Acquisition { get; } = new();

        public RecordingPagePersistenceService Persistence { get; } = new();

        public RecordingApplicationLogger Logger { get; } = new();

        public RecordingListingLifecycleService ListingLifecycle { get; } = new();

        public RecordingPageIndexingService Indexing { get; } = new();

        public RecordingPageSchedulingService Scheduling { get; } = new();

        public PageScraper Scraper { get; }

        public RecordingPageContentClassifier Classifier => _classifier;

        public PageClassificationResult ClassifierResult
        {
            set => _classifier.Result = value;
        }
    }

    private sealed class RecordingPageAcquisitionService : IPageAcquisitionService
    {
        public PageAcquisitionStatus Status { get; set; }

        public PageAcquisitionStatus Acquire(Page page, bool useProxy) => Status;
    }

    private sealed class RecordingPageContentClassifier : IPageContentClassifier
    {
        public PageClassificationResult Result { get; set; } =
            new(PageType.MainPage, null, false);

        public int Calls { get; private set; }

        public PageClassificationResult Classify(Page page)
        {
            Calls++;
            return Result;
        }
    }

    private sealed class RecordingPagePersistenceService : IPagePersistenceService
    {
        public bool UpdateResult { get; set; } = true;

        public int UpdateCalls { get; private set; }

        public bool Insert(Page page) => true;

        public bool Update(Page page)
        {
            UpdateCalls++;
            return UpdateResult;
        }

        public bool UpdateNextScrape(Page page) => true;

        public bool Delete(Page page) => true;

        public bool ListingParserInputExistsOnAnotherListing(Page page) => false;
    }

    private sealed class RecordingApplicationLogger : IApplicationLogger
    {
        public List<(string Source, string Message)> Errors { get; } = [];

        public void WriteError(string source, string message) =>
            Errors.Add((source, message));

        public void WriteInfo(string source, string message)
        {
        }
    }

    private sealed class RecordingListingLifecycleService : IListingLifecycleService
    {
        public int Calls { get; private set; }

        public Listing? LastListing { get; private set; }

        public void Apply(Page page, Listing? listing)
        {
            Calls++;
            LastListing = listing;
        }
    }

    private sealed class RecordingPageIndexingService : IPageIndexingService
    {
        public int Calls { get; private set; }

        public void Index(Page page) => Calls++;
    }

    private sealed class RecordingPageSchedulingService : IPageSchedulingService
    {
        public int SetNextScrapeCalls { get; private set; }

        public void SetNextScrape(Page page) => SetNextScrapeCalls++;

        public void SetNextScrapeFromNow(Page page)
        {
        }
    }
}
