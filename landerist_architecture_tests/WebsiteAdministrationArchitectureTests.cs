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
        Assert.Contains("AdministrationOptions options", cleanup);
        Assert.Contains("IWebsiteCleanupFileReader", cleanup);
        Assert.DoesNotContain("AppConfig.", cleanup);
        Assert.DoesNotContain("IWebsiteNetworkService", cleanup);
    }

    [Fact]
    public void AdministrationModule_DoesNotReadGlobalConfiguration()
    {
        string directory = Path.Combine(
            FindRepositoryRoot(),
            "landerist_infrastructure",
            "Infrastructure",
            "Administration");
        string[] forbidden = ["Config.", "AppConfig.", "LanderistSettings"];

        string[] violations = Directory.GetFiles(directory, "*.cs")
            .Where(path => forbidden.Any(token =>
                File.ReadAllText(path).Contains(token, StringComparison.Ordinal)))
            .Select(Path.GetFileName)
            .OfType<string>()
            .ToArray();

        Assert.Empty(violations);
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
