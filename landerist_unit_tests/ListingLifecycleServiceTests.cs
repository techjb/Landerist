using landerist_library.Application.Listings;
using landerist_library.Application.Logging;
using landerist_library.Application.Parsing;
using landerist_library.Pages;
using landerist_library.Websites;
using landerist_orels.ES;

namespace landerist_unit_tests;

public sealed class ListingLifecycleServiceTests
{
    [Fact]
    public void Apply_WhenPageIsListing_PublishesEnrichesAndUpserts()
    {
        Page page = CreatePage("https://example.com/listing/1");
        page.SetPageType(PageType.Listing);
        Listing listing = new() { guid = page.UriHash };
        TestContext context = new();

        context.Service.Apply(page, listing);

        Assert.Equal(ListingStatus.published, listing.listingStatus);
        Assert.NotNull(listing.listingDate);
        Assert.Equal(1, context.Enricher.Calls);
        Assert.Same(page, context.Enricher.LastPage);
        Assert.Same(listing, context.Enricher.LastListing);
        var upsert = Assert.Single(context.Store.Upserts);
        Assert.Same(listing, upsert.Listing);
        Assert.Null(upsert.Decision);
    }

    [Fact]
    public void Apply_WhenParserRejectsPage_InsertsNotListingCache()
    {
        Page page = CreatePage("https://example.com/listing/1");
        page.SetPageType(PageType.NotListingByParser);
        TestContext context = new();

        context.Service.Apply(page, listing: null);

        Assert.Equal(1, context.Cache.InsertCalls);
        Assert.Same(page, context.Cache.LastPage);
        Assert.Empty(context.Store.Upserts);
    }

    [Fact]
    public async Task ApplyAsync_WhenParserRejectsPage_UsesAsyncNotListingCache()
    {
        Page page = CreatePage("https://example.com/listing/1");
        page.SetPageType(PageType.NotListingByParser);
        TestContext context = new();

        await context.Service.ApplyAsync(page, listing: null, CancellationToken.None);

        Assert.Equal(0, context.Cache.InsertCalls);
        Assert.Equal(1, context.Cache.InsertAsyncCalls);
        Assert.Same(page, context.Cache.LastPage);
        Assert.Empty(context.Store.Upserts);
    }
    [Fact]
    public void Apply_WhenPolicyRequiresUnpublish_UnpublishesAndPassesDecision()
    {
        Page page = CreatePage("https://example.com/listing/1");
        page.SetPageType(PageType.HttpStatusCodeGone);
        Listing listing = new() { guid = page.UriHash };
        ListingUnpublishDecision decision = new(
            true,
            ListingUnpublishDecisionReason.EvidenceCounterReachedRequired,
            page.PageType,
            page.HttpStatusCode,
            page.PageTypeCounter ?? 0,
            1);
        TestContext context = new(decision);

        context.Service.Apply(page, listing);

        Assert.Equal(ListingStatus.unpublished, listing.listingStatus);
        Assert.NotNull(listing.unlistingDate);
        var upsert = Assert.Single(context.Store.Upserts);
        Assert.Same(listing, upsert.Listing);
        Assert.Same(decision, upsert.Decision);
    }

    [Fact]
    public void Apply_WhenMovedDestinationIsPublished_IndexesAndUnpublishesSource()
    {
        Page sourcePage = CreatePage("https://example.com/listing/1");
        sourcePage.RedirectUrl = "/listing/2";
        sourcePage.SetPageType(PageType.RedirectToAnotherUrl);
        Listing sourceListing = new() { guid = sourcePage.UriHash };
        Uri destinationUri = new("https://example.com/listing/2");
        Listing destinationListing = new()
        {
            guid = "destination",
            listingStatus = ListingStatus.published
        };
        TestContext context = new();
        context.Store.ListingsByUri[sourcePage.Uri] = sourceListing;
        context.Store.ListingsByUri[destinationUri] = destinationListing;
        context.PageLinks.ResolveResult = destinationUri;

        context.Service.Apply(sourcePage, sourceListing);

        Assert.Equal(destinationUri, context.PageLinks.IndexedUri);
        Assert.Same(sourcePage, context.PageLinks.IndexedSourcePage);
        Assert.Equal(ListingStatus.unpublished, sourceListing.listingStatus);
        var upsert = Assert.Single(context.Store.Upserts);
        Assert.Equal(
            ListingUnpublishDecisionReason.MovedListingDestinationPublished,
            upsert.Decision?.Reason);
    }

    [Fact]
    public void Apply_WhenCanonicalDestinationIsPublished_UsesInspectorAndUnpublishesSource()
    {
        Page sourcePage = CreatePage("https://example.com/listing/1");
        sourcePage.SetPageType(PageType.NotCanonical);
        Listing sourceListing = new() { guid = sourcePage.UriHash };
        Uri destinationUri = new("https://example.com/listing/2");
        Listing destinationListing = new()
        {
            guid = "destination",
            listingStatus = ListingStatus.published
        };
        TestContext context = new();
        context.Store.ListingsByUri[sourcePage.Uri] = sourceListing;
        context.Store.ListingsByUri[destinationUri] = destinationListing;
        context.ContentInspector.CanonicalUri = destinationUri;

        context.Service.Apply(sourcePage, sourceListing);

        Assert.Equal(destinationUri, context.PageLinks.IndexedUri);
        Assert.Equal(ListingStatus.unpublished, sourceListing.listingStatus);
    }
    [Fact]
    public void Apply_WhenPublishedListingCannotBeLoaded_LogsErrorWithoutUpsert()
    {
        Page page = CreatePage("https://example.com/listing/1");
        page.SetPageType(PageType.Listing);
        TestContext context = new();

        context.Service.Apply(page, listing: null);

        Assert.Empty(context.Store.Upserts);
        var error = Assert.Single(context.Logger.Errors);
        Assert.Equal("PageScraper HandlePublishedListing", error.Source);
        Assert.Equal("NewListing is null", error.Message);
    }

    private static Page CreatePage(string uri) =>
        new(
            new Website(new Uri("https://example.com")),
            new Uri(uri));

    private sealed class TestContext
    {
        public TestContext(ListingUnpublishDecision? decision = null)
        {
            Policy.Decision = decision ?? new ListingUnpublishDecision(
                false,
                ListingUnpublishDecisionReason.NoUnpublishEvidence,
                null,
                null,
                0,
                null);
            Service = new ListingLifecycleService(
                Store,
                Cache,
                PageLinks,
                Enricher,
                Policy,
                Logger,
                ContentInspector);
        }

        public RecordingListingStore Store { get; } = new();

        public RecordingNotListingCache Cache { get; } = new();

        public RecordingPageLinkService PageLinks { get; } = new();

        public RecordingListingEnricher Enricher { get; } = new();

        public RecordingUnpublishPolicy Policy { get; } = new();

        public RecordingApplicationLogger Logger { get; } = new();

        public StubPageContentInspector ContentInspector { get; } = new();

        public ListingLifecycleService Service { get; }
    }

    private sealed class RecordingListingStore : IListingStore
    {
        public Dictionary<Uri, Listing> ListingsByUri { get; } = [];

        public List<(Page Page, Listing Listing, ListingUnpublishDecision? Decision)> Upserts { get; } = [];

        public Listing? Get(Page page, bool loadMedia, bool loadSources) =>
            ListingsByUri.GetValueOrDefault(page.Uri);

        public void Upsert(
            Page page,
            Listing listing,
            ListingUnpublishDecision? unpublishDecision = null) =>
            Upserts.Add((page, listing, unpublishDecision));
    }

    private sealed class RecordingNotListingCache : INotListingCacheService
    {
        public int InsertCalls { get; private set; }

        public int InsertAsyncCalls { get; private set; }

        public Page? LastPage { get; private set; }

        public bool Contains(Page page) => false;

        public bool Insert(Page page)
        {
            InsertCalls++;
            LastPage = page;
            return true;
        }

        public Task<bool> InsertAsync(
            Page page,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            InsertAsyncCalls++;
            LastPage = page;
            return Task.FromResult(true);
        }
    }
    private sealed class RecordingPageLinkService : IPageLinkService
    {
        public Uri? ResolveResult { get; set; }

        public Page? IndexedSourcePage { get; private set; }

        public Uri? IndexedUri { get; private set; }

        public Uri? Resolve(Page sourcePage, string? url) => ResolveResult;

        public void Index(Page sourcePage, Uri destinationUri)
        {
            IndexedSourcePage = sourcePage;
            IndexedUri = destinationUri;
        }
    }

    private sealed class RecordingListingEnricher : IListingEnricher
    {
        public int Calls { get; private set; }

        public Page? LastPage { get; private set; }

        public Listing? LastListing { get; private set; }

        public void Enrich(Page page, Listing listing)
        {
            Calls++;
            LastPage = page;
            LastListing = listing;
        }
    }

    private sealed class RecordingUnpublishPolicy : IListingUnpublishPolicy
    {
        public ListingUnpublishDecision Decision { get; set; } = null!;

        public ListingUnpublishDecision Evaluate(Page page) => Decision;
    }

    private sealed class StubPageContentInspector : IPageContentInspector
    {
        public Uri? CanonicalUri { get; set; }
        public bool ContainsMetaRobotsNoIndex(Page page) => false;
        public bool IsNotCanonical(Page page) => page.IsNotCanonical();
        public Uri? GetCanonicalUri(Page page) => CanonicalUri;
        public bool HasIncorrectLanguage(Page page) => false;
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
}
