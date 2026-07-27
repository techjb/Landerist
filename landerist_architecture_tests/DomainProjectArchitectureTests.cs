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
    public void LegacyLibrary_ReferencesDomainProject()
    {
        string projectPath = Path.Combine(
            FindRepositoryRoot(),
            "landerist_library",
            "landerist_library.csproj");
        string project = File.ReadAllText(projectPath);

        Assert.Contains(
            "..\\landerist_domain\\landerist_domain.csproj",
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