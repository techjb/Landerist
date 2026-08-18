namespace landerist_architecture_tests;

public sealed class IntegrationCiArchitectureTests
{
    [Fact]
    public void IntegrationTests_UseExplicitEnvironmentAndRunInCi()
    {
        string root = FindRepositoryRoot();
        string tests = File.ReadAllText(Path.Combine(
            root,
            "landerist_integration_tests",
            "DatabaseIntegrationTests.cs"));
        string workflow = File.ReadAllText(Path.Combine(
            root,
            ".github",
            "workflows",
            "architecture.yml"));

        Assert.Contains("LANDERIST_TEST_SQL_DATASOURCE", tests);
        Assert.Contains("SqlDatabaseOptions", tests);
        Assert.DoesNotContain("Config.", tests);
        Assert.DoesNotContain("AppConfig.", tests);
        Assert.Contains("integration-test:", workflow);
        Assert.Contains("mcr.microsoft.com/mssql/server:2022-latest", workflow);
        Assert.Contains("landerist_integration_tests.csproj", workflow);
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
