using landerist_library.Application.Logging;
using landerist_library.Pages;
using landerist_orels.ES;

namespace landerist_library.Application.Listings;

public sealed class ListingLifecycleService : IListingLifecycleService
{
    private readonly IListingStore _listingStore;
    private readonly INotListingCacheService _notListingCache;
    private readonly IPageLinkService _pageLinks;
    private readonly IListingEnricher _listingEnricher;
    private readonly IListingUnpublishPolicy _unpublishPolicy;
    private readonly IApplicationLogger _logger;

    public ListingLifecycleService(
        IListingStore listingStore,
        INotListingCacheService notListingCache,
        IPageLinkService pageLinks,
        IListingEnricher listingEnricher,
        IListingUnpublishPolicy unpublishPolicy,
        IApplicationLogger logger)
    {
        ArgumentNullException.ThrowIfNull(listingStore);
        ArgumentNullException.ThrowIfNull(notListingCache);
        ArgumentNullException.ThrowIfNull(pageLinks);
        ArgumentNullException.ThrowIfNull(listingEnricher);
        ArgumentNullException.ThrowIfNull(unpublishPolicy);
        ArgumentNullException.ThrowIfNull(logger);

        _listingStore = listingStore;
        _notListingCache = notListingCache;
        _pageLinks = pageLinks;
        _listingEnricher = listingEnricher;
        _unpublishPolicy = unpublishPolicy;
        _logger = logger;
    }

    public void Apply(Page page, Listing? listing)
    {
        ArgumentNullException.ThrowIfNull(page);

        if (page.IsListing())
        {
            Publish(page, listing);
            return;
        }

        if (page.IsNotListingByParser())
        {
            _notListingCache.Insert(page);
        }

        if (IsMovedListing(page))
        {
            HandleMovedListing(page, listing);
        }

        var unpublishDecision = _unpublishPolicy.Evaluate(page);
        if (unpublishDecision.ShouldUnpublish)
        {
            Unpublish(page, listing, unpublishDecision);
        }
    }

    private bool IsMovedListing(Page page)
    {
        if (!page.IsNotCanonical() && !page.IsRedirectToAnotherUrl())
        {
            return false;
        }

        return _listingStore.Get(page, loadMedia: false, loadSources: false) is not null;
    }

    private void HandleMovedListing(Page page, Listing? listing)
    {
        var destinationUri = GetDestinationUri(page);
        if (destinationUri is null)
        {
            _logger.WriteError("PageScraper HandleMovedListing", "Destination uri is null");
            return;
        }

        _pageLinks.Index(page, destinationUri);

        using var destinationPage = new Page(page.Website, destinationUri);
        var destinationListing = _listingStore.Get(destinationPage, loadMedia: false, loadSources: false);
        if (destinationListing?.listingStatus != ListingStatus.published)
        {
            return;
        }

        Unpublish(page, listing, CreateMovedListingUnpublishDecision(page));
    }

    private Uri? GetDestinationUri(Page page)
    {
        if (page.IsRedirectToAnotherUrl())
        {
            return _pageLinks.Resolve(page, page.RedirectUrl);
        }

        return page.IsNotCanonical()
            ? page.GetCanonicalUri()
            : null;
    }

    private void Publish(Page page, Listing? listing)
    {
        listing ??= _listingStore.Get(page, loadMedia: true, loadSources: true);
        if (listing is null)
        {
            _logger.WriteError("PageScraper HandlePublishedListing", "NewListing is null");
            return;
        }

        listing.SetPublished();
        _listingEnricher.Enrich(page, listing);
        _listingStore.Upsert(page, listing);
    }

    private void Unpublish(
        Page page,
        Listing? listing,
        ListingUnpublishDecision unpublishDecision)
    {
        listing ??= _listingStore.Get(page, loadMedia: true, loadSources: true);
        if (listing is null)
        {
            _logger.WriteError("PageScraper HandleUnpublishedListing", "NewListing is null");
            return;
        }

        listing.SetUnpublished();
        _listingStore.Upsert(page, listing, unpublishDecision);
    }

    private static ListingUnpublishDecision CreateMovedListingUnpublishDecision(Page page) =>
        new(
            true,
            ListingUnpublishDecisionReason.MovedListingDestinationPublished,
            page.PageType,
            page.HttpStatusCode,
            page.PageTypeCounter ?? 0,
            null);
}
