using landerist_library.Application.Listings;
using landerist_library.Application.Pages;
using landerist_library.Application.Persistence;
using landerist_library.Websites;

namespace landerist_library.Application.Websites;

public sealed class WebsiteDeletionService : IWebsiteDeletionService
{
    private readonly IPageCatalog _pages;
    private readonly IListingDeletionService _listings;
    private readonly IPageDeletionService _pageDeletion;
    private readonly IWebsitePersistenceService _websites;

    public WebsiteDeletionService(
        IPageCatalog pages,
        IListingDeletionService listings,
        IPageDeletionService pageDeletion,
        IWebsitePersistenceService websites)
    {
        ArgumentNullException.ThrowIfNull(pages);
        ArgumentNullException.ThrowIfNull(listings);
        ArgumentNullException.ThrowIfNull(pageDeletion);
        ArgumentNullException.ThrowIfNull(websites);
        _pages = pages;
        _listings = listings;
        _pageDeletion = pageDeletion;
        _websites = websites;
    }

    public bool DeleteWithRelations(Website website)
    {
        ArgumentNullException.ThrowIfNull(website);
        foreach (var page in _pages.GetByWebsite(website))
        {
            _listings.Delete(page);
        }

        _pageDeletion.DeleteByHost(website.Host);
        return _websites.Delete(website);
    }
}
