using System.Text.RegularExpressions;

namespace landerist_architecture_tests;

public sealed partial class ArchitectureBoundaryTests
{
    private static readonly string RepositoryRoot = FindRepositoryRoot();
    private static readonly string LibraryRoot = Path.Combine(RepositoryRoot, "landerist_library");
    private static readonly HashSet<string> ApplicationAllowedDependencies =
        new(StringComparer.Ordinal) { "Application", "Pages", "Websites" };

    [Fact]
    public void Application_DoesNotDependOnOuterLayers()
    {
        string applicationRoot = Path.Combine(LibraryRoot, "Application");
        List<string> violations = [];

        foreach (string file in GetSourceFiles(applicationRoot))
        {
            string source = File.ReadAllText(file);
            foreach (Match match in LanderistNamespaceReferenceRegex().Matches(source))
            {
                string dependency = match.Groups["root"].Value;
                if (!ApplicationAllowedDependencies.Contains(dependency))
                {
                    violations.Add($"{GetRelativePath(file)} -> landerist_library.{dependency}");
                }
            }
        }

        Assert.True(
            violations.Count == 0,
            "Application must only depend on Application, Pages and Websites." +
            Environment.NewLine +
            string.Join(Environment.NewLine, violations.Distinct().Order()));
    }

    [Theory]
    [InlineData("Application")]
    [InlineData("Infrastructure")]
    public void ArchitecturalFolders_UseMatchingNamespaces(string boundary)
    {
        string boundaryRoot = Path.Combine(LibraryRoot, boundary);
        List<string> violations = [];

        foreach (string file in GetSourceFiles(boundaryRoot))
        {
            Match match = NamespaceDeclarationRegex().Match(File.ReadAllText(file));
            string expectedNamespace = $"landerist_library.{boundary}";

            if (!match.Success || !IsExpectedNamespace(match.Groups["namespace"].Value, expectedNamespace))
            {
                string actualNamespace = match.Success ? match.Groups["namespace"].Value : "<missing>";
                violations.Add($"{GetRelativePath(file)}: expected {expectedNamespace}, found {actualNamespace}");
            }
        }

        Assert.True(
            violations.Count == 0,
            $"Files under {boundary} must use its namespace." + Environment.NewLine +
            string.Join(Environment.NewLine, violations));
    }

    [Theory]
    [InlineData("Pages")]
    [InlineData("Websites")]
    public void DomainAreas_DoNotReachIntoInfrastructure(string area)
    {
        string areaRoot = Path.Combine(LibraryRoot, area);
        List<string> violations = [];

        foreach (string file in GetSourceFiles(areaRoot))
        {
            string source = File.ReadAllText(file);
            if (source.Contains("landerist_library.Infrastructure", StringComparison.Ordinal) ||
                source.Contains("LegacyDatabase", StringComparison.Ordinal))
            {
                violations.Add(GetRelativePath(file));
            }
        }

        Assert.True(
            violations.Count == 0,
            $"{area} must use Application ports instead of SQL infrastructure " +
            "or the legacy database service locator." + Environment.NewLine +
            string.Join(Environment.NewLine, violations));
    }

    [Fact]
    public void LegacyDependencyBaseline_DoesNotGrowOrBecomeStale()
    {
        string baselinePath = Path.Combine(AppContext.BaseDirectory, "ArchitectureBaseline.txt");
        HashSet<string> expected = File.ReadLines(baselinePath)
            .Select(line => line.Trim())
            .Where(line => line.Length > 0 && !line.StartsWith('#'))
            .ToHashSet(StringComparer.Ordinal);
        HashSet<string> actual = FindLegacyBoundaryReferences();

        string[] added = actual.Except(expected).Order().ToArray();
        string[] removed = expected.Except(actual).Order().ToArray();
        List<string> message = [];

        if (added.Length > 0)
        {
            message.Add("New forbidden dependencies (move behavior behind an Application port):");
            message.AddRange(added);
        }

        if (removed.Length > 0)
        {
            message.Add("Resolved dependencies (remove them from the baseline):");
            message.AddRange(removed);
        }

        Assert.True(
            added.Length == 0 && removed.Length == 0,
            string.Join(Environment.NewLine, message));
    }

    private static HashSet<string> FindLegacyBoundaryReferences()
    {
        var references = new HashSet<string>(StringComparer.Ordinal);
        string applicationRoot = Path.Combine(LibraryRoot, "Application");
        string infrastructureRoot = Path.Combine(LibraryRoot, "Infrastructure");

        foreach (string file in GetSourceFiles(LibraryRoot))
        {
            if (IsWithin(file, applicationRoot) || IsWithin(file, infrastructureRoot))
            {
                continue;
            }

            string source = File.ReadAllText(file);
            foreach (Match match in ArchitecturalNamespaceReferenceRegex().Matches(source))
            {
                references.Add($"{match.Groups["boundary"].Value}|{GetRelativePath(file)}");
            }
        }

        return references;
    }

    private static IEnumerable<string> GetSourceFiles(string root) =>
        Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .Where(file =>
                !file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase) &&
                !file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase));

    private static bool IsWithin(string file, string directory)
    {
        string relative = Path.GetRelativePath(directory, file);
        return !relative.StartsWith("..", StringComparison.Ordinal) && !Path.IsPathRooted(relative);
    }

    private static bool IsExpectedNamespace(string actualNamespace, string expectedNamespace) =>
        actualNamespace.Equals(expectedNamespace, StringComparison.Ordinal) ||
        actualNamespace.StartsWith(expectedNamespace + ".", StringComparison.Ordinal);

    private static string GetRelativePath(string file) =>
        Path.GetRelativePath(RepositoryRoot, file).Replace('\\', '/');

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

        throw new DirectoryNotFoundException("Could not locate the repository root containing Landerist.sln.");
    }

    [GeneratedRegex(@"(?:global::)?landerist_library\.(?<root>[A-Za-z_][A-Za-z0-9_]*)", RegexOptions.CultureInvariant)]
    private static partial Regex LanderistNamespaceReferenceRegex();

    [GeneratedRegex(@"(?:global::)?landerist_library\.(?<boundary>Application|Infrastructure)(?:\.|\b)", RegexOptions.CultureInvariant)]
    private static partial Regex ArchitecturalNamespaceReferenceRegex();

    [GeneratedRegex(@"\bnamespace\s+(?<namespace>landerist_library(?:\.[A-Za-z_][A-Za-z0-9_]*)*)", RegexOptions.CultureInvariant)]
    private static partial Regex NamespaceDeclarationRegex();
}
