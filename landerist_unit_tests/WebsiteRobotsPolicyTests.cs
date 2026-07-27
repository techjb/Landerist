using landerist_library.Infrastructure.WebsiteServices;
using landerist_library.Websites;

namespace landerist_unit_tests;

public sealed class WebsiteRobotsPolicyTests
{
    private readonly WebsiteRobotsPolicy _policy = new();

    [Fact]
    public void IsAllowed_UsesWebsiteUserAgentAndRules()
    {
        Website website = CreateWebsite("""
            User-agent: *
            Disallow: /private
            """);

        Assert.False(_policy.IsAllowed(
            website,
            new Uri("https://example.com/private/listing")));
        Assert.True(_policy.IsAllowed(
            website,
            new Uri("https://example.com/public/listing")));
    }

    [Fact]
    public void CrawlDelay_UsesWebsiteMaximum()
    {
        Website website = CreateWebsite("""
            User-agent: *
            Crawl-delay: 2
            """);

        Assert.Equal(2, _policy.GetCrawlDelaySeconds(website));
        Assert.False(_policy.IsCrawlDelayTooBig(website));

        Website strictWebsite = new(
            new Uri("https://example.com"),
            new WebsiteRules("TestBot/1.0", 10, 1))
        {
            RobotsTxt = website.RobotsTxt
        };

        Assert.True(_policy.IsCrawlDelayTooBig(strictWebsite));
    }

    [Fact]
    public void GetSitemapUrls_ReturnsUrisWithoutLeakingParserTypes()
    {
        Website website = CreateWebsite("""
            User-agent: *
            Sitemap: https://example.com/sitemap.xml
            Sitemap: https://example.com/listings.xml
            """);

        Assert.Equal(
            [
                new Uri("https://example.com/sitemap.xml"),
                new Uri("https://example.com/listings.xml")
            ],
            _policy.GetSitemapUrls(website));
    }

    private static Website CreateWebsite(string robotsTxt) =>
        new(new Uri("https://example.com"))
        {
            RobotsTxt = robotsTxt
        };
}
