using landerist_library.Websites;

namespace landerist_library.Application.Websites;

public interface IWebsiteSitemapService
{
    void RefreshSitemap(Website website);
}
