namespace landerist_architecture_tests;

public sealed class BackupArchitectureTests
{
    [Fact]
    public void BackupModule_DoesNotReadGlobalConfigurationOrConstructSideEffectsInCoordinator()
    {
        string directory = Path.Combine(
            FindRepositoryRoot(),
            "landerist_infrastructure",
            "Infrastructure",
            "Backup");
        string[] forbidden = ["Config.", "AppConfig.", "LanderistSettings"];
        string[] violations = Directory.GetFiles(directory, "*.cs")
            .Where(path => forbidden.Any(token =>
                File.ReadAllText(path).Contains(token, StringComparison.Ordinal)))
            .Select(Path.GetFileName)
            .OfType<string>()
            .ToArray();
        string coordinator = File.ReadAllText(Path.Combine(
            directory,
            "SqlDatabaseBackupService.cs"));

        Assert.Empty(violations);
        Assert.Contains("IBackupStorage", coordinator);
        Assert.Contains("IBackupFileSystem", coordinator);
        Assert.Contains("TimeProvider", coordinator);
        Assert.DoesNotContain("new S3()", coordinator);
        Assert.DoesNotContain("File.", coordinator);
        Assert.DoesNotContain("Directory.", coordinator);
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
