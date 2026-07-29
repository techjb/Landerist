using System.Xml.Linq;

namespace landerist_architecture_tests;

public sealed class DomainProjectArchitectureTests
{
    [Fact]
    public void DomainProject_DependsOnlyOnOrels()
    {
        string root = FindRepositoryRoot();
        string projectPath = Path.Combine(
            root,
            "landerist_domain",
            "landerist_domain.csproj");
        XDocument project = XDocument.Load(projectPath);

        string[] packageReferences = project
            .Descendants("PackageReference")
            .Select(element => (string?)element.Attribute("Include"))
            .Where(value => value is not null)
            .Cast<string>()
            .ToArray();
        string[] projectReferences = project
            .Descendants("ProjectReference")
            .Select(element => (string?)element.Attribute("Include"))
            .Where(value => value is not null)
            .Cast<string>()
            .ToArray();

        Assert.Empty(packageReferences);
        Assert.Equal(
            ["..\\landerist_orels\\landerist_orels.csproj"],
            projectReferences);
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

    [Fact]
    public void DomainAreas_ArePhysicallyOwnedByDomainProject()
    {
        string root = FindRepositoryRoot();

        Assert.True(Directory.Exists(Path.Combine(root, "landerist_domain", "Pages")));
        Assert.True(Directory.Exists(Path.Combine(root, "landerist_domain", "Websites")));
        Assert.False(Directory.Exists(Path.Combine(root, "landerist_library", "Pages")));
        Assert.False(Directory.Exists(Path.Combine(root, "landerist_library", "Websites")));
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