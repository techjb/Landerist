using landerist_library.Websites;

namespace landerist_library.Application.Websites;

public interface IWebsiteRobotsPolicy
{
    bool IsAllowed(Website website, Uri uri);

    int GetCrawlDelaySeconds(Website website);

    bool IsCrawlDelayTooBig(Website website);

    IReadOnlyList<Uri> GetSitemapUrls(Website website);
}
