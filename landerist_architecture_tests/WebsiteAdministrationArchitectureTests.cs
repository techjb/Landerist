namespace landerist_architecture_tests;

public sealed class WebsiteAdministrationArchitectureTests
{
    [Fact]
    public void Facade_DelegatesMaintenanceWithoutOwningInfrastructureWorkflows()
    {
        string source = ReadAdministrationFile("WebsiteAdministrationService.cs");

        Assert.Contains("RefreshOperations.RefreshAllMainUris()", source);
        Assert.Contains("RefreshOperations.RefreshOutdatedSitemaps()", source);
        Assert.Contains("Reporting.CountMainUriAccess()", source);
        Assert.Contains("FileCleanup.DeleteWebsitesWithoutListingUrl()", source);
        Assert.DoesNotContain("Parallel.ForEach", source);
        Assert.DoesNotContain("Console.WriteLine", source);
        Assert.DoesNotContain("AppConfig.", source);
    }

    [Fact]
    public void MaintenanceResponsibilities_AreSeparatedByUseCase()
    {
        string refresh = ReadAdministrationFile("WebsiteRefreshOperations.cs");
        string reporting = ReadAdministrationFile("WebsiteAdministrationReporting.cs");
        string cleanup = ReadAdministrationFile("WebsiteFileCleanup.cs");

        Assert.Contains("IWebsiteNetworkService", refresh);
        Assert.Contains("IWebsiteSitemapService", refresh);
        Assert.DoesNotContain("IWebsiteRobotsPolicy", refresh);
        Assert.Contains("IWebsiteRobotsPolicy", reporting);
        Assert.DoesNotContain("AppConfig.", reporting);
        Assert.Contains("AppConfig.INSERT_DIRECTORY", cleanup);
        Assert.DoesNotContain("IWebsiteNetworkService", cleanup);
    }

    private static string ReadAdministrationFile(string fileName) =>
        File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "landerist_infrastructure",
            "Infrastructure",
            "Administration",
            fileName));

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
