namespace landerist_architecture_tests;

public sealed class ListingMaterializationArchitectureTests
{
    [Fact]
    public void StructuredOutputParser_DoesNotReadGlobalRulesOrClock()
    {
        string parserFile = Path.Combine(
            FindRepositoryRoot(),
            "landerist_library",
            "Parse",
            "ListingParser",
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
