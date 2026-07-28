using System.Xml.Linq;

namespace landerist_architecture_tests;

public sealed class InfrastructureProjectArchitectureTests
{
    [Fact]
    public void InfrastructureProject_UsesOnlyDeclaredAdapterDependencies()
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

        Assert.Equal(
            ["Com.Bekijkhet.RobotsTxt", "Microsoft.Data.SqlClient", "PuppeteerSharp"],
            packages);
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
    public void ExtractedInfrastructure_IsPhysicallyOwnedByInfrastructureProject()
    {
        string root = FindRepositoryRoot();

        Assert.True(Directory.Exists(Path.Combine(
            root,
            "landerist_infrastructure",
            "Infrastructure",
            "Http")));
        Assert.True(Directory.Exists(Path.Combine(
            root,
            "landerist_infrastructure",
            "Infrastructure",
            "Browser")));
        Assert.False(Directory.Exists(Path.Combine(
            root,
            "landerist_library",
            "Infrastructure",
            "Browser")));
        Assert.False(Directory.Exists(Path.Combine(
            root,
            "landerist_library",
            "Infrastructure",
            "Http")));
    }

    [Fact]
    public void ExtractedInfrastructure_DoesNotUseGlobalLogging()
    {
        string root = Path.Combine(
            FindRepositoryRoot(),
            "landerist_infrastructure",
            "Infrastructure");
        string[] violations = Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .Where(file => File.ReadAllText(file).Contains("Logs.Log", StringComparison.Ordinal))
            .Select(file => Path.GetFileName(file)!)
            .ToArray();

        Assert.Empty(violations);
    }
    [Fact]
    public void ExtractedWebsiteServices_AreOwnedByInfrastructureProject()
    {
        string root = FindRepositoryRoot();
        string[] services =
        [
            "WebsiteNetworkService.cs",
            "WebsiteRefreshService.cs",
            "WebsiteRobotsPolicy.cs",
            "WebsiteSitemapService.cs"
        ];

        foreach (string service in services)
        {
            Assert.True(File.Exists(Path.Combine(
                root,
                "landerist_infrastructure",
                "Infrastructure",
                "WebsiteServices",
                service)));
            Assert.False(File.Exists(Path.Combine(
                root,
                "landerist_library",
                "Infrastructure",
                "WebsiteServices",
                service)));
        }
    }
    [Fact]
    public void WebsiteSitemapService_DependsOnIndexerPort()
    {
        string source = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "landerist_infrastructure",
            "Infrastructure",
            "WebsiteServices",
            "WebsiteSitemapService.cs"));

        Assert.DoesNotContain("new SitemapIndexer", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Infrastructure.Indexing", source, StringComparison.Ordinal);
        Assert.Contains("IWebsiteSitemapIndexerFactory", source, StringComparison.Ordinal);
    }
    [Fact]
    public void SqlCore_IsPhysicallyOwnedByInfrastructureProject()
    {
        string root = FindRepositoryRoot();
        string[] databaseTypes = ["IDatabase.cs", "IDatabaseFactory.cs", "DataBase.cs"];
        foreach (string file in databaseTypes)
        {
            Assert.True(File.Exists(Path.Combine(root, "landerist_infrastructure", "Database", file)));
            Assert.False(File.Exists(Path.Combine(root, "landerist_library", "Database", file)));
        }

        string[] sqlTypes = ["SqlDatabaseFactory.cs", "SqlDatabaseOptions.cs"];
        foreach (string file in sqlTypes)
        {
            Assert.True(File.Exists(Path.Combine(
                root,
                "landerist_infrastructure",
                "Infrastructure",
                "Sql",
                file)));
            Assert.False(File.Exists(Path.Combine(
                root,
                "landerist_library",
                "Infrastructure",
                "Sql",
                file)));
        }
    }
    [Fact]
    public void PagePersistenceGroup_IsOwnedByInfrastructureProject()
    {
        string root = FindRepositoryRoot();
        string[] sqlFiles =
        [
            "PageRepository.cs",
            "PageQueryRepository.cs",
            "PageMaintenanceRepository.cs",
            "PageQueryOptions.cs",
            "SqlTableNames.cs"
        ];
        foreach (string file in sqlFiles)
        {
            Assert.True(File.Exists(Path.Combine(
                root,
                "landerist_infrastructure",
                "Infrastructure",
                "Sql",
                file)));
            Assert.False(File.Exists(Path.Combine(
                root,
                "landerist_library",
                "Infrastructure",
                "Sql",
                file)));
        }

        string[] mappers = ["PageDataMapper.cs", "WebsiteDataMapper.cs"];
        foreach (string file in mappers)
        {
            Assert.True(File.Exists(Path.Combine(
                root,
                "landerist_infrastructure",
                "Infrastructure",
                "Sql",
                "Mapping",
                file)));
            Assert.False(File.Exists(Path.Combine(
                root,
                "landerist_library",
                "Infrastructure",
                "Sql",
                "Mapping",
                file)));
        }
    }

    [Fact]
    public void PageQueryRepository_UsesExplicitOptions()
    {
        string source = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "landerist_infrastructure",
            "Infrastructure",
            "Sql",
            "PageQueryRepository.cs"));

        Assert.DoesNotContain("landerist_library.Configuration", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Config.", source, StringComparison.Ordinal);
        Assert.DoesNotContain("WebsitesThrottle.", source, StringComparison.Ordinal);
        Assert.Contains("PageQueryOptions", source, StringComparison.Ordinal);
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