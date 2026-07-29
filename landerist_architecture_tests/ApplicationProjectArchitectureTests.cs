using System.Xml.Linq;

namespace landerist_architecture_tests;

public sealed class ApplicationProjectArchitectureTests
{
    [Fact]
    public void ApplicationProject_DependsOnlyOnDomainAndOrels()
    {
        string root = FindRepositoryRoot();
        XDocument project = XDocument.Load(Path.Combine(
            root,
            "landerist_application",
            "landerist_application.csproj"));

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
                "..\\landerist_domain\\landerist_domain.csproj",
                "..\\landerist_orels\\landerist_orels.csproj"
            ],
            projects);
    }

    [Fact]
    public void Application_IsPhysicallyOwnedByApplicationProject()
    {
        string root = FindRepositoryRoot();

        Assert.True(Directory.Exists(Path.Combine(root, "landerist_application", "Application")));
        Assert.False(Directory.Exists(Path.Combine(root, "landerist_library", "Application")));
    }

    [Fact]
    public void LegacyLibraryProject_DoesNotExist()
    {
        string root = FindRepositoryRoot();

        Assert.False(Directory.Exists(Path.Combine(root, "landerist_library")));
        Assert.DoesNotContain(
            "landerist_library\\landerist_library.csproj",
            File.ReadAllText(Path.Combine(root, "Landerist.sln")),
            StringComparison.OrdinalIgnoreCase);
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