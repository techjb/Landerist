namespace landerist_architecture_tests;

public sealed class WebsiteHttpArchitectureTests
{
    [Fact]
    public void WebsiteAggregate_DoesNotCreateOrMutateHttpRequests()
    {
        string websiteFile = Path.Combine(
            FindRepositoryRoot(),
            "landerist_library",
            "Websites",
            "Website.cs");
        string source = File.ReadAllText(websiteFile);
        string[] forbiddenTokens =
        [
            "HttpRequestMessage",
            "CreateHttpRequestMessage",
            "ApplyHttpRequestHeaders",
            "GetHttpRequestHeaders"
        ];

        Assert.DoesNotContain(
            forbiddenTokens,
            token => source.Contains(token, StringComparison.Ordinal));
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
