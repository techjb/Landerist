using landerist_library.Websites;
using landerist_library.Infrastructure.WebsiteServices;

namespace landerist_unit_tests;

public sealed class WebsiteRulesTests
{
    private static readonly WebsiteRules CustomRules = new(
        DefaultBrowserUserAgent: "Landerist-Test/1.0",
        HttpClientTimeoutSeconds: 2,
        MaxCrawlDelaySeconds: 1);

    [Fact]
    public void BrowserUserAgent_UsesCustomDefault()
    {
        Website website = new(new Uri("https://example.com"), CustomRules);

        Assert.Equal("Landerist-Test/1.0", website.BrowserUserAgent);
    }

    [Fact]
    public void BrowserUserAgent_PrefersTrimmedWebsiteValue()
    {
        Website website = new(new Uri("https://example.com"), CustomRules)
        {
            UserAgent = "  ExampleBot/2.0  "
        };

        Assert.Equal("ExampleBot/2.0", website.BrowserUserAgent);
    }

    [Fact]
    public void CrawlDelayTooBig_UsesCustomMaximum()
    {
        Website website = new(new Uri("https://example.com"), CustomRules)
        {
            RobotsTxt = """
                User-agent: *
                Crawl-delay: 2
                """
        };

        Assert.True(new WebsiteRobotsPolicy().IsCrawlDelayTooBig(website));
    }
}
