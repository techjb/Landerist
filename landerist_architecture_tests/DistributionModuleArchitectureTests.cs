using System.Text.RegularExpressions;

namespace landerist_architecture_tests;

public sealed partial class DistributionModuleArchitectureTests
{
    [Fact]
    public void CloudAndFileSystemAdapters_AreIsolatedSubmodules()
    {
        AssertSubmoduleBoundary("Cloud");
        AssertSubmoduleBoundary("FileSystem");

        string publisher = File.ReadAllText(Path.Combine(
            GetDistributionDirectory(), "DownloadsArtifactPublisher.cs"));
        Assert.Contains("Distribution.Cloud", publisher, StringComparison.Ordinal);
        Assert.Contains("Distribution.FileSystem", publisher, StringComparison.Ordinal);
        Assert.DoesNotContain("new S3(", publisher, StringComparison.Ordinal);
    }

    [Fact]
    public void DirectCloudSdkUsage_IsConfinedToCloudSubmodule()
    {
        string distribution = GetDistributionDirectory();
        string[] directConsumers = Directory
            .GetFiles(distribution, "*.cs", SearchOption.TopDirectoryOnly)
            .Where(path =>
            {
                string source = File.ReadAllText(path);
                return source.Contains("new S3(", StringComparison.Ordinal) ||
                    source.Contains("new AmazonCloudFrontClient(", StringComparison.Ordinal);
            })
            .Select(Path.GetFileName)
            .Order(StringComparer.Ordinal)
            .ToArray()!;

        Assert.Empty(directConsumers);
    }

    [Fact]
    public void DistributionFileSystemAdapter_DoesNotDependOnCloudOrOtherModules()
    {
        string directory = Path.Combine(GetDistributionDirectory(), "FileSystem");
        foreach (string path in Directory.GetFiles(directory, "*.cs"))
        {
            string source = File.ReadAllText(path);
            Assert.DoesNotContain("Amazon.", source, StringComparison.Ordinal);
            Assert.DoesNotContain("landerist_library.Export", source, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Distribution_DoesNotDependOnSqlOrWebsiteServiceImplementations()
    {
        string[] forbidden =
        [
            "landerist_library.Infrastructure.Sql",
            "landerist_library.Infrastructure.WebsiteServices",
            "landerist_library.Infrastructure.Runtime"
        ];
        string[] violations = Directory
            .GetFiles(GetDistributionDirectory(), "*.cs", SearchOption.AllDirectories)
            .Where(path => forbidden.Any(item => File.ReadAllText(path).Contains(
                item,
                StringComparison.Ordinal)))
            .Select(path => Path.GetRelativePath(FindRepositoryRoot(), path))
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Empty(violations);
    }

    [Fact]
    public void DistributionReadPorts_AreOwnedByApplication()
    {
        string application = Path.Combine(
            FindRepositoryRoot(), "landerist_application", "Application");

        Assert.True(File.Exists(Path.Combine(
            application, "Statistics", "IPageStatisticsRepository.cs")));
        Assert.True(File.Exists(Path.Combine(
            application, "Distribution", "IDistributionWebsiteMetrics.cs")));
        Assert.True(File.Exists(Path.Combine(
            application, "Distribution", "IWebsiteExportSource.cs")));
        Assert.True(File.Exists(Path.Combine(
            application, "Distribution", "DistributionOptions.cs")));
    }

    private static void AssertSubmoduleBoundary(string submodule)
    {
        string directory = Path.Combine(GetDistributionDirectory(), submodule);
        foreach (string path in Directory.GetFiles(directory, "*.cs"))
        {
            string source = File.ReadAllText(path);
            Match namespaceMatch = NamespaceRegex().Match(source);
            Assert.True(namespaceMatch.Success);
            Assert.StartsWith(
                $"landerist_library.Infrastructure.Distribution.{submodule}",
                namespaceMatch.Groups["namespace"].Value,
                StringComparison.Ordinal);

            foreach (Match dependency in InfrastructureNamespaceRegex().Matches(source))
            {
                Assert.StartsWith(
                    "landerist_library.Infrastructure.Distribution",
                    dependency.Groups["namespace"].Value,
                    StringComparison.Ordinal);
            }
        }
    }

    private static string GetDistributionDirectory() => Path.Combine(
        FindRepositoryRoot(), "landerist_infrastructure", "Infrastructure", "Distribution");

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
