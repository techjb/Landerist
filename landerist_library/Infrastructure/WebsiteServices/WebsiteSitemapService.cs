using landerist_library.Application.Persistence;
using landerist_library.Application.Websites;
using landerist_library.Infrastructure.Indexing;
using landerist_library.Infrastructure.Http;
using landerist_library.Logs;
using landerist_library.Websites;

namespace landerist_library.Infrastructure.WebsiteServices;

public sealed class WebsiteSitemapService : IWebsiteSitemapService
{
    private readonly bool _indexingEnabled;
    private readonly IPagePersistenceService _pagePersistence;
    private readonly IWebsiteMetricsService _metrics;
    private readonly TimeProvider _timeProvider;
    private readonly IWebsiteRobotsPolicy _robots;
    private readonly HttpClientTransportFactory _httpClients;

    public WebsiteSitemapService(
        bool indexingEnabled,
        IPagePersistenceService pagePersistence,
        IWebsiteMetricsService metrics,
        IWebsiteRobotsPolicy robots,
        TimeProvider timeProvider,
        HttpClientTransportFactory httpClients)
    {
        ArgumentNullException.ThrowIfNull(pagePersistence);
        ArgumentNullException.ThrowIfNull(metrics);
        ArgumentNullException.ThrowIfNull(robots);
        ArgumentNullException.ThrowIfNull(timeProvider);
        ArgumentNullException.ThrowIfNull(httpClients);
        _indexingEnabled = indexingEnabled;
        _pagePersistence = pagePersistence;
        _metrics = metrics;
        _timeProvider = timeProvider;
        _robots = robots;
        _httpClients = httpClients;
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
                _robots,
                _httpClients,
                _pagePersistence.Insert,
                _metrics.HasAchievedMaximumPages);
            IReadOnlyList<Uri> sitemapUrls = _robots.GetSitemapUrls(website);
            foreach (Uri sitemapUrl in sitemapUrls)
            {
                indexedFromRobotsTxt |= sitemapIndexer.IndexNewPages(sitemapUrl);
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
