using landerist_library.Application;
using landerist_library.Application.Listings;
using landerist_library.Application.Logging;
using landerist_library.Application.Persistence;
using landerist_library.Application.Scraping;
using landerist_library.Pages;
using landerist_library.Websites;

namespace landerist_unit_tests;

public sealed class PersistenceServiceTests
{
    [Fact]
    public void PageService_UsesInjectedRepository()
    {
        FakePageRepository repository = new();
        PagePersistenceService service = new(repository);
        Page page = new(new Website(new Uri("https://example.com")), new Uri("https://example.com/listing/1"));

        bool result = service.Insert(page);

        Assert.True(result);
        Assert.Same(page, repository.InsertedPage);
    }

    [Fact]
    public void WebsiteService_UsesInjectedRepository()
    {
        FakeWebsiteRepository repository = new();
        WebsitePersistenceService service = new(repository);
        Website website = new(new Uri("https://example.com"));

        bool result = service.Update(website);

        Assert.True(result);
        Assert.Same(website, repository.UpdatedWebsite);
    }

    [Fact]
    public void LegacyFacades_UseConfiguredApplicationServices()
    {
        FakePageRepository pageRepository = new();
        FakeWebsiteRepository websiteRepository = new();
        LanderistApplication.Configure(new LanderistApplicationServices(
            new PagePersistenceService(pageRepository),
            new WebsitePersistenceService(websiteRepository),
            new NullApplicationLogger(),
            new NullListingLifecycleService(),
            CreateNullPageScraping()));
        Page page = new(
            new Website(new Uri("https://example.com")),
            new Uri("https://example.com/listing/2"));
        Website website = new(new Uri("https://another-example.com"));

        bool pageInserted = Pages.Insert(page);
        bool websiteUpdated = Websites.Update(website);

        Assert.True(pageInserted);
        Assert.True(websiteUpdated);
        Assert.Same(page, pageRepository.InsertedPage);
        Assert.Same(website, websiteRepository.UpdatedWebsite);
    }

    private sealed class FakePageRepository : IPageRepository
    {
        public Page? InsertedPage { get; private set; }
        public bool Insert(Page page) { InsertedPage = page; return true; }
        public bool Update(Page page, out Exception? exception) { exception = null; return true; }
        public bool UpdateNextScrape(string uriHash, DateTime? nextScrape) => true;
        public bool Delete(string uriHash) => true;
        public bool ListingParserInputExistsOnAnotherListing(string host, string uriHash, string? listingParserInputHash) => false;
    }

    private sealed class FakeWebsiteRepository : IWebsiteRepository
    {
        public Website? UpdatedWebsite { get; private set; }
        public bool Insert(Website website) => true;
        public bool Update(Website website) { UpdatedWebsite = website; return true; }
        public bool Delete(string host) => true;
    }

    private sealed class NullApplicationLogger : IApplicationLogger
    {
        public void WriteError(string source, string message)
        {
        }

        public void WriteInfo(string source, string message)
        {
        }
    }

    private sealed class NullListingLifecycleService : IListingLifecycleService
    {
        public void Apply(Page page, landerist_orels.ES.Listing? listing)
        {
        }
    }
    private static PageScrapePipelineServices CreateNullPageScraping() =>
        new(
            new NullPageAcquisitionService(),
            new NullPageContentClassifier(),
            new NullPageIndexingService(),
            new NullPageSchedulingService());

    private sealed class NullPageAcquisitionService : IPageAcquisitionService
    {
        public PageAcquisitionStatus Acquire(Page page, bool useProxy) =>
            PageAcquisitionStatus.DownloadFailed;
    }

    private sealed class NullPageContentClassifier : IPageContentClassifier
    {
        public PageClassificationResult Classify(Page page) =>
            new(null, null, false);
    }

    private sealed class NullPageIndexingService : IPageIndexingService
    {
        public void Index(Page page)
        {
        }
    }

    private sealed class NullPageSchedulingService : IPageSchedulingService
    {
        public void SetNextScrape(Page page)
        {
        }

        public void SetNextScrapeFromNow(Page page)
        {
        }
    }
}
