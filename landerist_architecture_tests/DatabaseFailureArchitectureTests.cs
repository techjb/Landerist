namespace landerist_architecture_tests;

public sealed class DatabaseFailureArchitectureTests
{
    [Fact]
    public void DatabaseExecutor_OnlyReturnsFallbackForExplicitExceptionProbe()
    {
        string source = File.ReadAllText(GetDatabasePath("DataBase.cs"));

        Assert.Contains("throw new DatabaseOperationException(operationName, ex)", source);
        Assert.Contains("bool returnFailureResult = false", source);
        Assert.Contains("returnFailureResult: true", source);
        Assert.Equal(1, CountOccurrences(source, "return failureResult;"));
    }

    [Fact]
    public void DatabaseOperationException_DoesNotCaptureQueryText()
    {
        string source = File.ReadAllText(
            GetDatabasePath("DatabaseOperationException.cs"));

        Assert.Contains("string operationName", source);
        Assert.DoesNotContain("string query", source);
        Assert.Contains("OperationName = operationName", source);
    }

    private static int CountOccurrences(string source, string value) =>
        source.Split(value, StringSplitOptions.None).Length - 1;

    private static string GetDatabasePath(string fileName) =>
        Path.Combine(FindRepositoryRoot(), "landerist_infrastructure", "Database", fileName);

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