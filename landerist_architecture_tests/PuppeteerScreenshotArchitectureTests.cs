namespace landerist_architecture_tests;

public sealed class PuppeteerScreenshotArchitectureTests
{
    [Fact]
    public void ScreenshotProcessing_DoesNotReadGlobalConfigurationOrWriteFiles()
    {
        string file = Path.Combine(
            FindRepositoryRoot(),
            "landerist_infrastructure",
            "Infrastructure",
            "Downloaders",
            "Puppeteer",
            "PuppeteerScreenshot.cs");
        string source = File.ReadAllText(file);
        string[] forbiddenTokens =
        [
            "Config.",
            "AppConfig.",
            "File.Write",
            "Directory.Create"
        ];

        Assert.DoesNotContain(
            forbiddenTokens,
            token => source.Contains(token, StringComparison.Ordinal));
    }

    [Fact]
    public void FileScreenshotStore_LivesInInfrastructure()
    {
        string repositoryRoot = FindRepositoryRoot();

        Assert.True(File.Exists(Path.Combine(
            repositoryRoot,
            "landerist_infrastructure",
            "Infrastructure",
            "Files",
            "FileScreenshotStore.cs")));
        Assert.False(File.Exists(Path.Combine(
            repositoryRoot,
            "landerist_infrastructure",
            "Infrastructure",
            "Downloaders",
            "Puppeteer",
            "FileScreenshotStore.cs")));
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
