using landerist_library.Application.Persistence;
using landerist_library.Application.Websites;
using landerist_library.Infrastructure.WebsiteServices;
using landerist_library.Pages;
using landerist_library.Websites;

namespace landerist_unit_tests;

public sealed class WebsiteRefreshServiceTests
{
    [Fact]
    public void Refresh_DelegatesNetworkAndSitemapUpdatesToPorts()
    {
        Website robotsWebsite = new(new Uri("https://robots.example.com"));
        Website ipWebsite = new(new Uri("https://ip.example.com"));
        Website sitemapWebsite = new(new Uri("https://sitemap.example.com"));
        RecordingWebsiteNetworkService network = new();
        RecordingWebsitePersistenceService persistence = new();
        RecordingWebsiteSitemapService sitemaps = new();
        WebsiteRefreshService service = new(
            new StubWebsiteCatalog(robotsWebsite, sitemapWebsite, ipWebsite),
            persistence,
            network,
            sitemaps);

        service.Refresh();

        Assert.Same(robotsWebsite, Assert.Single(network.RobotsWebsites));
        Assert.Same(ipWebsite, Assert.Single(network.IpWebsites));
        Assert.Same(sitemapWebsite, Assert.Single(sitemaps.Websites));
        Assert.Equal(3, persistence.UpdatedWebsites.Count);
    }

    private sealed class RecordingWebsiteSitemapService : IWebsiteSitemapService
    {
        public List<Website> Websites { get; } = [];

        public void RefreshSitemap(Website website) => Websites.Add(website);
    }
    private sealed class RecordingWebsiteNetworkService : IWebsiteNetworkService
    {
        public List<Website> RobotsWebsites { get; } = [];
        public List<Website> IpWebsites { get; } = [];

        public bool RefreshMainUri(Website website) => true;

        public bool RefreshRobotsTxt(Website website)
        {
            RobotsWebsites.Add(website);
            return true;
        }

        public bool RefreshIpAddress(Website website)
        {
            IpWebsites.Add(website);
            return true;
        }
    }

    private sealed class StubWebsiteCatalog(
        Website robotsWebsite,
        Website sitemapWebsite,
        Website ipWebsite) : IWebsiteCatalog
    {
        public IReadOnlyList<Website> GetNeedingRobotsTxtUpdate(DateTime updatedBefore) =>
            [robotsWebsite];

        public IReadOnlyList<Website> GetNeedingSitemapUpdate(DateTime updatedBefore) =>
            [sitemapWebsite];

        public IReadOnlyList<Website> GetNeedingIpAddressUpdate(DateTime updatedBefore) =>
            [ipWebsite];

        public IReadOnlyList<Website> GetAll() => [];
        public IReadOnlySet<string> GetHosts() => new HashSet<string>();
        public Website Get(string host) => throw new NotSupportedException();
        public bool Exists(string host) => false;
        public IReadOnlySet<string> GetUrls() => new HashSet<string>();
        public IReadOnlyList<Website> GetWithSuccessfulStatus() => [];
        public IReadOnlyList<Website> GetWithUnsuccessfulStatus() => [];
        public IReadOnlyList<Website> GetWithoutStatus() => [];
    }

    private sealed class RecordingWebsitePersistenceService : IWebsitePersistenceService
    {
        public List<Website> UpdatedWebsites { get; } = [];

        public bool Insert(Website website) => true;

        public bool Update(Website website)
        {
            UpdatedWebsites.Add(website);
            return true;
        }

        public bool Delete(Website website) => true;
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
