using landerist_library.Application.Persistence;
using landerist_library.Pages;
using landerist_library.Scrape;
using landerist_library.Websites;

namespace landerist_unit_tests;

public sealed class PageClassificationServiceTests
{
    [Fact]
    public void Constructor_RejectsNullPersistenceService()
    {
        Page page = CreatePage();

        Assert.Throws<ArgumentNullException>(() =>
            new PageScraper(page, (IPagePersistenceService)null!));
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
        PageScraper scraper = new(page, persistence);

        bool result = scraper.TryApplyPreClassificationBeforeDownload();

        Assert.False(result);
        Assert.Equal(0, persistence.UpdateCalls);
    }

    [Fact]
    public void ParsedMaybeListing_DoesNotPersist()
    {
        Page page = CreatePage();
        RecordingPagePersistenceService persistence = new();
        PageScraper scraper = new(page, persistence);

        bool result = scraper.ApplyParsedClassificationAfterParsing(
            PageType.MayBeListing,
            listing: null);

        Assert.False(result);
        Assert.Equal(0, persistence.UpdateCalls);
    }

    [Fact]
    public void Scraper_AcceptsInjectedPersistenceService()
    {
        RecordingPagePersistenceService persistence = new();

        Scraper scraper = new(persistence);

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
}
