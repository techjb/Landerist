using landerist_library.Application.Persistence;
using landerist_library.Application.Websites;
using landerist_library.Infrastructure.Http;
using landerist_library.Websites;

namespace landerist_library.Infrastructure.Indexing;

public sealed class LegacyWebsiteSitemapIndexerFactory : IWebsiteSitemapIndexerFactory
{
    private readonly IWebsiteRobotsPolicy _robots;
    private readonly IHttpClientTransportFactory _httpClients;
    private readonly IPagePersistenceService _pages;
    private readonly IWebsiteMetricsService _metrics;

    public LegacyWebsiteSitemapIndexerFactory(
        IWebsiteRobotsPolicy robots,
        IHttpClientTransportFactory httpClients,
        IPagePersistenceService pages,
        IWebsiteMetricsService metrics)
    {
        ArgumentNullException.ThrowIfNull(robots);
        ArgumentNullException.ThrowIfNull(httpClients);
        ArgumentNullException.ThrowIfNull(pages);
        ArgumentNullException.ThrowIfNull(metrics);
        _robots = robots;
        _httpClients = httpClients;
        _pages = pages;
        _metrics = metrics;
    }

    public IWebsiteSitemapIndexer Create(Website website)
    {
        ArgumentNullException.ThrowIfNull(website);
        return new Adapter(new SitemapIndexer(
            website,
            _robots,
            _httpClients,
            _pages.Insert,
            _metrics.HasAchievedMaximumPages));
    }

    private sealed class Adapter(SitemapIndexer indexer) : IWebsiteSitemapIndexer
    {
        public bool IndexNewPages(Uri sitemapUri) =>
            indexer.IndexNewPages(sitemapUri);
    }
}