namespace landerist_architecture_tests;

public sealed class WebsiteDependencyArchitectureTests
{
    [Fact]
    public void Websites_DoesNotDependOnConfigurationOrIndexing()
    {
        string websitesRoot = Path.Combine(
            FindRepositoryRoot(),
            "landerist_library",
            "Websites");
        string[] forbiddenTokens =
        [
            "landerist_library.Configuration",
            "landerist_library.Index",
            "SitemapIndexer",
            "Config."
        ];
        string[] violations = Directory
            .EnumerateFiles(websitesRoot, "*.cs", SearchOption.AllDirectories)
            .Where(file =>
            {
                string source = File.ReadAllText(file);
                return forbiddenTokens.Any(token =>
                    source.Contains(token, StringComparison.Ordinal));
            })
            .Select(file => Path.GetFileName(file)!)
            .Order()
            .ToArray();

        Assert.True(
            violations.Length == 0,
            "Websites must receive configuration and indexing behavior explicitly." +
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
