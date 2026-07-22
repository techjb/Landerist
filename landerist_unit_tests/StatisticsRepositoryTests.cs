using landerist_library.Infrastructure.Sql;
using landerist_library.Statistics;
using System.Data;

namespace landerist_unit_tests;

public sealed class StatisticsRepositoryTests
{
    [Fact]
    public void HostCountPages_DelegatesHostAndReturnsCount()
    {
        RecordingDatabase database = new() { QueryIntResult = 42 };
        HostStatisticsRepository repository = new(database);

        int result = repository.CountPages("example.com");

        Assert.Equal(42, result);
        Assert.Equal("example.com", database.LastParameters!["Host"]);
        Assert.Contains("COUNT(*)", database.LastQuery);
        Assert.Contains("[Host] = @Host", database.LastQuery);
    }

    [Fact]
    public void HostInsert_DelegatesAllValues()
    {
        RecordingDatabase database = new() { QueryResult = true };
        HostStatisticsRepository repository = new(database);
        DateTime date = new(2026, 7, 21);

        bool result = repository.Insert(date, "example.com", "Listings", 17);

        Assert.True(result);
        Assert.Equal(date, database.LastParameters!["Date"]);
        Assert.Equal("example.com", database.LastParameters["Host"]);
        Assert.Equal("Listings", database.LastParameters["Key"]);
        Assert.Equal(17, database.LastParameters["Counter"]);
        Assert.Contains("DELETE FROM", database.LastQuery);
        Assert.Contains("INSERT INTO", database.LastQuery);
    }

    [Fact]
    public void HostDeleteByPrefixAndDate_AppendsWildcard()
    {
        RecordingDatabase database = new() { QueryResult = true };
        HostStatisticsRepository repository = new(database);
        DateTime date = new(2026, 7, 20);

        bool result = repository.DeleteByHostKeyPrefixAndDate(
            date,
            "example.com",
            "PageType");

        Assert.True(result);
        Assert.Equal(date, database.LastParameters!["Date"]);
        Assert.Equal("example.com", database.LastParameters["Host"]);
        Assert.Equal("PageType_%", database.LastParameters["KeyPrefix"]);
    }

    [Fact]
    public void HostGetKeysLike_ReturnsDatabaseListAndDelegatesPattern()
    {
        RecordingDatabase database = new();
        database.ListStringResult.Add("PageType_Listing");
        HostStatisticsRepository repository = new(database);

        List<string> result = repository.GetKeysLike(
            "example.com",
            HostStatisticsKey.PageType);

        Assert.Same(database.ListStringResult, result);
        Assert.Equal("example.com", database.LastParameters!["Host"]);
        Assert.Equal("PageType_%", database.LastParameters["Key"]);
        Assert.Contains("SELECT DISTINCT", database.LastQuery);
    }

    [Fact]
    public void GlobalCountWebsites_UsesInjectedDatabase()
    {
        RecordingDatabase database = new() { QueryIntResult = 12 };
        GlobalStatisticsRepository repository = new(database);

        int result = repository.CountWebsites();

        Assert.Equal(12, result);
        Assert.Null(database.LastParameters);
        Assert.Contains("COUNT(*)", database.LastQuery);
        Assert.Contains("[WEBSITES]", database.LastQuery);
    }

    [Fact]
    public void GlobalInsert_DelegatesAllValues()
    {
        RecordingDatabase database = new() { QueryResult = true };
        GlobalStatisticsRepository repository = new(database);
        DateTime date = new(2026, 7, 21);

        bool result = repository.Insert(date, "Listings", 33);

        Assert.True(result);
        Assert.Equal(date, database.LastParameters!["Date"]);
        Assert.Equal("Listings", database.LastParameters["Key"]);
        Assert.Equal(33, database.LastParameters["Counter"]);
        Assert.Contains("DELETE FROM", database.LastQuery);
        Assert.Contains("INSERT INTO", database.LastQuery);
    }

    [Fact]
    public void GlobalGetStatistics_DelegatesKeyAndMonthWindow()
    {
        RecordingDatabase database = new();
        GlobalStatisticsRepository repository = new(database);

        DataTable result = repository.GetStatistics("Listings", -6);

        Assert.Same(database.TableResult, result);
        Assert.Equal("Listings", database.LastParameters!["Key"]);
        Assert.Equal(-6, database.LastParameters["Months"]);
        Assert.Contains("DATEADD(MONTH, @Months", database.LastQuery);
        Assert.Contains("ORDER BY [Date] ASC", database.LastQuery);
    }

    [Fact]
    public void GlobalGetLatestStatistics_DelegatesTopAndKey()
    {
        RecordingDatabase database = new();
        GlobalStatisticsRepository repository = new(database);

        DataTable result = repository.GetLatestStatistics("Listings", 10);

        Assert.Same(database.TableResult, result);
        Assert.Equal(10, database.LastParameters!["Top"]);
        Assert.Equal("Listings", database.LastParameters["Key"]);
        Assert.Contains("TOP (@Top)", database.LastQuery);
        Assert.Contains("ORDER BY [Date] DESC", database.LastQuery);
    }
}
