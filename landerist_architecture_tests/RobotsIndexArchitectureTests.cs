namespace landerist_architecture_tests;

public sealed class RobotsIndexArchitectureTests
{
    [Fact]
    public void Index_DoesNotCallWebsiteRobotsMethods()
    {
        string root = FindRepositoryRoot();
        string indexRoot = Path.Combine(root, "landerist_infrastructure", "Infrastructure", "Indexing");
        string[] violations = Directory
            .EnumerateFiles(indexRoot, "*.cs", SearchOption.AllDirectories)
            .Where(file =>
            {
                string source = File.ReadAllText(file);
                return source.Contains(".IsAllowedByRobotsTxt(", StringComparison.Ordinal) ||
                    source.Contains(".CrawlDelay()", StringComparison.Ordinal) ||
                    source.Contains("GetSiteMapsFromRobotsTxt", StringComparison.Ordinal);
            })
            .Select(file => Path.GetRelativePath(root, file).Replace('\\', '/'))
            .Order()
            .ToArray();

        Assert.True(
            violations.Length == 0,
            "Index must use IWebsiteRobotsPolicy." +
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
