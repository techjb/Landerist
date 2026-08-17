namespace landerist_architecture_tests;

public sealed class DatabaseFailureArchitectureTests
{
    [Fact]
    public void DatabaseExecutor_OnlyReturnsFallbackForExplicitExceptionProbe()
    {
        string facade = File.ReadAllText(GetDatabasePath("DataBase.cs"));
        string executor = File.ReadAllText(GetDatabasePath("SqlCommandExecutor.cs"));

        Assert.Contains("throw new DatabaseOperationException(operationName, ex)", executor);
        Assert.Contains("bool returnFailureResult = false", executor);
        Assert.Contains("returnFailureResult: true", facade);
        Assert.Equal(1, CountOccurrences(executor, "return failureResult;"));
        Assert.DoesNotContain("new SqlConnection", facade);
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
        string facade = File.ReadAllText(GetDatabasePath("DataBase.cs"));
        string executor = File.ReadAllText(GetDatabasePath("SqlCommandExecutor.cs"));
        string mapper = File.ReadAllText(GetDatabasePath("SqlDataReaderMapper.cs"));
        string contract = File.ReadAllText(GetDatabasePath("IDatabase.cs"));

        Assert.Contains("Task<bool> QueryAsync(", contract);
        Assert.Contains("Task<bool> QueryBoolAsync(", contract);
        Assert.Contains("connection.OpenAsync(cancellationToken)", executor);
        Assert.Contains("command.ExecuteNonQueryAsync(token)", facade);
        Assert.Contains("ExecuteScalarAsync(token)", facade);
        Assert.Contains("ExecuteReaderAsync(cancellationToken)", mapper);
        Assert.Contains("reader.ReadAsync(cancellationToken)", mapper);
        Assert.Contains("catch (OperationCanceledException)", executor);
        Assert.DoesNotContain("Task.FromResult", facade);
        Assert.DoesNotContain("Task.FromResult", executor);
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
        string pageProcessor = File.ReadAllText(
            Path.Combine(root, "landerist_application", "Application", "Scraping", "ScrapePageProcessor.cs"));
        string throttle = File.ReadAllText(
            Path.Combine(root, "landerist_infrastructure", "Infrastructure", "Scraping", "WebsitesThrottle.cs"));

        Assert.Contains("AddAsyncSchedule(", tasks);
        Assert.Contains("_scrapeJob.RunAsync", tasks);
        Assert.Contains("_scraper.RunBatchAsync(cancellationToken)", job);
        Assert.Contains(".CleanAsync(linkedCancellation.Token)", scraper);
        Assert.Contains("Parallel.ForEachAsync(", scraper);
        Assert.Contains("_pageProcessor.ProcessAsync(page, token)", scraper);
        Assert.Contains(".IsBlockedAsync(page.Website, cancellationToken)", pageProcessor);
        Assert.Contains(".TryAcquireAsync(page.Website, cancellationToken)", pageProcessor);
        Assert.Contains(".ReportForbiddenAsync(page.Website, cancellationToken)", pageProcessor);
        Assert.Contains(".ReportSuccessAsync(page.Website, cancellationToken)", pageProcessor);
        Assert.Contains("_database.QueryAsync(", throttle);
        Assert.Contains("_database.QueryBoolAsync(", throttle);
    }
    [Fact]
    public void AsyncScraping_PropagatesCancellationToConditionalHttpRequest()
    {
        string root = FindRepositoryRoot();
        string pageProcessor = File.ReadAllText(
            Path.Combine(root, "landerist_application", "Application", "Scraping", "ScrapePageProcessor.cs"));
        string pageScraper = File.ReadAllText(
            Path.Combine(root, "landerist_application", "Application", "Scraping", "PageScraper.cs"));
        string acquisition = File.ReadAllText(
            Path.Combine(root, "landerist_application", "Application", "Scraping", "PageAcquisitionService.cs"));
        string checker = File.ReadAllText(
            Path.Combine(root, "landerist_infrastructure", "Infrastructure", "Scraping", "ConditionalPageHeaderChecker.cs"));

        Assert.Contains(".ScrapeAsync(cancellationToken)", pageProcessor);
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
        string execution = File.ReadAllText(
            Path.Combine(root, "landerist_infrastructure", "Infrastructure", "Downloaders", "Puppeteer", "PuppeteerDownloadExecution.cs"));

        Assert.Contains(".DownloadAsync(page, useProxy, cancellationToken)", acquisition);
        Assert.Contains("pool.DownloadAsync(page, useProxy, cancellationToken)", pooled);
        Assert.Contains(".DownloadAsync(page, cancellationToken)", pool);
        Assert.Contains("Downloader.DownloadAsync(page, cancellationToken)", single);
        Assert.Contains("PuppeteerDownloadExecution.WaitAsync(", puppeteer);
        Assert.Contains("Task.WhenAny(download, timeout)", execution);
        Assert.Contains("Task.Delay(timeoutMilliseconds, cancellationToken)", execution);
        Assert.Contains("await closePageAsync()", execution);
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
        string lifecycle = File.ReadAllText(
            Path.Combine(root, "landerist_infrastructure", "Infrastructure", "Downloaders", "Puppeteer", "PuppeteerBrowserLifecycle.cs"));
        string scraper = File.ReadAllText(
            Path.Combine(root, "landerist_application", "Application", "Scraping", "Scraper.cs"));

        Assert.Contains("SingleDownloader.CreateAsync(", pool);
        Assert.Contains("Downloaders.Count + CreatingDownloaders", pool);
        Assert.Contains("Generation++", pool);
        Assert.Contains("downloader.CloseBrowserAsync()", pool);
        Assert.Contains("RestartBrowserAsync(cancellationToken)", single);
        Assert.Contains("PuppeteerBrowserLifecycle.LaunchAsync(", puppeteer);
        Assert.Contains("CloseCancelledLaunchAsync(launch)", lifecycle);
        Assert.Contains("await browser.CloseAsync()", lifecycle);
        Assert.Contains(".ClearDownloadersAsync(cancellationToken)", scraper);
    }
    [Fact]
    public void AsyncPageClassification_PropagatesPersistenceToQueryAsync()
    {
        string root = FindRepositoryRoot();
        string pageProcessor = File.ReadAllText(
            Path.Combine(root, "landerist_application", "Application", "Scraping", "ScrapePageProcessor.cs"));
        string pageScraper = File.ReadAllText(
            Path.Combine(root, "landerist_application", "Application", "Scraping", "PageScraper.cs"));
        string classification = File.ReadAllText(
            Path.Combine(root, "landerist_application", "Application", "Scraping", "PageClassificationService.cs"));
        string persistence = File.ReadAllText(
            Path.Combine(root, "landerist_application", "Application", "Persistence", "PagePersistenceService.cs"));
        string repository = File.ReadAllText(
            Path.Combine(root, "landerist_infrastructure", "Infrastructure", "Sql", "PageRepository.cs"));

        Assert.Contains("TryApplyPreClassificationBeforeDownloadAsync(", pageProcessor);
        Assert.Contains("ProcessAcquisitionResultAsync(status, cancellationToken)", pageScraper);
        Assert.Contains(".UpdateAsync(page, cancellationToken)", classification);
        Assert.Contains(".UpdateAsync(page, cancellationToken)", persistence);
        Assert.Contains("_database.QueryAsync(", repository);
    }
    [Fact]
    public void AsyncNotListingLifecycle_PropagatesCacheInsertToQueryAsync()
    {
        string root = FindRepositoryRoot();
        string classification = File.ReadAllText(
            Path.Combine(root, "landerist_application", "Application", "Scraping", "PageClassificationService.cs"));
        string lifecycle = File.ReadAllText(
            Path.Combine(root, "landerist_application", "Application", "Listings", "ListingLifecycleService.cs"));
        string cache = File.ReadAllText(
            Path.Combine(root, "landerist_infrastructure", "Infrastructure", "Listings", "SqlNotListingCacheService.cs"));

        Assert.Contains(".ApplyAsync(page, newListing, cancellationToken)", classification);
        Assert.Contains(".InsertAsync(page, cancellationToken)", lifecycle);
        Assert.Contains("_database.QueryAsync(", cache);
    }
    [Fact]
    public void AsyncListingLifecycle_LoadsAggregateThroughAsyncTableQueries()
    {
        string root = FindRepositoryRoot();
        string lifecycle = File.ReadAllText(
            Path.Combine(root, "landerist_application", "Application", "Listings", "ListingLifecycleService.cs"));
        string store = File.ReadAllText(
            Path.Combine(root, "landerist_infrastructure", "Infrastructure", "Listings", "SqlListingStore.cs"));
        string queries = File.ReadAllText(
            Path.Combine(root, "landerist_infrastructure", "Infrastructure", "Listings", "SqlListingQueryService.cs"));
        string listingRepository = File.ReadAllText(
            Path.Combine(root, "landerist_infrastructure", "Infrastructure", "Sql", "ListingQueryRepository.cs"));
        string mediaRepository = File.ReadAllText(
            Path.Combine(root, "landerist_infrastructure", "Infrastructure", "Sql", "MediaRepository.cs"));
        string sourceRepository = File.ReadAllText(
            Path.Combine(root, "landerist_infrastructure", "Infrastructure", "Sql", "SourceRepository.cs"));

        Assert.Contains("_listingStore.GetAsync(", lifecycle);
        Assert.Contains("_queries.GetAsync(", store);
        Assert.Contains(".GetListingAsync(page.UriHash, cancellationToken)", queries);
        Assert.Contains("_media.GetMediaAsync(listing, cancellationToken)", queries);
        Assert.Contains("_sources.GetSourcesAsync(listing, cancellationToken)", queries);
        Assert.Contains("Database.QueryTableAsync(", listingRepository);
        Assert.Contains("Database.QueryTableAsync(", mediaRepository);
        Assert.Contains("Database.QueryTableAsync(", sourceRepository);
    }
    [Fact]
    public void AsyncListingLifecycle_PersistsAggregateThroughAsyncQueries()
    {
        string root = FindRepositoryRoot();
        string lifecycle = File.ReadAllText(Path.Combine(root, "landerist_application", "Application", "Listings", "ListingLifecycleService.cs"));
        string store = File.ReadAllText(Path.Combine(root, "landerist_infrastructure", "Infrastructure", "Listings", "SqlListingStore.cs"));
        string listingRepository = File.ReadAllText(Path.Combine(root, "landerist_infrastructure", "Infrastructure", "Sql", "ListingRepository.cs"));
        string mediaRepository = File.ReadAllText(Path.Combine(root, "landerist_infrastructure", "Infrastructure", "Sql", "MediaRepository.cs"));
        string sourceRepository = File.ReadAllText(Path.Combine(root, "landerist_infrastructure", "Infrastructure", "Sql", "SourceRepository.cs"));
        string statisticsRepository = File.ReadAllText(Path.Combine(root, "landerist_infrastructure", "Infrastructure", "Statistics", "GlobalStatisticsRepository.cs"));

        Assert.Contains("_listingStore.UpsertAsync(", lifecycle);
        Assert.Contains("_listings.InsertAsync(", store);
        Assert.Contains("_listings.UpdateAsync(", store);
        Assert.Contains("_media.InsertAsync(", store);
        Assert.Contains("_sources.InsertAsync(", store);
        Assert.Contains("_statistics.InsertDailyCounterAsync(", store);
        Assert.Contains("Database.QueryAsync(", listingRepository);
        Assert.Contains("Database.QueryAsync(", mediaRepository);
        Assert.Contains("Database.QueryAsync(", sourceRepository);
        Assert.Contains("Database.QueryAsync(", statisticsRepository);
    }

    [Fact]
    public void AsyncListingAdministration_PropagatesMaintenanceWritesToQueryAsync()
    {
        string root = FindRepositoryRoot();
        string contract = File.ReadAllText(Path.Combine(root, "landerist_application", "Application", "Listings", "IListingQueryService.cs"));
        string maintenance = File.ReadAllText(Path.Combine(root, "landerist_infrastructure", "Infrastructure", "Listings", "SqlListingMaintenanceService.cs"));
        string repository = File.ReadAllText(Path.Combine(root, "landerist_infrastructure", "Infrastructure", "Sql", "ListingRepository.cs"));

        Assert.Contains("DeleteAsync(string guid", contract);
        Assert.Contains("DeleteAllAsync(", contract);
        Assert.Contains("_listings.DeleteAsync(", maintenance);
        Assert.Contains("_media.DeleteAsync(", maintenance);
        Assert.Contains("_sources.DeleteAsync(", maintenance);
        Assert.Contains("Database.QueryAsync(", repository);
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
