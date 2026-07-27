namespace landerist_architecture_tests;

public sealed class HttpTransportArchitectureTests
{
    [Fact]
    public void MigratedInfrastructureConsumers_DoNotBuildTransportDirectly()
    {
        string libraryRoot = Path.Combine(
            FindRepositoryRoot(),
            "landerist_library");
        string[] consumers =
        [
            "Infrastructure/WebsiteServices/WebsiteNetworkService.cs",
            "Infrastructure/Indexing/GzipAwareSitemapFetcher.cs",
            "Infrastructure/Scraping/ConditionalPageHeaderChecker.cs"
        ];
        string[] forbiddenTokens =
        [
            "new HttpClient(",
            "new HttpClientHandler",
            "new WebProxy",
            "AppConfig.PROXY_",
            "Config.HTTPCLIENT_SECONDS_TIMEOUT"
        ];
        string[] violations = consumers
            .Where(relative =>
            {
                string source = File.ReadAllText(
                    Path.Combine(
                        libraryRoot,
                        relative.Replace('/', Path.DirectorySeparatorChar)));
                return forbiddenTokens.Any(token =>
                    source.Contains(token, StringComparison.Ordinal));
            })
            .ToArray();

        Assert.True(
            violations.Length == 0,
            "Migrated HTTP consumers must use HttpClientTransportFactory." +
            Environment.NewLine +
            string.Join(Environment.NewLine, violations));
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
