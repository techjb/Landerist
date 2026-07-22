using landerist_library.Infrastructure.Sql;
using landerist_library.Infrastructure.Sql.Mapping;
using landerist_library.Websites;
using System.Data;

namespace landerist_unit_tests;

public sealed class WebsiteRepositoryTests
{
    [Fact]
    public void Mapper_RestoresWebsiteWithoutEntityDatabaseAccess()
    {
        DataTable table = new();
        table.Columns.Add("MainUri", typeof(string));
        table.Columns.Add("Host", typeof(string));
        table.Columns.Add("LanguageCode", typeof(string));
        table.Columns.Add("CountryCode", typeof(string));
        table.Rows.Add("https://example.com", "example.com", "es", "ES");

        Website result = WebsiteDataMapper.Map(table.Rows[0]);

        Assert.Equal("example.com", result.Host);
        Assert.Equal(new Uri("https://example.com"), result.MainUri);
    }

    [Fact]
    public void Insert_MapsEntityAndDelegatesResult()
    {
        RecordingDatabase database = new() { QueryResult = true };
        WebsiteRepository repository = new(database);
        Website website = new(new Uri("https://example.com"));

        bool result = repository.Insert(website);

        Assert.True(result);
        Assert.Equal("example.com", database.LastParameters!["Host"]);
        Assert.Equal("https://example.com/", database.LastParameters["MainUri"]);
        Assert.Contains("INSERT INTO", database.LastQuery);
    }

    [Fact]
    public void GetHosts_ReturnsInjectedHashSet()
    {
        RecordingDatabase database = new();
        database.HashSetResult.Add("example.com");
        WebsiteQueryRepository repository = new(database);

        HashSet<string> result = repository.GetHosts();

        Assert.Same(database.HashSetResult, result);
        Assert.Contains("SELECT [Host]", database.LastQuery);
    }

    [Fact]
    public void Exists_DelegatesHostParameter()
    {
        RecordingDatabase database = new() { QueryExistsResult = true };
        WebsiteQueryRepository repository = new(database);

        bool result = repository.Exists("example.com");

        Assert.True(result);
        Assert.Equal("example.com", database.LastParameters!["Host"]);
    }

    [Fact]
    public void GetNeedToUpdateRobotsTxt_DelegatesCutoffDate()
    {
        RecordingDatabase database = new();
        WebsiteQueryRepository repository = new(database);
        DateTime cutoff = new(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);

        repository.GetNeedToUpdateRobotsTxt(cutoff);

        Assert.Equal(cutoff, database.LastParameters!["RobotsTxtUpdatedSpecialRules"]);
    }

    [Fact]
    public void CountPagesSince_UsesValidatedDateColumn()
    {
        RecordingDatabase database = new() { QueryIntResult = 12 };
        WebsitePageMetricsRepository repository = new(database);
        DateTime cutoff = new(2026, 1, 1);

        int result = repository.CountPagesSince("example.com", "LastScrape", cutoff);

        Assert.Equal(12, result);
        Assert.Contains("[LastScrape] >= @DateFrom", database.LastQuery);
        Assert.Equal("example.com", database.LastParameters!["Host"]);
        Assert.Equal(cutoff, database.LastParameters["DateFrom"]);
    }

    [Fact]
    public void CountPagesSince_RejectsUnsupportedDateColumn()
    {
        RecordingDatabase database = new();
        WebsitePageMetricsRepository repository = new(database);

        Assert.Throws<ArgumentException>(() => repository.CountPagesSince("example.com", "Unexpected]Column", DateTime.UtcNow));
        Assert.Equal(string.Empty, database.LastQuery);
    }
}