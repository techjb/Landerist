namespace landerist_architecture_tests;

public sealed class GoolzoomArchitectureTests
{
    [Fact]
    public void GoolzoomIntegration_DoesNotUseGlobalConfigurationOrStaticClient()
    {
        string root = Path.Combine(
            FindRepositoryRoot(),
            "landerist_infrastructure");
        string apiFile = Path.Combine(
            root,
            "Infrastructure",
            "Location",
            "Providers",
            "Goolzoom",
            "GoolzoomApi.cs");
        string source = File.ReadAllText(apiFile);

        Assert.DoesNotContain("Config.", source, StringComparison.Ordinal);
        Assert.DoesNotContain("AppConfig.", source, StringComparison.Ordinal);
        Assert.DoesNotContain(
            "static readonly HttpClient",
            source,
            StringComparison.Ordinal);
    }

    [Fact]
    public void GoolzoomConsumers_DoNotConstructConcreteClient()
    {
        string root = Path.Combine(
            FindRepositoryRoot(),
            "landerist_infrastructure");
        string apiFile = Path.Combine(
            "Infrastructure",
            "Location",
            "Providers",
            "Goolzoom",
            "GoolzoomApi.cs");
        string[] violations = Directory
            .EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .Where(file =>
                !Path.GetRelativePath(root, file).Equals(
                    apiFile,
                    StringComparison.OrdinalIgnoreCase) &&
                File.ReadAllText(file).Contains(
                    "new GoolzoomApi(",
                    StringComparison.Ordinal))
            .Select(file => Path.GetRelativePath(root, file))
            .ToArray();

        Assert.Empty(violations);
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
