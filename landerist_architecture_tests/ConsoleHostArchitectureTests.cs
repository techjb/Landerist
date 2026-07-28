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
    public void Program_DelegatesObjectGraphConstructionToCompositionRoot()
    {
        string program = ReadConsoleSource("Program.cs");
        string registrations = ReadConsoleSource(
            "LanderistServiceCollectionExtensions.cs");
        string composition = ReadConsoleSource("LanderistServiceComposition.cs");

        Assert.Contains("builder.Services.AddLanderist()", program);
        Assert.Contains(
            "LanderistServiceComposition.CreateTasksService(",
            registrations);
        Assert.Contains("CreateTasksService(", composition);
        Assert.True(
            File.ReadLines(GetConsolePath("Program.cs")).Count() <= 30,
            "Program must remain a small host bootstrapper.");
    }

    [Fact]
    public void PersistenceRegistration_OwnsDatabaseFactoryConstruction()
    {
        string registrations = ReadConsoleSource(
            "LanderistPersistenceServiceCollectionExtensions.cs");
        string composition = ReadConsoleSource("LanderistServiceComposition.cs");
        string adapterFactory = ReadConsoleSource(
            "LanderistDatabaseAdapterFactory.cs");

        Assert.Contains("SqlDatabaseOptions databaseOptions = new(", registrations);
        Assert.Contains("LegacyDatabase.Configure(databaseFactory)", registrations);
        Assert.Contains("AddSingleton<IDatabaseFactory>", registrations);
        Assert.DoesNotContain("SqlDatabaseOptions databaseOptions", composition);
        Assert.DoesNotContain("LegacyDatabase.Configure", composition);
        Assert.DoesNotContain("new PageRepository(", composition);
        Assert.DoesNotContain("new WebsiteRepository(", composition);
        Assert.DoesNotContain("new ListingRepository(", composition);
        Assert.Contains("AddTransient(_ => new PageRepository(", registrations);
        Assert.Contains("AddTransient(_ => new WebsiteRepository(", registrations);
        Assert.Contains("AddTransient(_ => new ListingRepository(", registrations);
        Assert.DoesNotContain("databaseFactory.Create()", composition);
        Assert.Contains("IDatabaseFactory databaseFactory", adapterFactory);
        Assert.Contains("databaseFactory.Create()", adapterFactory);
    }
    [Fact]
    public void ScrapingRegistration_OwnsSharedBrowserInfrastructure()
    {
        string registrations = ReadConsoleSource(
            "LanderistScrapingServiceCollectionExtensions.cs");
        string composition = ReadConsoleSource("LanderistServiceComposition.cs");
        string pipelineFactory = ReadConsoleSource(
            "LanderistScrapingPipelineFactory.cs");

        Assert.Contains("HttpClientTransportFactory httpClients = new(", registrations);
        Assert.Contains("PuppeteerBrowserOptions browserOptions = new(", registrations);
        Assert.Contains("AddSingleton<ChromeMaintenanceService>", registrations);
        Assert.Contains("services.AddSingleton<IWebsiteRobotsPolicy>", registrations);
        Assert.DoesNotContain("new HttpClientTransportFactory(", composition);
        Assert.DoesNotContain("new PuppeteerBrowserOptions(", composition);
        Assert.DoesNotContain("new ChromeMaintenanceService(", composition);
        Assert.DoesNotContain("WebsiteRobotsPolicy robotsPolicy = new", composition);
        Assert.Contains("new PageAcquisitionService(", pipelineFactory);
        Assert.Contains("new PageContentClassifier(", pipelineFactory);
        Assert.Contains("new PageIndexingService(", pipelineFactory);
        Assert.Contains("PageBatchSelector pageBatchSelector = new(", pipelineFactory);
        Assert.DoesNotContain("new PageAcquisitionService(", composition);
        Assert.DoesNotContain("new PageContentClassifier(", composition);
        Assert.DoesNotContain("new PageIndexingService(", composition);
        Assert.DoesNotContain("new PageBatchSelector(", composition);
    }
    [Fact]
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