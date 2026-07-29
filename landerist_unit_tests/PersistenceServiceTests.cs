using landerist_library.Application.Logging;
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
        PagePersistenceService service = new(repository, new RecordingLogger());
        Page page = new(new Website(new Uri("https://example.com")), new Uri("https://example.com/listing/1"));

        bool result = service.Insert(page);

        Assert.True(result);
        Assert.Same(page, repository.InsertedPage);
    }

    [Fact]
    public async Task PageService_UpdateAsync_UsesAsyncRepository()
    {
        FakePageRepository repository = new();
        PagePersistenceService service = new(repository, new RecordingLogger());
        Page page = new(
            new Website(new Uri("https://example.com")),
            new Uri("https://example.com/listing/1"));

        bool result = await service.UpdateAsync(page, CancellationToken.None);

        Assert.True(result);
        Assert.Equal(1, repository.UpdateAsyncCalls);
    }

    [Fact]
    public async Task PageService_UpdateAsync_WhenRepositoryFails_LogsAndReturnsFalse()
    {
        FakePageRepository repository = new()
        {
            UpdateAsyncException = new InvalidOperationException("database failure")
        };
        RecordingLogger logger = new();
        PagePersistenceService service = new(repository, logger);
        Page page = new(
            new Website(new Uri("https://example.com")),
            new Uri("https://example.com/listing/1"));

        bool result = await service.UpdateAsync(page, CancellationToken.None);

        Assert.False(result);
        Assert.Contains("database failure", Assert.Single(logger.Errors));
    }
    [Fact]
    public async Task PageService_UpdateAsync_WhenCancelled_PropagatesWithoutLogging()
    {
        FakePageRepository repository = new();
        RecordingLogger logger = new();
        PagePersistenceService service = new(repository, logger);
        Page page = new(
            new Website(new Uri("https://example.com")),
            new Uri("https://example.com/listing/1"));
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            service.UpdateAsync(page, cancellation.Token));

        Assert.Empty(logger.Errors);
        Assert.Equal(0, repository.UpdateAsyncCalls);
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

    private sealed class RecordingLogger : IApplicationLogger
    {
        public List<string> Errors { get; } = [];

        public void WriteError(string source, string message) => Errors.Add(message);
        public void WriteInfo(string source, string message) { }
    }

    private sealed class FakePageRepository : IPageRepository
    {
        public Page? InsertedPage { get; private set; }
        public int UpdateAsyncCalls { get; private set; }
        public Exception? UpdateAsyncException { get; init; }
        public bool Insert(Page page) { InsertedPage = page; return true; }
        public bool Update(Page page, out Exception? exception) { exception = null; return true; }
        public Task<bool> UpdateAsync(Page page, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            UpdateAsyncCalls++;
            return UpdateAsyncException is null
                ? Task.FromResult(true)
                : Task.FromException<bool>(UpdateAsyncException);
        }
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
