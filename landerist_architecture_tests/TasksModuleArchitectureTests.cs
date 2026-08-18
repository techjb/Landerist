using System.Text.RegularExpressions;

namespace landerist_architecture_tests;

public sealed partial class TasksModuleArchitectureTests
{
    [Fact]
    public void TasksModule_DoesNotDependOnSiblingInfrastructureModules()
    {
        IReadOnlyList<string> violations = GetSourceFiles()
            .SelectMany(path => InfrastructureNamespaceRegex()
                .Matches(File.ReadAllText(path))
                .Select(match => match.Groups["namespace"].Value)
                .Where(@namespace => !@namespace.StartsWith(
                    "landerist_library.Infrastructure.Tasks",
                    StringComparison.Ordinal))
                .Select(@namespace =>
                    $"{Path.GetRelativePath(FindRepositoryRoot(), path)}: references {@namespace}"))
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.True(
            violations.Count == 0,
            "Infrastructure/Tasks must orchestrate through Domain and Application ports, " +
            "not through sibling Infrastructure modules." + Environment.NewLine +
            string.Join(Environment.NewLine, violations));
    }

    [Fact]
    public void TasksModule_DoesNotAccessGlobalConfiguration()
    {
        string[] patterns =
        [
            @"(?<![A-Za-z0-9_])Config\.",
            @"(?<![A-Za-z0-9_])AppConfig\.",
            @"\bLanderistSettings\b"
        ];
        IReadOnlyList<string> violations = GetSourceFiles()
            .Where(path => patterns.Any(pattern => Regex.IsMatch(
                File.ReadAllText(path), pattern, RegexOptions.CultureInvariant)))
            .Select(path => Path.GetRelativePath(FindRepositoryRoot(), path))
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.True(
            violations.Count == 0,
            "Task orchestration must receive typed options through composition." +
            Environment.NewLine + string.Join(Environment.NewLine, violations));
    }

    [Fact]
    public void BatchTaskPorts_AreOwnedByApplication()
    {
        string application = Path.Combine(
            FindRepositoryRoot(), "landerist_application", "Application");
        string infrastructureTasks = Path.Combine(
            FindRepositoryRoot(), "landerist_infrastructure", "Infrastructure", "Tasks");
        string[] parsingPorts =
        [
            "IBatchInputWriter.cs",
            "IListingBatchUploadProvider.cs",
            "IBatchListingResponseParser.cs"
        ];

        foreach (string port in parsingPorts)
        {
            Assert.True(File.Exists(Path.Combine(application, "Parsing", port)));
            Assert.Empty(Directory.GetFiles(infrastructureTasks, port));
        }

        Assert.True(File.Exists(Path.Combine(
            application, "Tasks", "IBatchArtifactCleaner.cs")));
        Assert.Empty(Directory.GetFiles(
            infrastructureTasks, "IBatchArtifactCleaner.cs"));
    }

    private static IReadOnlyList<string> GetSourceFiles() =>
        Directory.GetFiles(
            Path.Combine(FindRepositoryRoot(), "landerist_infrastructure",
                "Infrastructure", "Tasks"),
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
}
