namespace landerist_architecture_tests;

public sealed class StatisticsPageArchitectureTests
{
    [Fact]
    public void StatisticsPage_OnlyCoordinatesPageGeneration()
    {
        string page = ReadDistributionSource("StatisticsPage.cs");

        Assert.Contains("StatisticsChartsBuilder", page);
        Assert.Contains("StatisticsSummaryTableBuilder", page);
        Assert.Contains("StatisticsPageRenderer", page);
        Assert.DoesNotContain("File.", page);
        Assert.DoesNotContain("new S3", page);
        Assert.DoesNotContain("JsonSerializer", page);
        Assert.DoesNotContain("GetLatestStatistics", page);
    }

    [Fact]
    public void StatisticsPageSideEffects_BelongToRenderer()
    {
        string renderer = ReadDistributionSource("StatisticsPageRenderer.cs");
        string charts = ReadDistributionSource("StatisticsChartsBuilder.cs");
        string formatter = ReadDistributionSource("StatisticsChartFormatter.cs");

        Assert.Contains("File.ReadAllText", renderer);
        Assert.Contains("File.WriteAllText", renderer);
        Assert.Contains("IWebsiteArtifactStorage", renderer);
        Assert.Contains("_storage.Upload", renderer);
        Assert.DoesNotContain("new S3", renderer);
        Assert.DoesNotContain("File.", charts);
        Assert.DoesNotContain("new S3", charts);
        Assert.DoesNotContain("File.", formatter);
        Assert.DoesNotContain("new S3", formatter);
    }

    private static string ReadDistributionSource(string fileName)
    {
        string root = FindRepositoryRoot();
        return File.ReadAllText(Path.Combine(
            root,
            "landerist_infrastructure",
            "Infrastructure",
            "Distribution",
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
