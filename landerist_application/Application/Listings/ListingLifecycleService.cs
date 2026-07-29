using landerist_library.Application.Logging;
using landerist_library.Application.Parsing;
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
    private readonly IPageContentInspector _contentInspector;

    public ListingLifecycleService(
        IListingStore listingStore,
        INotListingCacheService notListingCache,
        IPageLinkService pageLinks,
        IListingEnricher listingEnricher,
        IListingUnpublishPolicy unpublishPolicy,
        IApplicationLogger logger,
        IPageContentInspector contentInspector)
    {
        ArgumentNullException.ThrowIfNull(listingStore);
        ArgumentNullException.ThrowIfNull(notListingCache);
        ArgumentNullException.ThrowIfNull(pageLinks);
        ArgumentNullException.ThrowIfNull(listingEnricher);
        ArgumentNullException.ThrowIfNull(unpublishPolicy);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(contentInspector);

        _listingStore = listingStore;
        _notListingCache = notListingCache;
        _pageLinks = pageLinks;
        _listingEnricher = listingEnricher;
        _unpublishPolicy = unpublishPolicy;
        _logger = logger;
        _contentInspector = contentInspector;
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

    public async Task ApplyAsync(
        Page page,
        Listing? listing,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(page);
        cancellationToken.ThrowIfCancellationRequested();

        if (page.IsListing())
        {
            await PublishAsync(page, listing, cancellationToken)
                .ConfigureAwait(false);
            return;
        }

        if (page.IsNotListingByParser())
        {
            await _notListingCache
                .InsertAsync(page, cancellationToken)
                .ConfigureAwait(false);
        }

        if (await IsMovedListingAsync(page, cancellationToken)
            .ConfigureAwait(false))
        {
            await HandleMovedListingAsync(page, listing, cancellationToken)
                .ConfigureAwait(false);
        }

        ListingUnpublishDecision decision = _unpublishPolicy.Evaluate(page);
        if (decision.ShouldUnpublish)
        {
            await UnpublishAsync(page, listing, decision, cancellationToken)
                .ConfigureAwait(false);
        }
    }
    private async Task<bool> IsMovedListingAsync(
        Page page,
        CancellationToken cancellationToken)
    {
        if (!page.IsNotCanonical() && !page.IsRedirectToAnotherUrl())
        {
            return false;
        }

        return await _listingStore
            .GetAsync(page, loadMedia: false, loadSources: false, cancellationToken)
            .ConfigureAwait(false) is not null;
    }

    private async Task HandleMovedListingAsync(
        Page page,
        Listing? listing,
        CancellationToken cancellationToken)
    {
        Uri? destinationUri = GetDestinationUri(page);
        if (destinationUri is null)
        {
            _logger.WriteError(
                "PageScraper HandleMovedListing",
                "Destination uri is null");
            return;
        }

        _pageLinks.Index(page, destinationUri);
        using var destinationPage = new Page(page.Website, destinationUri);
        Listing? destinationListing = await _listingStore.GetAsync(
            destinationPage,
            loadMedia: false,
            loadSources: false,
            cancellationToken).ConfigureAwait(false);
        if (destinationListing?.listingStatus != ListingStatus.published)
        {
            return;
        }

        await UnpublishAsync(
            page,
            listing,
            CreateMovedListingUnpublishDecision(page),
            cancellationToken).ConfigureAwait(false);
    }

    private async Task PublishAsync(
        Page page,
        Listing? listing,
        CancellationToken cancellationToken)
    {
        listing ??= await _listingStore.GetAsync(
            page,
            loadMedia: true,
            loadSources: true,
            cancellationToken).ConfigureAwait(false);
        if (listing is null)
        {
            _logger.WriteError(
                "PageScraper HandlePublishedListing",
                "NewListing is null");
            return;
        }

        listing.SetPublished();
        _listingEnricher.Enrich(page, listing);
        _listingStore.Upsert(page, listing);
    }

    private async Task UnpublishAsync(
        Page page,
        Listing? listing,
        ListingUnpublishDecision decision,
        CancellationToken cancellationToken)
    {
        listing ??= await _listingStore.GetAsync(
            page,
            loadMedia: true,
            loadSources: true,
            cancellationToken).ConfigureAwait(false);
        if (listing is null)
        {
            _logger.WriteError(
                "PageScraper HandleUnpublishedListing",
                "NewListing is null");
            return;
        }

        listing.SetUnpublished();
        _listingStore.Upsert(page, listing, decision);
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
            ? _contentInspector.GetCanonicalUri(page)
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
