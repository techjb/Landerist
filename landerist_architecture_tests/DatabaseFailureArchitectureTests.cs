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
        Assert.Contains("Task<bool> QueryBoolAsync(", contract);
        Assert.Contains("connection.OpenAsync(cancellationToken)", source);
        Assert.Contains("command.ExecuteNonQueryAsync(token)", source);
        Assert.Contains("ExecuteScalarAsync(token)", source);
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
    [Fact]
    public void ScheduledScraping_PropagatesAsyncExecutionToDatabase()
    {
        string root = FindRepositoryRoot();
        string tasks = File.ReadAllText(
            Path.Combine(root, "landerist_application", "Application", "Tasks", "TasksService.cs"));
        string job = File.ReadAllText(
            Path.Combine(root, "landerist_infrastructure", "Infrastructure", "Tasks", "ScrapeTaskJob.cs"));
        string scraper = File.ReadAllText(
            Path.Combine(root, "landerist_application", "Application", "Scraping", "Scraper.cs"));
        string throttle = File.ReadAllText(
            Path.Combine(root, "landerist_infrastructure", "Infrastructure", "Scraping", "WebsitesThrottle.cs"));

        Assert.Contains("AddAsyncSchedule(", tasks);
        Assert.Contains("_scrapeJob.RunAsync", tasks);
        Assert.Contains("_scraper.RunBatchAsync(cancellationToken)", job);
        Assert.Contains(".CleanAsync(linkedCancellation.Token)", scraper);
        Assert.Contains("Parallel.ForEachAsync(", scraper);
        Assert.Contains(".IsBlockedAsync(page.Website, cancellationToken)", scraper);
        Assert.Contains(".TryAcquireAsync(page.Website, cancellationToken)", scraper);
        Assert.Contains(".ReportForbiddenAsync(page.Website, cancellationToken)", scraper);
        Assert.Contains(".ReportSuccessAsync(page.Website, cancellationToken)", scraper);
        Assert.Contains("_database.QueryAsync(", throttle);
        Assert.Contains("_database.QueryBoolAsync(", throttle);
    }
    [Fact]
    public void AsyncScraping_PropagatesCancellationToConditionalHttpRequest()
    {
        string root = FindRepositoryRoot();
        string scraper = File.ReadAllText(
            Path.Combine(root, "landerist_application", "Application", "Scraping", "Scraper.cs"));
        string pageScraper = File.ReadAllText(
            Path.Combine(root, "landerist_application", "Application", "Scraping", "PageScraper.cs"));
        string acquisition = File.ReadAllText(
            Path.Combine(root, "landerist_application", "Application", "Scraping", "PageAcquisitionService.cs"));
        string checker = File.ReadAllText(
            Path.Combine(root, "landerist_infrastructure", "Infrastructure", "Scraping", "ConditionalPageHeaderChecker.cs"));

        Assert.Contains("pageScraper.ScrapeAsync(cancellationToken)", scraper);
        Assert.Contains(".AcquireAsync(_page, _useProxy, cancellationToken)", pageScraper);
        Assert.Contains(".CheckAsync(page, useProxy, cancellationToken)", acquisition);
        Assert.Contains("HttpCompletionOption.ResponseHeadersRead,", checker);
        Assert.Contains("cancellationToken)", checker);
        Assert.DoesNotContain("CheckAsync(page).GetAwaiter().GetResult()", checker);
    }
    [Fact]
    public void AsyncPageDownload_PropagatesCancellationToPuppeteerSession()
    {
        string root = FindRepositoryRoot();
        string acquisition = File.ReadAllText(
            Path.Combine(root, "landerist_application", "Application", "Scraping", "PageAcquisitionService.cs"));
        string pooled = File.ReadAllText(
            Path.Combine(root, "landerist_infrastructure", "Infrastructure", "Scraping", "PooledPageDownloader.cs"));
        string pool = File.ReadAllText(
            Path.Combine(root, "landerist_infrastructure", "Infrastructure", "Downloaders", "Multiple", "DownloadersPool.cs"));
        string single = File.ReadAllText(
            Path.Combine(root, "landerist_infrastructure", "Infrastructure", "Downloaders", "Multiple", "SingleDownloader.cs"));
        string puppeteer = File.ReadAllText(
            Path.Combine(root, "landerist_infrastructure", "Infrastructure", "Downloaders", "Puppeteer", "PuppeteerDownloader.cs"));

        Assert.Contains(".DownloadAsync(page, useProxy, cancellationToken)", acquisition);
        Assert.Contains("pool.DownloadAsync(page, useProxy, cancellationToken)", pooled);
        Assert.Contains(".DownloadAsync(page, cancellationToken)", pool);
        Assert.Contains("Downloader.DownloadAsync(page, cancellationToken)", single);
        Assert.Contains("await Task.WhenAny(download, timeout)", puppeteer);
        Assert.Contains("Task.Delay(delay + 1000, cancellationToken)", puppeteer);
        Assert.Contains("await ClosePageAsync()", puppeteer);
    }
    [Fact]
    public void AsyncPuppeteerLifecycle_DoesNotLeakCancelledBrowserLaunches()
    {
        string root = FindRepositoryRoot();
        string pool = File.ReadAllText(
            Path.Combine(root, "landerist_infrastructure", "Infrastructure", "Downloaders", "Multiple", "DownloadersPool.cs"));
        string single = File.ReadAllText(
            Path.Combine(root, "landerist_infrastructure", "Infrastructure", "Downloaders", "Multiple", "SingleDownloader.cs"));
        string puppeteer = File.ReadAllText(
            Path.Combine(root, "landerist_infrastructure", "Infrastructure", "Downloaders", "Puppeteer", "PuppeteerDownloader.cs"));
        string scraper = File.ReadAllText(
            Path.Combine(root, "landerist_application", "Application", "Scraping", "Scraper.cs"));

        Assert.Contains("SingleDownloader.CreateAsync(", pool);
        Assert.Contains("Downloaders.Count + CreatingDownloaders", pool);
        Assert.Contains("Generation++", pool);
        Assert.Contains("downloader.CloseBrowserAsync()", pool);
        Assert.Contains("RestartBrowserAsync(cancellationToken)", single);
        Assert.Contains("CloseCancelledLaunchAsync(launch)", puppeteer);
        Assert.Contains("await browser.CloseAsync()", puppeteer);
        Assert.Contains(".ClearDownloadersAsync(cancellationToken)", scraper);
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