namespace landerist_architecture_tests;

public sealed class RobotsPolicyArchitectureTests
{
    [Fact]
    public void Application_DoesNotDependOnConcreteRobotsParser()
    {
        string repositoryRoot = FindRepositoryRoot();
        string applicationRoot = Path.Combine(
            repositoryRoot,
            "landerist_library",
            "Application");
        string sitemapService = Path.Combine(
            repositoryRoot,
            "landerist_library",
            "Infrastructure",
            "WebsiteServices",
            "WebsiteSitemapService.cs");
        string[] files = Directory
            .EnumerateFiles(applicationRoot, "*.cs", SearchOption.AllDirectories)
            .Append(sitemapService)
            .ToArray();
        string[] violations = files
            .Where(file =>
            {
                string source = File.ReadAllText(file);
                return source.Contains("Com.Bekijkhet.RobotsTxt", StringComparison.Ordinal) ||
                    source.Contains("GetSiteMapsFromRobotsTxt", StringComparison.Ordinal) ||
                    source.Contains("IsAllowedByRobotsTxt", StringComparison.Ordinal) ||
                    source.Contains("CrawlDelayTooBig()", StringComparison.Ordinal);
            })
            .Select(file => Path.GetRelativePath(repositoryRoot, file).Replace('\\', '/'))
            .Order()
            .ToArray();

        Assert.True(
            violations.Length == 0,
            "Application and sitemap orchestration must use IWebsiteRobotsPolicy." +
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
