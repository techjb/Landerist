namespace landerist_architecture_tests;

public sealed class ConsoleHostArchitectureTests
{
    [Fact]
    public void Program_UsesGenericHostForProcessLifecycle()
    {
        string program = ReadConsoleSource("Program.cs");

        Assert.Contains("Host.CreateApplicationBuilder(args)", program);
        Assert.Contains("AddHostedService<LanderistWorker>()", program);
        Assert.DoesNotContain("ManualResetEvent", program);
        Assert.DoesNotContain("Console.CancelKeyPress", program);
        Assert.DoesNotContain("Environment.Exit", program);
    }

    [Fact]
    public void Program_DelegatesObjectGraphConstructionToServiceRegistrations()
    {
        string program = ReadConsoleSource("Program.cs");
        string registrations = ReadConsoleSource(
            "LanderistServiceCollectionExtensions.cs");
        string taskRegistrations = ReadConsoleSource(
            "LanderistTaskServiceCollectionExtensions.cs");
        string recurringRegistrations = ReadConsoleSource(
            "LanderistRecurringTaskServiceCollectionExtensions.cs");

        Assert.Contains("builder.Services.AddLanderist()", program);
        Assert.Contains(".AddLanderistTasks(runtimeOptions)", registrations);
        Assert.Contains(".AddLanderistRecurringTasks(runtimeOptions)", taskRegistrations);
        Assert.Contains("AddSingleton<TasksService>", recurringRegistrations);
        Assert.False(File.Exists(GetConsolePath("LanderistServiceComposition.cs")));
        Assert.True(
            File.ReadLines(GetConsolePath("Program.cs")).Count() <= 30,
            "Program must remain a small host bootstrapper.");
    }
    [Fact]
    public void PersistenceRegistration_OwnsDatabaseFactoryConstruction()
    {
        string registrations = ReadConsoleSource(
            "LanderistPersistenceServiceCollectionExtensions.cs");
        string taskRegistrations = ReadConsoleSource(
            "LanderistTaskServiceCollectionExtensions.cs");
        string adapterFactory = ReadConsoleSource(
            "LanderistDatabaseAdapterFactory.cs");

        Assert.Contains("SqlDatabaseOptions databaseOptions = new(", registrations);
        Assert.Contains("LegacyDatabase.Configure(databaseFactory)", registrations);
        Assert.Contains("AddSingleton<IDatabaseFactory>", registrations);
        Assert.DoesNotContain("SqlDatabaseOptions databaseOptions", taskRegistrations);
        Assert.DoesNotContain("LegacyDatabase.Configure", taskRegistrations);
        Assert.DoesNotContain("new PageRepository(", taskRegistrations);
        Assert.DoesNotContain("new WebsiteRepository(", taskRegistrations);
        Assert.DoesNotContain("new ListingRepository(", taskRegistrations);
        Assert.Contains("AddTransient(_ => new PageRepository(", registrations);
        Assert.Contains("AddTransient(_ => new WebsiteRepository(", registrations);
        Assert.Contains("AddTransient(_ => new ListingRepository(", registrations);
        Assert.DoesNotContain("databaseFactory.Create()", taskRegistrations);
        Assert.Contains("IDatabaseFactory databaseFactory", adapterFactory);
        Assert.Contains("databaseFactory.Create()", adapterFactory);
        Assert.Contains("AddSingleton<PagePersistenceService>()", registrations);
        Assert.Contains("AddSingleton<WebsitePersistenceService>()", registrations);
        Assert.Contains("AddSingleton<SqlListingQueryService>()", registrations);
        Assert.Contains("AddSingleton<SqlPageCatalog>()", registrations);
        Assert.Contains("AddSingleton<SqlWebsiteCatalog>()", registrations);
        Assert.DoesNotContain("new PagePersistenceService(", taskRegistrations);
        Assert.DoesNotContain("new WebsitePersistenceService(", taskRegistrations);
        Assert.DoesNotContain("new SqlListingQueryService(", taskRegistrations);
        Assert.DoesNotContain("new WebsiteDeletionService(", taskRegistrations);
    }
    [Fact]
    public void ScrapingRegistration_DelegatesToCohesiveModules()
    {
        string coordinator = ReadConsoleSource(
            "LanderistScrapingServiceCollectionExtensions.cs");
        string infrastructure = ReadConsoleSource(
            "LanderistScrapingInfrastructureServiceCollectionExtensions.cs");
        string websites = ReadConsoleSource(
            "LanderistWebsiteScrapingServiceCollectionExtensions.cs");
        string listings = ReadConsoleSource(
            "LanderistListingScrapingServiceCollectionExtensions.cs");
        string taskRegistrations = ReadConsoleSource(
            "LanderistTaskServiceCollectionExtensions.cs");
        string pipelineFactory = ReadConsoleSource(
            "LanderistScrapingPipelineFactory.cs");

        Assert.Contains(
            ".AddLanderistScrapingInfrastructure(runtimeOptions)",
            coordinator);
        Assert.Contains(
            ".AddLanderistWebsiteScraping(runtimeOptions)",
            coordinator);
        Assert.Contains(
            ".AddLanderistListingScraping(runtimeOptions)",
            coordinator);
        Assert.True(
            File.ReadLines(GetConsolePath(
                "LanderistScrapingServiceCollectionExtensions.cs")).Count() <= 25,
            "Scraping composition coordinator must remain small.");
        Assert.DoesNotContain("AddSingleton<", coordinator);
        Assert.DoesNotContain("new HttpClientTransportFactory(", coordinator);

        Assert.Contains("HttpClientTransportFactory httpClients = new(", infrastructure);
        Assert.Contains("PuppeteerBrowserOptions browserOptions = new(", infrastructure);
        Assert.Contains("AddSingleton<ChromeMaintenanceService>", infrastructure);
        Assert.Contains("services.AddSingleton<IWebsiteRobotsPolicy>", infrastructure);
        Assert.Contains("AddSingleton<WebsiteSitemapService>", websites);
        Assert.Contains("AddSingleton<PooledPageDownloader>", websites);
        Assert.Contains("AddSingleton<ScrapeBrowserManager>", websites);
        Assert.Contains("AddSingleton<ListingLifecycleService>", listings);
        Assert.Contains("AddSingleton<LanderistScrapingPipelineFactory>", listings);

        Assert.DoesNotContain("new HttpClientTransportFactory(", taskRegistrations);
        Assert.DoesNotContain("new PuppeteerBrowserOptions(", taskRegistrations);
        Assert.Contains("new PageAcquisitionService(", pipelineFactory);
        Assert.Contains("new PageContentClassifier(", pipelineFactory);
        Assert.Contains("new PageIndexingService(", pipelineFactory);
        Assert.Contains("PageBatchSelector pageBatchSelector = new(", pipelineFactory);
    }    [Fact]
    public void LoggingAdapters_AreOwnedByInfrastructureProject()
    {
        string root = FindRepositoryRoot();

        Assert.True(File.Exists(Path.Combine(
            root,
            "landerist_infrastructure",
            "Infrastructure",
            "Logging",
            "SqlApplicationLogger.cs")));
        Assert.True(File.Exists(Path.Combine(
            root,
            "landerist_infrastructure",
            "Infrastructure",
            "Logging",
            "ConsoleScrapeProgressReporter.cs")));
        Assert.False(Directory.Exists(Path.Combine(
            root,
            "landerist_library",
            "Infrastructure",
            "Logging")));
        string logPath = Path.Combine(
            root,
            "landerist_infrastructure",
            "Compatibility",
            "LegacyLogging",
            "Log.cs");
        Assert.True(File.Exists(logPath));
        Assert.False(Directory.Exists(Path.Combine(
            root,
            "landerist_library",
            "Logs")));
        string logSource = File.ReadAllText(logPath);
        Assert.DoesNotContain("Config.", logSource);
        Assert.DoesNotContain("LegacyDatabase", logSource);
    }
    [Fact]
    public void Downloaders_AreOwnedByInfrastructureProject()
    {
        string root = FindRepositoryRoot();
        string infrastructureDownloaders = Path.Combine(
            root,
            "landerist_infrastructure",
            "Infrastructure",
            "Downloaders");

        Assert.True(Directory.Exists(infrastructureDownloaders));
        Assert.True(File.Exists(Path.Combine(
            infrastructureDownloaders,
            "Puppeteer",
            "PuppeteerDownloader.cs")));
        Assert.True(File.Exists(Path.Combine(
            infrastructureDownloaders,
            "Multiple",
            "DownloadersPool.cs")));
        Assert.False(Directory.Exists(Path.Combine(
            root,
            "landerist_library",
            "Downloaders")));
        Assert.False(Directory.Exists(Path.Combine(
            root,
            "landerist_library",
            "Infrastructure",
            "Downloaders")));
    }
    [Fact]
    public void ConsoleConfigurationGlobals_AreConfinedToLegacyAdapter()
    {
        string consoleDirectory = Path.Combine(FindRepositoryRoot(), "landerist_console");
        string[] forbiddenTokens =
        [
            "landerist_library.Configuration",
            "Config.",
            "LanderistSettings"
        ];

        foreach (string path in Directory.EnumerateFiles(consoleDirectory, "*.cs"))
        {
            if (Path.GetFileName(path) == "LanderistRuntimeOptionsAdapter.cs")
            {
                continue;
            }

            string source = File.ReadAllText(path);
            foreach (string token in forbiddenTokens)
            {
                Assert.DoesNotContain(token, source, StringComparison.Ordinal);
            }
        }
    }
    [Fact]
    public void TaskRegistration_DelegatesSpecializedObjectGraphsToModules()
    {
        string coordinator = ReadConsoleSource(
            "LanderistTaskServiceCollectionExtensions.cs");
        string parsing = ReadConsoleSource(
            "LanderistParsingTaskServiceCollectionExtensions.cs");
        string scraping = ReadConsoleSource(
            "LanderistScrapingTaskServiceCollectionExtensions.cs");
        string localAi = ReadConsoleSource(
            "LanderistLocalAiTaskServiceCollectionExtensions.cs");
        string recurring = ReadConsoleSource(
            "LanderistRecurringTaskServiceCollectionExtensions.cs");

        Assert.Contains(".AddLanderistParsingTasks()", coordinator);
        Assert.Contains(".AddLanderistScrapingTasks()", coordinator);
        Assert.Contains(".AddLanderistLocalAiTasks(runtimeOptions)", coordinator);
        Assert.Contains(".AddLanderistRecurringTasks(runtimeOptions)", coordinator);
        Assert.True(
            File.ReadLines(GetConsolePath(
                "LanderistTaskServiceCollectionExtensions.cs")).Count() <= 25,
            "Task composition coordinator must remain small.");

        Assert.Contains("AddSingleton<LanderistAiComposition>()", parsing);
        Assert.Contains("AddSingleton<ParseListing>", parsing);
        Assert.Contains("AddSingleton<LanderistBatchProviderComposition>()", scraping);
        Assert.Contains("AddSingleton<LanderistBatchComposition>()", scraping);
        Assert.Contains("AddSingleton<LanderistScrapingPipeline>", scraping);
        Assert.Contains("AddSingleton<ScrapeTaskJob>", scraping);
        Assert.Contains("AddSingleton<LocalAiTaskJob>", localAi);
        Assert.Contains("AddSingleton<LanderistDistributionComposition>()", recurring);
        Assert.Contains("AddSingleton<HourlyTaskJob>", recurring);
        Assert.Contains("AddSingleton<DailyTaskJob>", recurring);
        Assert.Contains("AddSingleton<TasksService>", recurring);

        Assert.DoesNotContain("AddSingleton<", coordinator);
        Assert.DoesNotContain("OpenAIListingParserClient", coordinator);
        Assert.DoesNotContain("TaskBatchUpload", coordinator);
        Assert.DoesNotContain("DistributionPublisher", coordinator);
    }    [Fact]
    public void BatchComposition_SeparatesProviderConfigurationFromJobAssembly()
    {
        string jobs = ReadConsoleSource("LanderistBatchComposition.cs");
        string providers = ReadConsoleSource(
            "LanderistBatchProviderComposition.cs");

        Assert.Contains("LanderistBatchProviderComposition", jobs);
        Assert.Contains("providerComposition.Create()", jobs);
        Assert.DoesNotContain("OpenAIBatchClient", jobs);
        Assert.DoesNotContain("VertexBatchJobClient", jobs);
        Assert.DoesNotContain("StructuredOutputSchema", jobs);

        Assert.Contains("OpenAIBatchClient", providers);
        Assert.Contains("VertexBatchJobClient", providers);
        Assert.Contains("ListingBatchUploadProviderCatalog", providers);
        Assert.Contains("BatchDownloadProviderCatalog", providers);
        Assert.Contains("IBatchArtifactCleaner", providers);
    }

    [Fact]
    public void DistributionComposition_DependsOnExplicitAdministrationPort()
    {
        string persistence = ReadConsoleSource(
            "LanderistPersistenceServiceCollectionExtensions.cs");
        string distribution = ReadConsoleSource(
            "LanderistDistributionComposition.cs");

        Assert.Contains(
            "AddSingleton<IListingAdministrationService,",
            persistence);
        Assert.Contains(
            "IListingAdministrationService listingAdministration",
            distribution);
        Assert.DoesNotContain("IServiceProvider", distribution);
        Assert.DoesNotContain("GetRequiredService", distribution);
        Assert.DoesNotContain("ListingRepository", distribution);
        Assert.DoesNotContain("MediaRepository", distribution);
        Assert.DoesNotContain("SourceRepository", distribution);
        Assert.DoesNotContain(
            "new SqlListingAdministrationService",
            distribution);
    }

    [Fact]
    public void AddressSelectionOptions_AreOwnedByScrapingComposition()
    {
        string parsing = ReadConsoleSource("LanderistAiComposition.cs");
        string scraping = ReadConsoleSource(
            "LanderistListingScrapingServiceCollectionExtensions.cs");

        Assert.DoesNotContain("CreateAddressSelectorOptions", parsing);
        Assert.DoesNotContain("VertexAddressSelectorOptions", parsing);
        Assert.Contains(
            "services.AddSingleton(new VertexAddressSelectorOptions(",
            scraping);
        Assert.Contains(
            "GetRequiredService<VertexAddressSelectorOptions>()",
            scraping);
        Assert.DoesNotContain(
            "GetRequiredService<LanderistAiComposition>()",
            scraping);
    }

    private static string ReadConsoleSource(string fileName) =>
        File.ReadAllText(GetConsolePath(fileName));

    private static string GetConsolePath(string fileName) =>
        Path.Combine(FindRepositoryRoot(), "landerist_console", fileName);

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