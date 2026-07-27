namespace landerist_architecture_tests;

public sealed class ChromeMaintenanceArchitectureTests
{
    [Fact]
    public void BrowserOrchestration_DoesNotUseGlobalProcessUtilities()
    {
        string root = Path.Combine(
            FindRepositoryRoot(),
            "landerist_library");
        string[] files =
        [
            "Infrastructure/Scraping/ScrapeBrowserManager.cs",
            "Downloaders/Puppeteer/PuppeteerDownloader.cs"
        ];
        string[] forbiddenTokens =
        [
            "ChromeKiller",
            "Process.GetProcesses",
            "new Process(",
            "Config.IsConfiguration",
            "Config.IsPrincipalMachine"
        ];
        string[] violations = files
            .Where(relative =>
            {
                string source = File.ReadAllText(Path.Combine(
                    root,
                    relative.Replace('/', Path.DirectorySeparatorChar)));
                return forbiddenTokens.Any(token =>
                    source.Contains(token, StringComparison.Ordinal));
            })
            .ToArray();

        Assert.Empty(violations);
    }

    [Fact]
    public void LegacyChromeKiller_DoesNotExist()
    {
        string legacyFile = Path.Combine(
            FindRepositoryRoot(),
            "landerist_library",
            "Downloaders",
            "Puppeteer",
            "ChromeKiller.cs");

        Assert.False(File.Exists(legacyFile));
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
