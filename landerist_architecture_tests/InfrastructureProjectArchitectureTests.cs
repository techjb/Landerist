using System.Xml.Linq;

namespace landerist_architecture_tests;

public sealed class InfrastructureProjectArchitectureTests
{
    [Fact]
    public void InfrastructureProject_DoesNotReferenceLegacyLibraryOrPackages()
    {
        string root = FindRepositoryRoot();
        XDocument project = XDocument.Load(Path.Combine(
            root,
            "landerist_infrastructure",
            "landerist_infrastructure.csproj"));

        string[] packages = project.Descendants("PackageReference")
            .Select(element => (string?)element.Attribute("Include"))
            .Where(value => value is not null)
            .Cast<string>()
            .ToArray();
        string[] projects = project.Descendants("ProjectReference")
            .Select(element => (string?)element.Attribute("Include"))
            .Where(value => value is not null)
            .Cast<string>()
            .Order()
            .ToArray();

        Assert.Empty(packages);
        Assert.Equal(
            [
                "..\\landerist_application\\landerist_application.csproj",
                "..\\landerist_domain\\landerist_domain.csproj"
            ],
            projects);
        Assert.DoesNotContain(
            projects,
            reference => reference.Contains("landerist_library", StringComparison.Ordinal));
    }

    [Fact]
    public void HttpInfrastructure_IsPhysicallyOwnedByInfrastructureProject()
    {
        string root = FindRepositoryRoot();

        Assert.True(Directory.Exists(Path.Combine(
            root,
            "landerist_infrastructure",
            "Infrastructure",
            "Http")));
        Assert.False(Directory.Exists(Path.Combine(
            root,
            "landerist_library",
            "Infrastructure",
            "Http")));
    }

    [Fact]
    public void LegacyLibrary_ReferencesInfrastructureProject()
    {
        string project = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "landerist_library",
            "landerist_library.csproj"));

        Assert.Contains(
            "..\\landerist_infrastructure\\landerist_infrastructure.csproj",
            project);
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