using landerist_library.Application.Persistence;
using landerist_library.Application.Websites;
using landerist_library.Websites;

namespace landerist_library.Infrastructure.WebsiteServices;

public sealed class WebsiteRefreshService : IWebsiteRefreshService
{
    private readonly IWebsiteCatalog _catalog;
    private readonly IWebsitePersistenceService _websitePersistence;
    private readonly IWebsiteNetworkService _network;
    private readonly IWebsiteSitemapService _sitemaps;

    public WebsiteRefreshService(
        IWebsiteCatalog catalog,
        IWebsitePersistenceService websitePersistence,
        IWebsiteNetworkService network,
        IWebsiteSitemapService sitemaps)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(websitePersistence);
        ArgumentNullException.ThrowIfNull(network);
        ArgumentNullException.ThrowIfNull(sitemaps);
        _catalog = catalog;
        _websitePersistence = websitePersistence;
        _network = network;
        _sitemaps = sitemaps;
    }

    public void Refresh()
    {
        DateTime updatedBefore = DateTime.Now.AddDays(-1);
        Refresh(
            _catalog.GetNeedingRobotsTxtUpdate(updatedBefore),
            website => _network.RefreshRobotsTxt(website));
        Refresh(
            _catalog.GetNeedingSitemapUpdate(updatedBefore),
            website => _sitemaps.RefreshSitemap(website));
        Refresh(
            _catalog.GetNeedingIpAddressUpdate(updatedBefore),
            website => _network.RefreshIpAddress(website));
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