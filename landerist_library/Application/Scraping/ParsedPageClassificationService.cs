using landerist_library.Application.Listings;
using landerist_library.Application.Persistence;
using landerist_library.Pages;
using landerist_orels.ES;

namespace landerist_library.Application.Scraping;

public sealed class ParsedPageClassificationService : IParsedPageClassificationService
{
    private readonly IPagePersistenceService _pagePersistence;
    private readonly IListingLifecycleService _listingLifecycle;

    public ParsedPageClassificationService(
        IPagePersistenceService pagePersistence,
        IListingLifecycleService listingLifecycle)
    {
        ArgumentNullException.ThrowIfNull(pagePersistence);
        ArgumentNullException.ThrowIfNull(listingLifecycle);
        _pagePersistence = pagePersistence;
        _listingLifecycle = listingLifecycle;
    }

    public bool Apply(Page page, PageType pageType, Listing? listing)
    {
        ArgumentNullException.ThrowIfNull(page);
        if (pageType == PageType.MayBeListing)
        {
            return false;
        }

        page.RemoveWaitingStatus();
        page.SetResponseBodyFromZipped();
        page.SetPageType(pageType);
        _listingLifecycle.Apply(page, listing);
        page.RemoveResponseBodyZipped();
        return _pagePersistence.Update(page);
    }
}
