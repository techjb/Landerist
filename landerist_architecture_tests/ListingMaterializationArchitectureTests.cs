namespace landerist_architecture_tests;

public sealed class ListingMaterializationArchitectureTests
{
    [Fact]
    public void StructuredOutputParser_DoesNotReadGlobalRulesOrClock()
    {
        string parserFile = Path.Combine(
            FindRepositoryRoot(),
            "landerist_infrastructure",
            "Infrastructure",
            "Ai",
            "StructuredOutputs",
            "StructuredOutputEsParser.cs");
        string source = File.ReadAllText(parserFile);

        Assert.DoesNotContain("Config.", source, StringComparison.Ordinal);
        Assert.DoesNotContain("AppConfig.", source, StringComparison.Ordinal);
        Assert.DoesNotContain("DateTime.Now", source, StringComparison.Ordinal);
        Assert.Contains(
            "ListingMaterializationRules",
            source,
            StringComparison.Ordinal);
        Assert.Contains("TimeProvider", source, StringComparison.Ordinal);
    }

    [Fact]
    public void StructuredOutputParser_DelegatesFieldsAndRelations()
    {
        string directory = Path.Combine(
            FindRepositoryRoot(),
            "landerist_infrastructure",
            "Infrastructure",
            "Ai",
            "StructuredOutputs");
        string parser = File.ReadAllText(Path.Combine(
            directory,
            "StructuredOutputEsParser.cs"));
        string mapper = File.ReadAllText(Path.Combine(
            directory,
            "StructuredOutputListingMapper.cs"));
        string relations = File.ReadAllText(Path.Combine(
            directory,
            "StructuredOutputListingRelations.cs"));

        Assert.Contains("StructuredOutputListingMapper", parser);
        Assert.Contains("StructuredOutputListingRelations.Attach", parser);
        Assert.DoesNotContain("new Listing", parser);
        Assert.Contains("internal Listing Create(Page page)", mapper);
        Assert.DoesNotContain("AddMediaImages", mapper);
        Assert.Contains("AddMediaImages", relations);
        Assert.Contains("listing.sources.Add", relations);
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
