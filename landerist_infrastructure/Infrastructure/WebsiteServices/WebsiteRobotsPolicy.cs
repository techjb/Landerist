using Com.Bekijkhet.RobotsTxt;
using landerist_library.Application.Websites;
using landerist_library.Websites;

namespace landerist_library.Infrastructure.WebsiteServices;

public sealed class WebsiteRobotsPolicy : IWebsiteRobotsPolicy
{
    public bool IsAllowed(Website website, Uri uri)
    {
        ArgumentNullException.ThrowIfNull(website);
        ArgumentNullException.ThrowIfNull(uri);
        Robots? robots = Parse(website);
        return robots is null ||
            robots.IsPathAllowed(website.BrowserUserAgent, uri.PathAndQuery);
    }

    public int GetCrawlDelaySeconds(Website website)
    {
        ArgumentNullException.ThrowIfNull(website);
        Robots? robots = Parse(website);
        return robots is null
            ? 0
            : (int)robots.CrawlDelay(website.BrowserUserAgent) / 1000;
    }

    public bool IsCrawlDelayTooBig(Website website) =>
        GetCrawlDelaySeconds(website) > website.Rules.MaxCrawlDelaySeconds;

    public IReadOnlyList<Uri> GetSitemapUrls(Website website)
    {
        ArgumentNullException.ThrowIfNull(website);
        Robots? robots = Parse(website);
        return robots?.Sitemaps?
            .Select(sitemap => sitemap.Url)
            .ToArray() ?? [];
    }

    private static Robots? Parse(Website website) =>
        string.IsNullOrWhiteSpace(website.RobotsTxt)
            ? null
            : Robots.Load(website.RobotsTxt);
}
