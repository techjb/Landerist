using landerist_library.Infrastructure.Sql;
using landerist_library.Infrastructure.Sql.Mapping;
using landerist_library.Pages;
using landerist_library.Websites;
using landerist_orels.ES;
using System.Data;

namespace landerist_unit_tests;

public sealed class PageRepositoryTests
{
    [Fact]
    public void Mapper_RestoresPageWithoutEntityDatabaseAccess()
    {
        DataTable table = CreatePageTable();
        table.Rows.Add("example.com", "https://example.com/listing/1", "expected-hash", new DateTime(2026, 1, 1));
        Website website = new(new Uri("https://example.com"));

        Page result = PageDataMapper.Map(table.Rows[0], website);

        Assert.Equal("expected-hash", result.UriHash);
        Assert.Equal(website, result.Website);
        Assert.Equal(new DateTime(2026, 1, 1), result.Inserted);
    }

    [Fact]
    public void Insert_MapsEntityAndDelegatesResult()
    {
        RecordingDatabase database = new() { QueryResult = true };
        PageRepository repository = new(database);
        Page page = CreatePage();

        bool result = repository.Insert(page);

        Assert.True(result);
        Assert.Equal(page.UriHash, database.LastParameters!["UriHash"]);
        Assert.Equal(page.Uri.ToString(), database.LastParameters["Uri"]);
        Assert.Contains("INSERT INTO", database.LastQuery);
    }

    [Fact]
    public void Update_PropagatesDatabaseException()
    {
        InvalidOperationException expectedException = new("Expected failure");
        RecordingDatabase database = new() { QueryResult = false, QueryException = expectedException };
        PageRepository repository = new(database);

        bool result = repository.Update(CreatePage(), out Exception? exception);

        Assert.False(result);
        Assert.Same(expectedException, exception);
        Assert.Contains("UPDATE", database.LastQuery);
    }

    [Fact]
    public void CountPages_UsesInjectedDatabase()
    {
        RecordingDatabase database = new() { QueryIntResult = 42 };
        PageQueryRepository repository = new(database);

        int result = repository.CountPages();

        Assert.Equal(42, result);
        Assert.Contains("COUNT(*)", database.LastQuery);
    }

    [Fact]
    public void SelectWaitingStatus_ParameterizesLimits()
    {
        RecordingDatabase database = new();
        PageMaintenanceRepository repository = new(database);

        repository.SelectWaitingStatus(5, WaitingStatus.waiting_ai_request, WaitingStatus.waiting_ai_response, 100, isMaxTokenCount: true);

        Assert.Contains("TOP (@TopRows)", database.LastQuery);
        Assert.Contains("[TokenCount] <= @TokenCount", database.LastQuery);
        Assert.Equal(5, database.LastParameters!["TopRows"]);
        Assert.Equal(100, database.LastParameters["TokenCount"]);
    }

    [Fact]
    public void GroupByPageType_DelegatesListingStatus()
    {
        RecordingDatabase database = new();
        database.DictionaryResult["Listing"] = 3;
        PageStatisticsRepository repository = new(database);

        Dictionary<string, object?> result = repository.GroupByPageType(ListingStatus.published);

        Assert.Same(database.DictionaryResult, result);
        Assert.Equal("published", database.LastParameters!["ListingStatus"]);
    }

    [Fact]
    public void GetPagesWithProhibitedUris_ParameterizesFragments()
    {
        RecordingDatabase database = new();
        PageQueryRepository repository = new(database);
        const string fragment = "unsafe'fragment";

        repository.GetPagesWithProhibitedUris([fragment]);

        Assert.DoesNotContain(fragment, database.LastQuery);
        Assert.Contains("@UriFragment0", database.LastQuery);
        Assert.Equal("%" + fragment + "%", database.LastParameters!["UriFragment0"]);
    }

    [Fact]
    public void GetScrapePages_RejectsInvalidLimitBeforeExecutingSql()
    {
        RecordingDatabase database = new();
        PageQueryRepository repository = new(database);

        Assert.Throws<ArgumentOutOfRangeException>(() => repository.GetScrapePages(0));
        Assert.Empty(database.LastQuery);
    }

    [Fact]
    public void ListingParserInputExists_UsesInjectedDatabase()
    {
        RecordingDatabase database = new() { QueryExistsResult = true };
        PageRepository repository = new(database);

        bool result = repository.ListingParserInputExistsOnAnotherListing("example.com", "current-hash", "content-hash");

        Assert.True(result);
        Assert.Equal("example.com", database.LastParameters!["Host"]);
        Assert.Equal("current-hash", database.LastParameters["UriHash"]);
        Assert.Equal("content-hash", database.LastParameters["ListingParserInputHash"]);
    }

    private static Page CreatePage()
    {
        Website website = new(new Uri("https://example.com"));
        return new Page(website, new Uri("https://example.com/listing/1"));
    }

    private static DataTable CreatePageTable()
    {
        DataTable table = new();
        table.Columns.Add("Host", typeof(string));
        table.Columns.Add("Uri", typeof(string));
        table.Columns.Add("UriHash", typeof(string));
        table.Columns.Add("Inserted", typeof(DateTime));
        return table;
    }
}
