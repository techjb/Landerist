using landerist_library.Infrastructure.Sql;
using System.Data;

namespace landerist_unit_tests;

public sealed class WebsiteRepositoryTests
{
    [Fact]
    public void GetDataRow_UsesInjectedDatabase()
    {
        RecordingDatabase database = new();
        database.TableResult.Columns.Add("Host", typeof(string));
        database.TableResult.Rows.Add("example.com");
        WebsiteRepository repository = new(database);

        DataRow? result = repository.GetDataRow("example.com");

        Assert.NotNull(result);
        Assert.Equal("example.com", result["Host"]);
        Assert.Equal("example.com", database.LastParameters!["Host"]);
    }

    [Fact]
    public void Insert_DelegatesParametersAndResult()
    {
        RecordingDatabase database = new() { QueryResult = true };
        WebsiteRepository repository = new(database);
        Dictionary<string, object?> parameters = new()
        {
            ["Host"] = "example.com"
        };

        bool result = repository.Insert(parameters);

        Assert.True(result);
        Assert.Same(parameters, database.LastParameters);
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

        Assert.Throws<ArgumentException>(() =>
            repository.CountPagesSince("example.com", "Unexpected]Column", DateTime.UtcNow));

        Assert.Equal(string.Empty, database.LastQuery);
    }
}
