using landerist_library.Application.Persistence;
using landerist_library.Application.Websites;
using landerist_library.Index;
using landerist_library.Logs;
using landerist_library.Websites;

namespace landerist_library.Infrastructure.WebsiteServices;

public sealed class WebsiteSitemapService : IWebsiteSitemapService
{
    private readonly bool _indexingEnabled;
    private readonly IPagePersistenceService _pagePersistence;
    private readonly IWebsiteMetricsService _metrics;
    private readonly TimeProvider _timeProvider;

    public WebsiteSitemapService(
        bool indexingEnabled,
        IPagePersistenceService pagePersistence,
        IWebsiteMetricsService metrics,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(pagePersistence);
        ArgumentNullException.ThrowIfNull(metrics);
        ArgumentNullException.ThrowIfNull(timeProvider);
        _indexingEnabled = indexingEnabled;
        _pagePersistence = pagePersistence;
        _metrics = metrics;
        _timeProvider = timeProvider;
    }

    public void RefreshSitemap(Website website)
    {
        ArgumentNullException.ThrowIfNull(website);
        website.SitemapUpdated = _timeProvider.GetLocalNow().DateTime;

        if (!_indexingEnabled)
        {
            return;
        }

        try
        {
            bool indexedFromRobotsTxt = false;
            SitemapIndexer sitemapIndexer = new(
                website,
                _pagePersistence.Insert,
                _metrics.HasAchievedMaximumPages);
            List<Com.Bekijkhet.RobotsTxt.Sitemap>? sitemaps =
                website.GetSiteMapsFromRobotsTxt();

            if (sitemaps is { Count: > 0 })
            {
                indexedFromRobotsTxt = sitemapIndexer.IndexNewPages(sitemaps);
            }

            if (!indexedFromRobotsTxt)
            {
                sitemapIndexer.IndexNewPages(new Uri(website.MainUri, "sitemap.xml"));
            }
        }
        catch (Exception exception)
        {
            Log.WriteError("Website InsertPagesFromSiteMap", website.Host, exception);
        }
    }
}
