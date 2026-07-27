using landerist_library.Websites;

namespace landerist_library.Application.Websites;

public interface IWebsiteSitemapIndexer
{
    bool IndexNewPages(Uri sitemapUri);
}

public interface IWebsiteSitemapIndexerFactory
{
    IWebsiteSitemapIndexer Create(Website website);
}