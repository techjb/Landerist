using landerist_library.Infrastructure.Statistics;
using landerist_library.Application.Listings;
using landerist_library.Database;
using landerist_library.Infrastructure.Listings;
using landerist_library.Infrastructure.PageServices;
using landerist_library.Infrastructure.WebsiteServices;
using landerist_library.Infrastructure.Scraping;
using landerist_library.Infrastructure.Sql;
using landerist_library.Application.Statistics;
using landerist_library.Pages;
using landerist_library.Websites;
using landerist_orels.ES;
using System.Data;

namespace landerist_unit_tests;

public sealed class SqlPersistenceAdaptersTests
{
    [Fact]
    public void WebsiteThrottle_UsesInjectedDatabase()
    {
        RecordingDatabase database = new() { QueryBoolResult = true };
        WebsitesThrottle throttle = new(database, new StubWebsiteRobotsPolicy());
        Website website = new(new Uri("https://example.com"));

        bool blocked = throttle.IsBlocked(website);

        Assert.True(blocked);
        Assert.Equal("example.com", database.LastParameters!["Host"]);
        Assert.Contains("BlockUntil", database.LastQuery);
    }

    [Fact]
    public async Task WebsiteThrottle_CleanAsync_UsesAsyncDatabaseExecution()
    {
        RecordingDatabase database = new() { QueryResult = true };
        WebsitesThrottle throttle = new(database, new StubWebsiteRobotsPolicy());

        bool cleaned = await throttle.CleanAsync(CancellationToken.None);

        Assert.True(cleaned);
        Assert.Equal(1, database.QueryAsyncCalls);
        Assert.Contains("DELETE FROM [WEBSITES_THROTTLE]", database.LastQuery);
    }
    [Fact]
    public async Task WebsiteThrottle_AcquisitionAsync_UsesAsyncScalarQueries()
    {
        RecordingDatabase database = new() { QueryBoolResult = true };
        WebsitesThrottle throttle = new(database, new StubWebsiteRobotsPolicy());
        Website website = new(new Uri("https://example.com"));

        bool blocked = await throttle.IsBlockedAsync(website, CancellationToken.None);
        bool acquired = await throttle.BlockAsync(website, CancellationToken.None);

        Assert.True(blocked);
        Assert.True(acquired);
        Assert.Equal(2, database.QueryBoolAsyncCalls);
        Assert.Equal("example.com", database.LastParameters!["Host"]);
        Assert.Contains("SELECT @Acquired", database.LastQuery);
    }
    [Fact]
    public async Task WebsiteThrottle_ReportsAsync_UseAsyncDatabaseExecution()
    {
        RecordingDatabase database = new() { QueryResult = true };
        WebsitesThrottle throttle = new(database, new StubWebsiteRobotsPolicy());
        Website website = new(new Uri("https://example.com"));

        bool forbidden = await throttle.ReportForbiddenAsync(
            website,
            CancellationToken.None);
        bool success = await throttle.ReportSuccessAsync(
            website,
            CancellationToken.None);

        Assert.True(forbidden);
        Assert.True(success);
        Assert.Equal(2, database.QueryAsyncCalls);
        Assert.Equal("example.com", database.LastParameters!["Host"]);
        Assert.Contains("SuccessCounterAfterForbidden", database.LastQuery);
    }
    [Fact]
    public void NotListingCache_WhenEnabled_InsertsThroughInjectedDatabase()
    {
        RecordingDatabase database = new() { QueryResult = true };
        SqlNotListingCacheService cache = new(database, enabled: true);
        Page page = CreatePage();
        page.ListingParserInputHash = "content-hash";

        bool inserted = cache.Insert(page);

        Assert.True(inserted);
        Assert.Equal("example.com", database.LastParameters!["Host"]);
        Assert.Equal("content-hash", database.LastParameters["ListingParserInputHash"]);
    }

    [Fact]
    public async Task NotListingCache_InsertAsync_UsesAsyncDatabaseExecution()
    {
        RecordingDatabase database = new() { QueryResult = true };
        SqlNotListingCacheService cache = new(database, enabled: true);
        Page page = CreatePage();
        page.ListingParserInputHash = "content-hash";

        bool inserted = await cache.InsertAsync(page, CancellationToken.None);

        Assert.True(inserted);
        Assert.Equal(1, database.QueryAsyncCalls);
        Assert.Equal("content-hash", database.LastParameters!["ListingParserInputHash"]);
    }
    [Fact]
    public void NotListingCache_WhenDisabled_DoesNotAccessDatabase()
    {
        RecordingDatabase database = new();
        SqlNotListingCacheService cache = new(database, enabled: false);
        Page page = CreatePage();
        page.ListingParserInputHash = "content-hash";

        Assert.False(cache.Insert(page));
        Assert.Empty(database.Calls);
    }

    [Fact]
    public void NotListingCache_Clean_RemovesExpiredEntriesThroughInjectedDatabase()
    {
        RecordingDatabase database = new() { QueryResult = true };
        SqlNotListingCacheService cache = new(database, enabled: true);

        bool cleaned = cache.Clean();

        Assert.True(cleaned);
        Assert.Contains("DATEADD(DAY, -30", database.LastQuery);
        Assert.Contains("NOT_LISTINGS_CACHE", database.LastQuery);
    }
    [Fact]
    public async Task ListingQueryRepository_GetAsync_UsesAsyncTableExecution()
    {
        RecordingDatabase database = new();
        ListingQueryRepository repository = new(database);

        DataTable rows = await repository.GetListingAsync(
            "listing-guid",
            CancellationToken.None);

        Assert.Same(database.TableResult, rows);
        Assert.Equal(1, database.QueryTableAsyncCalls);
        Assert.Equal("listing-guid", database.LastParameters!["Guid"]);
    }
    [Fact]
    public async Task PageRepository_UpdateAsync_UsesAsyncDatabaseExecution()
    {
        RecordingDatabase database = new() { QueryResult = true };
        PageRepository repository = new(database);
        Page page = CreatePage();

        bool updated = await repository.UpdateAsync(page, CancellationToken.None);

        Assert.True(updated);
        Assert.Equal(1, database.QueryAsyncCalls);
        Assert.Contains("UPDATE [PAGES] SET", database.LastQuery);
        Assert.Equal(page.UriHash, database.LastParameters!["UriHash"]);
    }
    [Fact]
    public void PageSelection_MapsRowsAndCleansLocksThroughInjectedDatabase()
    {
        RecordingDatabase database = new();
        AddPageRow(database.TableResult);
        SqlPageSelectionRepository repository = new(
            database,
            "test-machine",
            new PageQueryOptions("test-machine", 10));

        IReadOnlyList<Page> pages = repository.GetScrapePages(5);

        Page page = Assert.Single(pages);
        Assert.Equal("expected-hash", page.UriHash);
        Assert.Equal(5, int.Parse(database.LastQuery.Split("SELECT TOP ")[1].Split(' ')[0]));

        repository.CleanLockedPages();
        Assert.Equal("test-machine", database.LastParameters!["LockedBy"]);
    }

    [Fact]
    public void ScrapePageSource_LoadsExistingPageWithoutStaticFacade()
    {
        RecordingDatabase database = new();
        AddPageRow(database.TableResult);
        RecordingListingStore listings = new();
        SqlScrapePageSource source = new(database, listings);

        Page page = source.LoadOrCreate(new Uri("https://example.com/listing/1"));

        Assert.Equal("expected-hash", page.UriHash);
        Assert.Equal("example.com", page.Website.Host);
    }

    [Fact]
    public void ScrapeMetrics_WriteGlobalAndHostCountersThroughInjectedDatabase()
    {
        RecordingDatabase database = new() { QueryResult = true };
        SqlScrapeMetrics metrics = new(database);

        metrics.RecordPageNotModified(CreatePage());

        Assert.Equal(2, database.Calls.Count);
        Assert.Equal("PageNotModified", database.Calls[0].Parameters!["Key"]);
        Assert.Equal("example.com", database.Calls[1].Parameters!["Host"]);
    }

    [Fact]
    public void GlobalStatistics_ReadsThroughInjectedRepository()
    {
        RecordingDatabase database = new();
        GlobalStatistics statistics = new(new GlobalStatisticsRepository(database), persistenceEnabled: true);

        statistics.GetLatestStatistics(StatisticsKey.Pages.ToString(), 15);

        Assert.Contains("GLOBAL_STATISTICS", database.LastQuery);
        Assert.Equal("Pages", database.LastParameters!["Key"]);
        Assert.Equal(15, database.LastParameters["Top"]);
    }

    [Fact]
    public void HostStatistics_SnapshotsHostsFromInjectedRepositories()
    {
        RecordingDatabase database = new();
        database.HashSetResult.Add("example.com");
        HostStatistics statistics = new(
            new HostStatisticsRepository(database),
            new SqlWebsiteCatalog(new WebsiteQueryRepository(database)),
            persistenceEnabled: true);

        statistics.TakeSnapshots();

        Assert.Contains(database.Calls, call =>
            call.Parameters?.TryGetValue("Host", out var host) == true &&
            Equals(host, "example.com"));
        Assert.DoesNotContain(database.Calls, call => call.Query.Contains("SELECT *", StringComparison.Ordinal));
    }
    [Fact]
    public void PageWaitingStatus_SelectsAndMapsThroughInjectedRepository()
    {
        RecordingDatabase database = new();
        AddPageRow(database.TableResult);
        SqlPageWaitingStatusService waitingStatus = new(new PageMaintenanceRepository(database));

        List<Page> pages = waitingStatus.SelectAIRequest(
            5,
            WaitingStatus.readed_by_localai,
            7000,
            isMaxTokenCount: true);

        Assert.Single(pages);
        Assert.Equal(5, database.LastParameters!["TopRows"]);
        Assert.Equal("waiting_ai_request", database.LastParameters["WaitingStatusFrom"]);
        Assert.Equal("readed_by_localai", database.LastParameters["WaitingStatusTo"]);
    }

    [Fact]
    public void WebsiteCatalog_MapsWebsitesThroughInjectedRepository()
    {
        RecordingDatabase database = new();
        AddPageRow(database.TableResult);
        SqlWebsiteCatalog catalog = new(new WebsiteQueryRepository(database));

        IReadOnlyList<Website> websites = catalog.GetAll();

        Website website = Assert.Single(websites);
        Assert.Equal("example.com", website.Host);
        Assert.Contains("FROM [WEBSITES]", database.LastQuery);
    }

    [Fact]
    public void PageQueryService_ReturnsMappedDomainPages()
    {
        RecordingDatabase database = new();
        AddPageRow(database.TableResult);
        SqlPageQueryService queries = new(new PageQueryRepository(database));

        IReadOnlyList<Page> pages = queries.GetByType(PageType.Listing);

        Page page = Assert.Single(pages);
        Assert.Equal("expected-hash", page.UriHash);
        Assert.Equal(PageType.Listing.ToString(), database.LastParameters!["PageType"]);
    }

    [Fact]
    public void WebsiteCatalog_ReturnsMappedMaintenanceCandidates()
    {
        RecordingDatabase database = new();
        AddPageRow(database.TableResult);
        SqlWebsiteCatalog catalog = new(new WebsiteQueryRepository(database));
        DateTime updatedBefore = new(2026, 1, 15);

        IReadOnlyList<Website> websites =
            catalog.GetNeedingRobotsTxtUpdate(updatedBefore);

        Assert.Single(websites);
        Assert.Equal(updatedBefore, database.LastParameters!["RobotsTxtUpdatedSpecialRules"]);
    }

    [Fact]
    public void MaintenanceServices_DelegateCommandsToRepositories()
    {
        RecordingDatabase database = new() { QueryResult = true };
        SqlPageMaintenanceService pages = new(new PageMaintenanceRepository(database));
        SqlWebsiteMaintenanceService websites = new(new WebsiteQueryRepository(database));

        Assert.True(pages.RemoveListingParserInputHash(PageType.Listing));
        Assert.Equal(PageType.Listing.ToString(), database.LastParameters!["PageType"]);
        Assert.True(websites.DeleteAll());
        Assert.Contains("DELETE FROM [WEBSITES]", database.LastQuery);
    }
    [Fact]
    public void PageCatalog_MapsPageByHashThroughInjectedRepository()
    {
        RecordingDatabase database = new();
        AddPageRow(database.TableResult);
        SqlPageCatalog catalog = new(new PageQueryRepository(database));

        Page? page = catalog.GetByHash("expected-hash");

        Assert.NotNull(page);
        Assert.Equal("expected-hash", page.UriHash);
        Assert.Equal("example.com", page.Website.Host);
        Assert.Equal("expected-hash", database.LastParameters!["UriHash"]);
    }

    [Fact]
    public void PageCatalog_MapsPagesForWebsiteThroughInjectedRepository()
    {
        RecordingDatabase database = new();
        AddPageRow(database.TableResult);
        SqlPageCatalog catalog = new(new PageQueryRepository(database));
        Website website = new(new Uri("https://example.com"));

        IReadOnlyList<Page> pages = catalog.GetByWebsite(website);

        Page page = Assert.Single(pages);
        Assert.Equal("expected-hash", page.UriHash);
        Assert.Same(website, page.Website);
        Assert.Equal("example.com", database.LastParameters!["Host"]);
    }

    [Fact]
    public void PageDeletion_DeletesPagesForHostThroughInjectedRepository()
    {
        RecordingDatabase database = new() { QueryResult = true };
        SqlPageDeletionService deletion = new(new PageMaintenanceRepository(database));

        bool deleted = deletion.DeleteByHost("example.com");

        Assert.True(deleted);
        Assert.Contains("DELETE FROM", database.LastQuery);
        Assert.Equal("example.com", database.LastParameters!["Host"]);
    }

    [Fact]
    public void WebsiteMetrics_CountsThroughInjectedRepositories()
    {
        RecordingDatabase database = new() { QueryIntResult = 12 };
        WebsiteMetricsService metrics = new(
            new WebsitePageMetricsRepository(database),
            new ListingStatisticsRepository(database),
            maximumPagesPerWebsite: 100);
        Website website = new(new Uri("https://example.com"));

        int count = metrics.CountPublishedListings(website);

        Assert.Equal(12, count);
        Assert.Equal("example.com", database.LastParameters!["Host"]);
        Assert.Equal("published", database.LastParameters["ListingStatus"]);
    }
    [Fact]
    public void PageScheduling_UsesInjectedListingStore()
    {
        Page page = CreatePage();
        page.SetPageType(PageType.Listing);
        RecordingListingStore listings = new()
        {
            Result = new Listing
            {
                guid = page.UriHash,
                listingStatus = ListingStatus.published
            }
        };
        SqlPageSchedulingService scheduling = new(listings);

        scheduling.SetNextScrapeFromNow(page);

        Assert.NotNull(page.NextScrape);
        Assert.Equal(1, listings.GetCalls);
    }

    private static Page CreatePage()
    {
        Website website = new(new Uri("https://example.com"));
        return new Page(website, new Uri("https://example.com/listing/1"));
    }

    private static void AddPageRow(DataTable table)
    {
        table.Columns.Add("MainUri", typeof(string));
        table.Columns.Add("Host", typeof(string));
        table.Columns.Add("LanguageCode", typeof(string));
        table.Columns.Add("CountryCode", typeof(string));
        table.Columns.Add("Uri", typeof(string));
        table.Columns.Add("UriHash", typeof(string));
        table.Columns.Add("Inserted", typeof(DateTime));
        table.Rows.Add(
            "https://example.com",
            "example.com",
            "es",
            "ES",
            "https://example.com/listing/1",
            "expected-hash",
            new DateTime(2026, 1, 1));
    }

    private sealed class RecordingListingStore : IListingStore
    {
        public Listing? Result { get; init; }
        public int GetCalls { get; private set; }

        public Listing? Get(Page page, bool loadMedia, bool loadSources)
        {
            GetCalls++;
            return Result;
        }

        public Task<Listing?> GetAsync(
            Page page,
            bool loadMedia,
            bool loadSources,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            GetCalls++;
            return Task.FromResult(Result);
        }
        public void Upsert(
            Page page,
            Listing listing,
            ListingUnpublishDecision? unpublishDecision = null)
        {
        }
    }
}
