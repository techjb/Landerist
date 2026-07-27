namespace landerist_architecture_tests;

public sealed class RobotsMediaArchitectureTests
{
    [Fact]
    public void ParseMedia_DoesNotCallWebsiteRobotsMethods()
    {
        string mediaRoot = Path.Combine(
            FindRepositoryRoot(),
            "landerist_library",
            "Parse",
            "Media");
        string[] forbiddenCalls =
        [
            ".IsAllowedByRobotsTxt(",
            ".IsMainUriAllowedByRobotsTxt(",
            ".CrawlDelay(",
            ".CrawlDelayTooBig(",
            ".GetSiteMapsFromRobotsTxt(",
            ".CountRobotsSiteMaps("
        ];
        string[] violations = Directory
            .EnumerateFiles(mediaRoot, "*.cs", SearchOption.AllDirectories)
            .Where(file =>
            {
                string source = File.ReadAllText(file);
                return forbiddenCalls.Any(call =>
                    source.Contains(call, StringComparison.Ordinal));
            })
            .Select(file => Path.GetRelativePath(mediaRoot, file))
            .ToArray();

        Assert.True(
            violations.Length == 0,
            "Parse/Media must use IWebsiteRobotsPolicy." +
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
