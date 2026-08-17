namespace landerist_architecture_tests;

public sealed class DistributionConfigurationArchitectureTests
{
    [Fact]
    public void DistributionModule_DoesNotReadGlobalConfiguration()
    {
        string directory = Path.Combine(
            FindRepositoryRoot(),
            "landerist_infrastructure",
            "Infrastructure",
            "Distribution");
        string[] forbidden = ["Config.", "AppConfig.", "LanderistSettings"];

        string[] violations = Directory.GetFiles(directory, "*.cs")
            .Where(path => forbidden.Any(token =>
                File.ReadAllText(path).Contains(token, StringComparison.Ordinal)))
            .Select(Path.GetFileName)
            .OfType<string>()
            .ToArray();

        Assert.Empty(violations);
    }

    [Fact]
    public void DistributionOptions_FlowFromCompositionBoundary()
    {
        string root = FindRepositoryRoot();
        string adapter = File.ReadAllText(Path.Combine(
            root,
            "landerist_console",
            "LanderistRuntimeOptionsAdapter.cs"));
        string composition = File.ReadAllText(Path.Combine(
            root,
            "landerist_console",
            "LanderistDistributionComposition.cs"));
        string publisher = File.ReadAllText(Path.Combine(
            root,
            "landerist_infrastructure",
            "Infrastructure",
            "Distribution",
            "DistributionPublisher.cs"));

        Assert.Contains("Distribution = new DistributionOptions(", adapter);
        Assert.Contains("runtimeOptions.Distribution", composition);
        Assert.Contains("DistributionOptions options", publisher);
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
