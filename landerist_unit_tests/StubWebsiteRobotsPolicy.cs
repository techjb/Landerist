using landerist_library.Application.Websites;
using landerist_library.Websites;

namespace landerist_unit_tests;

internal sealed class StubWebsiteRobotsPolicy : IWebsiteRobotsPolicy
{
    public bool IsAllowed(Website website, Uri uri) => true;

    public int GetCrawlDelaySeconds(Website website) => 0;

    public bool IsCrawlDelayTooBig(Website website) => false;

    public IReadOnlyList<Uri> GetSitemapUrls(Website website) => [];
}
