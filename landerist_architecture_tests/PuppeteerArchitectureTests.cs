namespace landerist_architecture_tests;

public sealed class PuppeteerArchitectureTests
{
    [Fact]
    public void BrowserConstruction_DoesNotReadGlobalConfiguration()
    {
        string root = Path.Combine(
            FindRepositoryRoot(),
            "landerist_infrastructure");
        string[] files =
        [
            "Infrastructure/Downloaders/Puppeteer/PuppeteerDownloader.cs",
            "Infrastructure/Downloaders/Puppeteer/PuppeteerDownloaderFactory.cs",
            "Infrastructure/Downloaders/Puppeteer/PuppeteerLaunchOptionsFactory.cs",
            "Infrastructure/Downloaders/Multiple/DownloadersPool.cs",
            "Infrastructure/Downloaders/Multiple/SingleDownloader.cs"
        ];
        string[] forbiddenTokens =
        [
            "Config.",
            "AppConfig.",
            "LanderistSettings"
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
    public void DownloadersPool_DoesNotKeepGlobalSessionState()
    {
        string poolFile = Path.Combine(
            FindRepositoryRoot(),
            "landerist_infrastructure",
            "Infrastructure",
            "Downloaders",
            "Multiple",
            "DownloadersPool.cs");
        string source = File.ReadAllText(poolFile);

        Assert.DoesNotContain(
            "static readonly List<SingleDownloader>",
            source,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "static readonly Lock",
            source,
            StringComparison.Ordinal);
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
