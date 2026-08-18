using System.Text.RegularExpressions;

namespace landerist_architecture_tests;

public sealed partial class AiModuleArchitectureTests
{
    private static readonly string[] ForbiddenNamespacePrefixes =
    [
        "landerist_library.Configuration",
        "landerist_library.Database",
        "landerist_library.Infrastructure.Configuration",
        "landerist_library.Infrastructure.DatabaseMaintenance",
        "landerist_library.Infrastructure.Scraping",
        "landerist_library.Infrastructure.Sql"
    ];

    private static readonly (string Name, string Pattern)[] ForbiddenLegacyConfigurationAccess =
    [
        ("Config.", @"(?<![A-Za-z0-9_])Config\."),
        ("AppConfig.", @"(?<![A-Za-z0-9_])AppConfig\."),
        ("LanderistSettings", @"\bLanderistSettings\b")
    ];

    [Fact]
    public void AiModule_DoesNotDependOnPersistenceScrapingOrConfigurationModules()
    {
        IReadOnlyList<string> violations = GetAiSourceFiles()
            .SelectMany(path => FindForbiddenDependencies(path))
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.True(
            violations.Count == 0,
            "Infrastructure/Ai must communicate through Domain and Application ports; " +
            "it must not depend directly on persistence, scraping or configuration modules." +
            Environment.NewLine + string.Join(Environment.NewLine, violations));
    }

    [Fact]
    public void AiModule_ContainsOnlyAiNamespaces()
    {
        IReadOnlyList<string> violations = GetAiSourceFiles()
            .Select(path => new
            {
                Path = path,
                Match = NamespaceRegex().Match(File.ReadAllText(path))
            })
            .Where(item =>
                !item.Match.Success ||
                !item.Match.Groups["namespace"].Value.StartsWith(
                    "landerist_library.Infrastructure.Ai",
                    StringComparison.Ordinal))
            .Select(item => Path.GetRelativePath(FindRepositoryRoot(), item.Path))
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.True(
            violations.Count == 0,
            "Every source file in Infrastructure/Ai must belong to its module namespace." +
            Environment.NewLine + string.Join(Environment.NewLine, violations));
    }

    private static IEnumerable<string> FindForbiddenDependencies(string path)
    {
        string source = File.ReadAllText(path);
        string relativePath = Path.GetRelativePath(FindRepositoryRoot(), path);

        foreach (string prefix in ForbiddenNamespacePrefixes)
        {
            if (source.Contains(prefix, StringComparison.Ordinal))
            {
                yield return $"{relativePath}: references {prefix}";
            }
        }

        foreach ((string name, string pattern) in ForbiddenLegacyConfigurationAccess)
        {
            if (Regex.IsMatch(source, pattern, RegexOptions.CultureInvariant))
            {
                yield return $"{relativePath}: references {name}";
            }
        }
    }

    private static IReadOnlyList<string> GetAiSourceFiles() =>
        Directory.GetFiles(
            Path.Combine(
                FindRepositoryRoot(),
                "landerist_infrastructure",
                "Infrastructure",
                "Ai"),
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
        @"\bnamespace\s+(?<namespace>[A-Za-z_][A-Za-z0-9_.]*)",
        RegexOptions.CultureInvariant)]
    private static partial Regex NamespaceRegex();
}
