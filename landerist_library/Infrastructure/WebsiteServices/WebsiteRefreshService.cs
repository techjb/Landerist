using landerist_library.Application.Persistence;
using landerist_library.Application.Websites;
using landerist_library.Websites;

namespace landerist_library.Infrastructure.WebsiteServices;

public sealed class WebsiteRefreshService : IWebsiteRefreshService
{
    private readonly IWebsiteCatalog _catalog;
    private readonly IWebsitePersistenceService _websitePersistence;
    private readonly IPagePersistenceService _pagePersistence;

    public WebsiteRefreshService(
        IWebsiteCatalog catalog,
        IWebsitePersistenceService websitePersistence,
        IPagePersistenceService pagePersistence)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(websitePersistence);
        ArgumentNullException.ThrowIfNull(pagePersistence);
        _catalog = catalog;
        _websitePersistence = websitePersistence;
        _pagePersistence = pagePersistence;
    }

    public void Refresh()
    {
        DateTime updatedBefore = DateTime.Now.AddDays(-1);
        Refresh(
            _catalog.GetNeedingRobotsTxtUpdate(updatedBefore),
            website => website.SetRobotsTxt());
        Refresh(
            _catalog.GetNeedingSitemapUpdate(updatedBefore),
            website => website.ReadSitemap(_pagePersistence.Insert));
        Refresh(
            _catalog.GetNeedingIpAddressUpdate(updatedBefore),
            website => website.SetIpAddress());
    }

    private void Refresh(
        IEnumerable<Website> websites,
        Action<Website> update)
    {
        Parallel.ForEach(websites, website =>
        {
            try
            {
                update(website);
                _websitePersistence.Update(website);
            }
            finally
            {
                website.Dispose();
            }
        });
    }
}