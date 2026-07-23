using landerist_library.Application.Listings;
using landerist_library.Application.Persistence;
using landerist_library.Application.Scraping;
using landerist_library.Pages;
using landerist_library.Websites;
using landerist_orels.ES;

namespace landerist_unit_tests;

public sealed class ParsedPageClassificationServiceTests
{
    [Fact]
    public void Constructor_RejectsNullDependencies()
    {
        RecordingPagePersistenceService persistence = new();
        RecordingListingLifecycleService lifecycle = new();

        Assert.Throws<ArgumentNullException>(() =>
            new ParsedPageClassificationService(null!, lifecycle));
        Assert.Throws<ArgumentNullException>(() =>
            new ParsedPageClassificationService(persistence, null!));
    }

    [Fact]
    public void Apply_WhenClassificationIsMaybeListing_DoesNotMutateOrPersist()
    {
        RecordingPagePersistenceService persistence = new();
        RecordingListingLifecycleService lifecycle = new();
        ParsedPageClassificationService service = new(persistence, lifecycle);
        Page page = CreatePage();

        bool result = service.Apply(page, PageType.MayBeListing, null);

        Assert.False(result);
        Assert.Null(page.PageType);
        Assert.Equal(0, persistence.UpdateCalls);
        Assert.Equal(0, lifecycle.ApplyCalls);
    }

    [Fact]
    public void Apply_DelegatesLifecycleAndPersistsClassification()
    {
        RecordingPagePersistenceService persistence = new();
        RecordingListingLifecycleService lifecycle = new();
        ParsedPageClassificationService service = new(persistence, lifecycle);
        Page page = CreatePage();
        Listing listing = new();

        bool result = service.Apply(page, PageType.Listing, listing);

        Assert.True(result);
        Assert.Equal(PageType.Listing, page.PageType);
        Assert.Equal(1, persistence.UpdateCalls);
        Assert.Equal(1, lifecycle.ApplyCalls);
        Assert.Same(page, lifecycle.LastPage);
        Assert.Same(listing, lifecycle.LastListing);
    }

    [Fact]
    public void Apply_RejectsNullPage()
    {
        ParsedPageClassificationService service = new(
            new RecordingPagePersistenceService(),
            new RecordingListingLifecycleService());

        Assert.Throws<ArgumentNullException>(() =>
            service.Apply(null!, PageType.Listing, null));
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
