namespace landerist_architecture_tests;

public sealed class RobotsServiceConsumersArchitectureTests
{
    [Fact]
    public void MigratedServices_DoNotCallWebsiteRobotsMethods()
    {
        string root = FindRepositoryRoot();
        string[] files =
        [
            Path.Combine(root, "landerist_infrastructure", "Infrastructure", "Sql", "Scraping", "WebsitesThrottle.cs"),
            Path.Combine(root, "landerist_infrastructure", "Infrastructure", "Listings", "SqlPageLinkService.cs"),
            Path.Combine(root, "landerist_infrastructure", "Infrastructure", "Sql", "Scraping", "SqlWebsiteThrottleService.cs"),
            Path.Combine(root, "landerist_infrastructure", "Infrastructure", "Administration", "WebsiteAdministrationService.cs")
        ];
        string[] forbiddenTokens =
        [
            ".IsAllowedByRobotsTxt(",
            ".IsMainUriAllowedByRobotsTxt(",
            ".CrawlDelay()",
            ".CrawlDelayTooBig()",
            ".CountRobotsSiteMaps()"
        ];
        string[] violations = files
            .Where(file =>
            {
                string source = File.ReadAllText(file);
                return forbiddenTokens.Any(token =>
                    source.Contains(token, StringComparison.Ordinal));
            })
            .Select(file => Path.GetRelativePath(root, file).Replace('\\', '/'))
            .Order()
            .ToArray();

        Assert.True(
            violations.Length == 0,
            "Migrated services must use IWebsiteRobotsPolicy." +
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
