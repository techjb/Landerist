using landerist_library.Application;
using landerist_library.Application.Persistence;
using landerist_library.Application.Websites;
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
            new FakeWebsiteDeletionService()));
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

    private sealed class FakeWebsiteDeletionService : IWebsiteDeletionService
    {
        public bool DeleteWithRelations(Website website) => true;
    }

    private sealed class FakeWebsiteRepository : IWebsiteRepository
    {
        public Website? UpdatedWebsite { get; private set; }
        public bool Insert(Website website) => true;
        public bool Update(Website website) { UpdatedWebsite = website; return true; }
        public bool Delete(string host) => true;
    }

}
