using System.Text.RegularExpressions;

namespace landerist_architecture_tests;

public sealed partial class BrowserModuleArchitectureTests
{
    private static readonly string[] ForbiddenLegacyConfigurationPatterns =
    [
        @"(?<![A-Za-z0-9_])Config\.",
        @"(?<![A-Za-z0-9_])AppConfig\.",
        @"\bLanderistSettings\b"
    ];

    [Fact]
    public void BrowserModule_DoesNotDependOnSiblingInfrastructureModules()
    {
        IReadOnlyList<string> violations = GetBrowserSourceFiles()
            .SelectMany(path => FindForbiddenDependencies(path))
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.True(
            violations.Count == 0,
            "Infrastructure/Browser may depend on Application ports and browser/process " +
            "SDKs, but not on sibling Infrastructure modules or global configuration." +
            Environment.NewLine + string.Join(Environment.NewLine, violations));
    }

    [Fact]
    public void BrowserModule_ContainsOnlyBrowserNamespaces()
    {
        IReadOnlyList<string> violations = GetBrowserSourceFiles()
            .Where(path =>
            {
                Match match = NamespaceRegex().Match(File.ReadAllText(path));
                return !match.Success ||
                    !match.Groups["namespace"].Value.StartsWith(
                        "landerist_library.Infrastructure.Browser",
                        StringComparison.Ordinal);
            })
            .Select(path => Path.GetRelativePath(FindRepositoryRoot(), path))
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.True(
            violations.Count == 0,
            "Every source file in Infrastructure/Browser must belong to its module namespace." +
            Environment.NewLine + string.Join(Environment.NewLine, violations));
    }

    private static IEnumerable<string> FindForbiddenDependencies(string path)
    {
        string source = File.ReadAllText(path);
        string relativePath = Path.GetRelativePath(FindRepositoryRoot(), path);

        foreach (Match match in InfrastructureNamespaceRegex().Matches(source))
        {
            string @namespace = match.Groups["namespace"].Value;
            if (!@namespace.StartsWith(
                "landerist_library.Infrastructure.Browser",
                StringComparison.Ordinal))
            {
                yield return $"{relativePath}: references {@namespace}";
            }
        }

        foreach (string pattern in ForbiddenLegacyConfigurationPatterns)
        {
            if (Regex.IsMatch(source, pattern, RegexOptions.CultureInvariant))
            {
                yield return $"{relativePath}: accesses global configuration";
            }
        }
    }

    private static IReadOnlyList<string> GetBrowserSourceFiles() =>
        Directory.GetFiles(
            Path.Combine(
                FindRepositoryRoot(),
                "landerist_infrastructure",
                "Infrastructure",
                "Browser"),
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
