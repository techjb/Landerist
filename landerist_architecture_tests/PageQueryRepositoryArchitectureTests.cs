namespace landerist_architecture_tests;

public sealed class PageQueryRepositoryArchitectureTests
{
    [Fact]
    public void PageQueryRepository_IsOnlyACompatibilityFacade()
    {
        string facade = ReadSqlSource("PageQueryRepository.cs");

        Assert.Contains("PageLookupQueries", facade);
        Assert.Contains("PageScrapingQueries", facade);
        Assert.Contains("PageListingQueries", facade);
        Assert.Contains("PageSqlQueryBuilder", facade);
        Assert.DoesNotContain("SELECT ", facade);
        Assert.DoesNotContain("UPDATE ", facade);
        Assert.DoesNotContain("QueryTable(", facade);
        Assert.DoesNotContain("QueryInt(", facade);
    }

    [Fact]
    public void PageQueryFamilies_HaveDistinctResponsibilities()
    {
        string lookup = ReadSqlSource("PageLookupQueries.cs");
        string scraping = ReadSqlSource("PageScrapingQueries.cs");
        string listings = ReadSqlSource("PageListingQueries.cs");
        string builder = ReadSqlSource("PageSqlQueryBuilder.cs");

        Assert.Contains("GetPageByUriHash", lookup);
        Assert.DoesNotContain("SET LockedBy", lookup);
        Assert.Contains("GetScrapePages", scraping);
        Assert.Contains("LockedBy", scraping);
        Assert.Contains("GetListingsWithHttpStatusCodeError", listings);
        Assert.Contains("SelectColumns", builder);
        Assert.DoesNotContain("IDatabase", builder);
    }

    private static string ReadSqlSource(string fileName)
    {
        string root = FindRepositoryRoot();
        return File.ReadAllText(Path.Combine(
            root,
            "landerist_infrastructure",
            "Infrastructure",
            "Sql",
            fileName));
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Landerist.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            "Could not locate the repository root containing Landerist.sln.");
    }
}
