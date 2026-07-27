namespace landerist_architecture_tests;

public sealed class ApplicationParsingArchitectureTests
{
    [Fact]
    public void PageTypeParser_UsesContentInspectionPortInsteadOfHtmlExtensions()
    {
        string source = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "landerist_application",
            "Application",
            "Parsing",
            "PageTypeParser.cs"));
        string[] forbiddenCalls =
        [
            "Page.ContainsMetaRobotsNoIndex(",
            "Page.NotCanonical(",
            "Page.IncorrectLanguage(",
            "Page.SetListingParserInput(",
            "Page.MatchesWebsiteListingUnavailableRule("
        ];

        Assert.DoesNotContain(
            forbiddenCalls,
            call => source.Contains(call, StringComparison.Ordinal));
    }

    [Fact]
    public void ListingLifecycle_UsesContentInspectionPortForCanonicalUri()
    {
        string source = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "landerist_application",
            "Application",
            "Listings",
            "ListingLifecycleService.cs"));

        Assert.DoesNotContain("page.GetCanonicalUri(", source, StringComparison.Ordinal);
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