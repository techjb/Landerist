using System.Text.RegularExpressions;

namespace landerist_architecture_tests;

public sealed partial class ScrapingModuleArchitectureTests
{
    private static readonly string[] AllowedInfrastructureDependencies =
    [
        "landerist_library.Infrastructure.Browser",
        "landerist_library.Infrastructure.Http"
    ];

    [Fact]
    public void ScrapingModule_DependsOnlyOnBrowserAndHttpInfrastructureModules()
    {
        IReadOnlyList<string> violations = GetSourceFiles()
            .SelectMany(path => InfrastructureNamespaceRegex()
                .Matches(File.ReadAllText(path))
                .Select(match => match.Groups["namespace"].Value)
                .Where(@namespace =>
                    !@namespace.StartsWith(
                        "landerist_library.Infrastructure.Scraping",
                        StringComparison.Ordinal) &&
                    !AllowedInfrastructureDependencies.Any(allowed =>
                        @namespace.StartsWith(allowed, StringComparison.Ordinal)))
                .Select(@namespace =>
                    $"{Path.GetRelativePath(FindRepositoryRoot(), path)}: references {@namespace}"))
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.True(
            violations.Count == 0,
            "Scraping may use only the lower-level Browser and Http Infrastructure modules." +
            Environment.NewLine + string.Join(Environment.NewLine, violations));
    }

    [Fact]
    public void ScrapingModule_DoesNotContainSqlAdaptersOrDatabaseAccess()
    {
        IReadOnlyList<string> violations = GetSourceFiles()
            .Where(path =>
            {
                string source = File.ReadAllText(path);
                return Path.GetFileName(path).StartsWith("Sql", StringComparison.Ordinal) ||
                    source.Contains("landerist_library.Database", StringComparison.Ordinal);
            })
            .Select(path => Path.GetRelativePath(FindRepositoryRoot(), path))
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.True(
            violations.Count == 0,
            "SQL adapters and direct database access must be owned by Infrastructure/Sql." +
            Environment.NewLine + string.Join(Environment.NewLine, violations));
    }

    [Fact]
    public void ScrapingModule_ContainsOnlyScrapingNamespaces()
    {
        IReadOnlyList<string> violations = GetSourceFiles()
            .Where(path =>
            {
                Match match = NamespaceRegex().Match(File.ReadAllText(path));
                return !match.Success || !match.Groups["namespace"].Value.StartsWith(
                    "landerist_library.Infrastructure.Scraping",
                    StringComparison.Ordinal);
            })
            .Select(path => Path.GetRelativePath(FindRepositoryRoot(), path))
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.True(
            violations.Count == 0,
            "Every source file in Infrastructure/Scraping must use its module namespace." +
            Environment.NewLine + string.Join(Environment.NewLine, violations));
    }

    private static IReadOnlyList<string> GetSourceFiles() =>
        Directory.GetFiles(
            Path.Combine(FindRepositoryRoot(), "landerist_infrastructure",
                "Infrastructure", "Scraping"),
            "*.cs",
            SearchOption.AllDirectories);

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? current = new(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Landerist.sln")))
            {
                return current.FullName;
            }
            current = current.Parent;
        }
        throw new DirectoryNotFoundException("Could not locate the repository root.");
    }

    [GeneratedRegex(
        @"\b(?:using|global\s+using)\s+(?<namespace>landerist_library\.Infrastructure(?:\.[A-Za-z_][A-Za-z0-9_]*)+)",
        RegexOptions.CultureInvariant)]
    private static partial Regex InfrastructureNamespaceRegex();

    [GeneratedRegex(
        @"\bnamespace\s+(?<namespace>[A-Za-z_][A-Za-z0-9_.]*)",
        RegexOptions.CultureInvariant)]
    private static partial Regex NamespaceRegex();
}
