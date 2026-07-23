using landerist_library.Application.Listings;
using landerist_library.Application.Logging;
using landerist_library.Application.Persistence;
using landerist_library.Pages;
using landerist_library.Scrape;
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
                new RecordingListingLifecycleService()));
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
                new RecordingListingLifecycleService()));
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
                (IListingLifecycleService)null!));
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
        PageScraper scraper = new(page, persistence, logger, listingLifecycle);

        bool result = scraper.TryApplyPreClassificationBeforeDownload();

        Assert.False(result);
        Assert.Equal(0, persistence.UpdateCalls);
        Assert.Equal(0, listingLifecycle.ApplyCalls);
    }

    [Fact]
    public void ParsedMaybeListing_DoesNotPersist()
    {
        Page page = CreatePage();
        RecordingPagePersistenceService persistence = new();
        RecordingApplicationLogger logger = new();
        RecordingListingLifecycleService listingLifecycle = new();
        PageScraper scraper = new(page, persistence, logger, listingLifecycle);

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
        PageScraper scraper = new(page, persistence, logger, listingLifecycle);

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
        PageScraper scraper = new(page, persistence, logger, listingLifecycle);

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

        Scraper scraper = new(persistence, logger, listingLifecycle);

        Assert.NotNull(scraper);
    }

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
}
