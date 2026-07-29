namespace landerist_architecture_tests;

public sealed class ListingParserOrchestrationArchitectureTests
{
    [Fact]
    public void ParseListing_DoesNotUseGlobalOptionsOrStaticClientCatalog()
    {
        string parserFile = Path.Combine(
            FindRepositoryRoot(),
            "landerist_infrastructure",
            "Infrastructure",
            "Parsing",
            "ParseListing.cs");
        string source = File.ReadAllText(parserFile);
        string[] forbiddenTokens =
        [
            "Config.",
            "AppConfig.",
            "static readonly Dictionary<LLMProvider",
            "new OpenAIListingParserClient",
            "new VertexAIListingParserClient",
            "new LocalAIListingParserClient"
        ];

        Assert.DoesNotContain(
            forbiddenTokens,
            token => source.Contains(token, StringComparison.Ordinal));
        Assert.Contains(
            "ListingParserOrchestrationOptions",
            source,
            StringComparison.Ordinal);
        Assert.Contains(
            "ListingParserClientCatalog",
            source,
            StringComparison.Ordinal);
    }

    [Fact]
    public void BatchDownload_PassesBatchProviderToParser()
    {
        string taskFile = Path.Combine(
            FindRepositoryRoot(),
            "landerist_infrastructure",
            "Infrastructure",
            "Tasks",
            "TaskBatchDownload.cs");
        string source = File.ReadAllText(taskFile);

        Assert.Contains(
            "_listingParser.Parse(page, text, batch.Provider)",
            source,
            StringComparison.Ordinal);
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
