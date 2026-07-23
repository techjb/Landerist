using landerist_library.Application.Listings;
using landerist_library.Application.Pages;
using landerist_library.Application.Persistence;
using landerist_library.Application.Websites;
using landerist_library.Pages;
using landerist_library.Websites;

namespace landerist_unit_tests;

public sealed class WebsiteDeletionServiceTests
{
    [Fact]
    public void DeleteWithRelations_DeletesListingsThenPagesThenWebsite()
    {
        List<string> events = [];
        Website website = new(new Uri("https://example.com"));
        Page first = new(website, new Uri("https://example.com/listing/1"));
        Page second = new(website, new Uri("https://example.com/listing/2"));
        WebsiteDeletionService service = new(
            new RecordingPageCatalog(first, second),
            new RecordingListingDeletion(events),
            new RecordingPageDeletion(events),
            new RecordingWebsitePersistence(events));

        bool deleted = service.DeleteWithRelations(website);

        Assert.True(deleted);
        Assert.Equal(
            [
                "listing:" + first.UriHash,
                "listing:" + second.UriHash,
                "pages:example.com",
                "website:example.com"
            ],
            events);
    }

    private sealed class RecordingPageCatalog(params Page[] pages) : IPageCatalog
    {
        public Page? GetByHash(string uriHash) =>
            pages.SingleOrDefault(page => page.UriHash == uriHash);

        public IReadOnlyList<Page> GetByWebsite(Website website) => pages;
    }

    private sealed class RecordingListingDeletion(List<string> events) : IListingDeletionService
    {
        public void Delete(Page page) => events.Add("listing:" + page.UriHash);
    }

    private sealed class RecordingPageDeletion(List<string> events) : IPageDeletionService
    {
        public bool DeleteByHost(string host)
        {
            events.Add("pages:" + host);
            return true;
        }
    }

    private sealed class RecordingWebsitePersistence(List<string> events) : IWebsitePersistenceService
    {
        public bool Insert(Website website) => true;

        public bool Update(Website website) => true;

        public bool Delete(Website website)
        {
            events.Add("website:" + website.Host);
            return true;
        }
    }
}
