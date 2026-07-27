using landerist_library.Application.Persistence;
using landerist_library.Application.Websites;
using landerist_library.Infrastructure.WebsiteServices;
using landerist_library.Pages;
using landerist_library.Websites;

namespace landerist_unit_tests;

public sealed class WebsiteSitemapServiceTests
{
    [Fact]
    public void RefreshSitemap_WhenIndexingIsDisabled_OnlyRecordsUpdateTime()
    {
        DateTimeOffset now = new(2026, 7, 27, 12, 30, 0, TimeSpan.Zero);
        Website website = new(new Uri("https://example.com"));
        WebsiteSitemapService service = new(
            indexingEnabled: false,
            new StubPagePersistenceService(),
            new StubWebsiteMetricsService(),
            new FixedTimeProvider(now));

        service.RefreshSitemap(website);

        Assert.Equal(now.DateTime, website.SitemapUpdated);
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
    }

    private sealed class StubPagePersistenceService : IPagePersistenceService
    {
        public bool Insert(Page page) => true;
        public bool Update(Page page) => true;
        public bool UpdateNextScrape(Page page) => true;
        public bool Delete(Page page) => true;
        public bool ListingParserInputExistsOnAnotherListing(Page page) => false;
    }

    private sealed class StubWebsiteMetricsService : IWebsiteMetricsService
    {
        public int CountPages(Website website) => 0;
        public bool HasAchievedMaximumPages(Website website) => false;
    }
}
