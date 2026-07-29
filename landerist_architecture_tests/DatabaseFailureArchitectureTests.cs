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

    [Fact]
    public void DatabaseAsyncExecution_UsesAsyncIoAndPropagatesCancellation()
    {
        string source = File.ReadAllText(GetDatabasePath("DataBase.cs"));
        string contract = File.ReadAllText(GetDatabasePath("IDatabase.cs"));

        Assert.Contains("Task<bool> QueryAsync(", contract);
        Assert.Contains("connection.OpenAsync(cancellationToken)", source);
        Assert.Contains("command.ExecuteNonQueryAsync(token)", source);
        Assert.Contains("catch (OperationCanceledException)", source);
        Assert.DoesNotContain("Task.FromResult", source);
    }
    [Fact]
    public void HostShutdown_PropagatesAsyncCleanupToPageLocks()
    {
        string root = FindRepositoryRoot();
        string worker = File.ReadAllText(
            Path.Combine(root, "landerist_console", "LanderistWorker.cs"));
        string tasks = File.ReadAllText(
            Path.Combine(root, "landerist_application", "Application", "Tasks", "TasksService.cs"));
        string job = File.ReadAllText(
            Path.Combine(root, "landerist_infrastructure", "Infrastructure", "Tasks", "ScrapeTaskJob.cs"));
        string scraper = File.ReadAllText(
            Path.Combine(root, "landerist_application", "Application", "Scraping", "Scraper.cs"));

        Assert.Contains("await _tasks.StopAsync(cancellationToken)", worker);
        Assert.Contains("await _scrapeJob.StopAsync(cancellationToken)", tasks);
        Assert.Contains("_scraper.StopAsync(cancellationToken)", job);
        Assert.Contains(".CleanPageLocksAsync(cancellationToken)", scraper);
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