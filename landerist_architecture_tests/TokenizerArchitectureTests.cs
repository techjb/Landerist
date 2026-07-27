namespace landerist_architecture_tests;

public sealed class TokenizerArchitectureTests
{
    [Theory]
    [InlineData("Parse", "ListingParser", "Tokenizer.cs")]
    [InlineData("Infrastructure", "Tasks", "TaskLocalAIParsing.cs")]
    public void TokenizationAndLocalAiTask_DoNotReadOrMutateGlobalConfiguration(
        string firstDirectory,
        string secondDirectory,
        string fileName)
    {
        string source = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "landerist_library",
            firstDirectory,
            secondDirectory,
            fileName));

        Assert.DoesNotContain("landerist_library.Configuration", source);
        Assert.DoesNotContain("Config.", source);
        Assert.DoesNotContain("AppConfig.", source);
        Assert.DoesNotContain("SetLLMProvider", source);
        Assert.DoesNotContain("EnableLogsErrorsInConsole", source);
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
