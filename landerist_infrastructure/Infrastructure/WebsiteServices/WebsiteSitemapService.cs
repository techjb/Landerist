using landerist_library.Application.Logging;
using landerist_library.Application.Websites;
using landerist_library.Websites;

namespace landerist_library.Infrastructure.WebsiteServices;

public sealed class WebsiteSitemapService : IWebsiteSitemapService
{
    private readonly bool _indexingEnabled;
    private readonly IWebsiteRobotsPolicy _robots;
    private readonly TimeProvider _timeProvider;
    private readonly IWebsiteSitemapIndexerFactory _indexers;
    private readonly IApplicationLogger _logger;

    public WebsiteSitemapService(
        bool indexingEnabled,
        IWebsiteRobotsPolicy robots,
        TimeProvider timeProvider,
        IWebsiteSitemapIndexerFactory indexers,
        IApplicationLogger logger)
    {
        ArgumentNullException.ThrowIfNull(robots);
        ArgumentNullException.ThrowIfNull(timeProvider);
        ArgumentNullException.ThrowIfNull(indexers);
        ArgumentNullException.ThrowIfNull(logger);
        _indexingEnabled = indexingEnabled;
        _robots = robots;
        _timeProvider = timeProvider;
        _indexers = indexers;
        _logger = logger;
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
            IWebsiteSitemapIndexer indexer = _indexers.Create(website);
            foreach (Uri sitemapUrl in _robots.GetSitemapUrls(website))
            {
                indexedFromRobotsTxt |= indexer.IndexNewPages(sitemapUrl);
            }

            if (!indexedFromRobotsTxt)
            {
                indexer.IndexNewPages(new Uri(website.MainUri, "sitemap.xml"));
            }
        }
        catch (Exception exception)
        {
            _logger.WriteError(
                "Website InsertPagesFromSiteMap",
                $"{website.Host}: {exception}");
        }
    }
}