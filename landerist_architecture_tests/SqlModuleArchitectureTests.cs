using System.Text.RegularExpressions;

namespace landerist_architecture_tests;

public sealed partial class SqlModuleArchitectureTests
{
    [Fact]
    public void SqlModule_DoesNotDependOnSiblingInfrastructureModules()
    {
        IReadOnlyList<string> violations = GetSqlSourceFiles()
            .SelectMany(path => InfrastructureNamespaceRegex()
                .Matches(File.ReadAllText(path))
                .Select(match => match.Groups["namespace"].Value)
                .Where(@namespace => !@namespace.StartsWith(
                    "landerist_library.Infrastructure.Sql",
                    StringComparison.Ordinal))
                .Select(@namespace =>
                    $"{Path.GetRelativePath(FindRepositoryRoot(), path)}: references {@namespace}"))
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.True(
            violations.Count == 0,
            "Infrastructure/Sql may depend on Domain, Application ports and the Database " +
            "abstraction, but not on sibling Infrastructure modules." +
            Environment.NewLine + string.Join(Environment.NewLine, violations));
    }

    [Fact]
    public void BatchPersistenceContracts_AreOwnedByApplication()
    {
        string applicationDirectory = Path.Combine(
            FindRepositoryRoot(),
            "landerist_application",
            "Application",
            "Parsing");
        string infrastructureDirectory = Path.Combine(
            FindRepositoryRoot(),
            "landerist_infrastructure",
            "Infrastructure");

        string[] contracts =
        [
            "BatchProvider.cs",
            "IBatchStore.cs",
            "IBatchRegistrationStore.cs"
        ];

        foreach (string contract in contracts)
        {
            Assert.True(File.Exists(Path.Combine(applicationDirectory, contract)));
            Assert.Empty(Directory.GetFiles(
                infrastructureDirectory,
                contract,
                SearchOption.AllDirectories));
        }
    }

    [Fact]
    public void SqlModule_ContainsOnlySqlNamespaces()
    {
        IReadOnlyList<string> violations = GetSqlSourceFiles()
            .Where(path =>
            {
                Match match = NamespaceRegex().Match(File.ReadAllText(path));
                return !match.Success ||
                    !match.Groups["namespace"].Value.StartsWith(
                        "landerist_library.Infrastructure.Sql",
                        StringComparison.Ordinal);
            })
            .Select(path => Path.GetRelativePath(FindRepositoryRoot(), path))
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.True(
            violations.Count == 0,
            "Every source file in Infrastructure/Sql must belong to its module namespace." +
            Environment.NewLine + string.Join(Environment.NewLine, violations));
    }

    private static IReadOnlyList<string> GetSqlSourceFiles() =>
        Directory.GetFiles(
            Path.Combine(
                FindRepositoryRoot(),
                "landerist_infrastructure",
                "Infrastructure",
                "Sql"),
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
