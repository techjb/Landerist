namespace landerist_architecture_tests;

public sealed class BatchUploadArchitectureTests
{
    [Fact]
    public void TaskBatchUpload_DependsOnOptionsAndProviderPort()
    {
        string source = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "landerist_infrastructure",
            "Infrastructure",
            "Tasks",
            "TaskBatchUpload.cs"));

        string[] forbiddenTokens =
        [
            "landerist_library.Configuration",
            "Config.",
            "AppConfig.",
            "TaskLocalAIParsing.GetMaxTokenCount",
            "OpenAIBatch",
            "VertexAIBatch",
            "CloudStorage.",
            "BatchPredictions.",
            "static bool _firstTime"
        ];

        Assert.DoesNotContain(
            forbiddenTokens,
            token => source.Contains(token, StringComparison.Ordinal));
        Assert.Contains("BatchUploadOptions", source, StringComparison.Ordinal);
        Assert.Contains(
            "IListingBatchUploadProvider",
            source,
            StringComparison.Ordinal);
    }

    [Fact]
    public void JsonlWriter_UsesListingInputPort()
    {
        string source = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "landerist_infrastructure",
            "Infrastructure",
            "Parsing",
            "JsonlBatchInputWriter.cs"));

        Assert.Contains("IListingInputPreparer", source, StringComparison.Ordinal);
        Assert.DoesNotContain("GetListingParserInput", source, StringComparison.Ordinal);
        Assert.DoesNotContain("PageListingInputExtensions", source, StringComparison.Ordinal);
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
