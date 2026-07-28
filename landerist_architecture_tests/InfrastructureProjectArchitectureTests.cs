using System.Xml.Linq;

namespace landerist_architecture_tests;

public sealed class InfrastructureProjectArchitectureTests
{
    [Fact]
    public void InfrastructureProject_UsesOnlyDeclaredAdapterDependencies()
    {
        string root = FindRepositoryRoot();
        XDocument project = XDocument.Load(Path.Combine(
            root,
            "landerist_infrastructure",
            "landerist_infrastructure.csproj"));

        string[] packages = project.Descendants("PackageReference")
            .Select(element => (string?)element.Attribute("Include"))
            .Where(value => value is not null)
            .Cast<string>()
            .ToArray();
        string[] projects = project.Descendants("ProjectReference")
            .Select(element => (string?)element.Attribute("Include"))
            .Where(value => value is not null)
            .Cast<string>()
            .Order()
            .ToArray();

        Assert.Equal(
            ["Com.Bekijkhet.RobotsTxt", "HtmlAgilityPack", "Louw.SitemapParser", "Microsoft.Data.SqlClient", "PuppeteerSharp"],
            packages);
        Assert.Equal(
            [
                "..\\landerist_application\\landerist_application.csproj",
                "..\\landerist_domain\\landerist_domain.csproj",
                "..\\landerist_orels\\landerist_orels.csproj"
            ],
            projects);
        Assert.DoesNotContain(
            projects,
            reference => reference.Contains("landerist_library", StringComparison.Ordinal));
    }

    [Fact]
    public void ExtractedInfrastructure_IsPhysicallyOwnedByInfrastructureProject()
    {
        string root = FindRepositoryRoot();

        Assert.True(Directory.Exists(Path.Combine(
            root,
            "landerist_infrastructure",
            "Infrastructure",
            "Http")));
        Assert.True(Directory.Exists(Path.Combine(
            root,
            "landerist_infrastructure",
            "Infrastructure",
            "Browser")));
        Assert.False(Directory.Exists(Path.Combine(
            root,
            "landerist_library",
            "Infrastructure",
            "Browser")));
        Assert.False(Directory.Exists(Path.Combine(
            root,
            "landerist_library",
            "Infrastructure",
            "Http")));
    }

    [Fact]
    public void ExtractedInfrastructure_DoesNotUseGlobalLogging()
    {
        string root = Path.Combine(
            FindRepositoryRoot(),
            "landerist_infrastructure",
            "Infrastructure");
        string[] violations = Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .Where(file => File.ReadAllText(file).Contains("Logs.Log", StringComparison.Ordinal))
            .Select(file => Path.GetFileName(file)!)
            .ToArray();

        Assert.Empty(violations);
    }
    [Fact]
    public void ExtractedWebsiteServices_AreOwnedByInfrastructureProject()
    {
        string root = FindRepositoryRoot();
        string[] services =
        [
            "WebsiteNetworkService.cs",
            "WebsiteRefreshService.cs",
            "WebsiteRobotsPolicy.cs",
            "WebsiteSitemapService.cs",
            "SqlWebsiteCatalog.cs",
            "WebsiteMetricsService.cs"
        ];

        foreach (string service in services)
        {
            Assert.True(File.Exists(Path.Combine(
                root,
                "landerist_infrastructure",
                "Infrastructure",
                "WebsiteServices",
                service)));
            Assert.False(File.Exists(Path.Combine(
                root,
                "landerist_library",
                "Infrastructure",
                "WebsiteServices",
                service)));
        }
    }
    [Fact]
    public void WebsiteSitemapService_DependsOnIndexerPort()
    {
        string source = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "landerist_infrastructure",
            "Infrastructure",
            "WebsiteServices",
            "WebsiteSitemapService.cs"));

        Assert.DoesNotContain("new SitemapIndexer", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Infrastructure.Indexing", source, StringComparison.Ordinal);
        Assert.Contains("IWebsiteSitemapIndexerFactory", source, StringComparison.Ordinal);
    }
    [Fact]
    public void SqlCore_IsPhysicallyOwnedByInfrastructureProject()
    {
        string root = FindRepositoryRoot();
        string[] databaseTypes = ["IDatabase.cs", "IDatabaseFactory.cs", "DataBase.cs"];
        foreach (string file in databaseTypes)
        {
            Assert.True(File.Exists(Path.Combine(root, "landerist_infrastructure", "Database", file)));
            Assert.False(File.Exists(Path.Combine(root, "landerist_library", "Database", file)));
        }

        string[] sqlTypes = ["SqlDatabaseFactory.cs", "SqlDatabaseOptions.cs"];
        foreach (string file in sqlTypes)
        {
            Assert.True(File.Exists(Path.Combine(
                root,
                "landerist_infrastructure",
                "Infrastructure",
                "Sql",
                file)));
            Assert.False(File.Exists(Path.Combine(
                root,
                "landerist_library",
                "Infrastructure",
                "Sql",
                file)));
        }
    }
    [Fact]
    public void PagePersistenceGroup_IsOwnedByInfrastructureProject()
    {
        string root = FindRepositoryRoot();
        string[] sqlFiles =
        [
            "PageRepository.cs",
            "PageQueryRepository.cs",
            "PageMaintenanceRepository.cs",
            "PageQueryOptions.cs",
            "SqlTableNames.cs"
        ];
        foreach (string file in sqlFiles)
        {
            Assert.True(File.Exists(Path.Combine(
                root,
                "landerist_infrastructure",
                "Infrastructure",
                "Sql",
                file)));
            Assert.False(File.Exists(Path.Combine(
                root,
                "landerist_library",
                "Infrastructure",
                "Sql",
                file)));
        }

        string[] mappers = ["PageDataMapper.cs", "WebsiteDataMapper.cs"];
        foreach (string file in mappers)
        {
            Assert.True(File.Exists(Path.Combine(
                root,
                "landerist_infrastructure",
                "Infrastructure",
                "Sql",
                "Mapping",
                file)));
            Assert.False(File.Exists(Path.Combine(
                root,
                "landerist_library",
                "Infrastructure",
                "Sql",
                "Mapping",
                file)));
        }
    }

    [Fact]
    public void PageQueryRepository_UsesExplicitOptions()
    {
        string source = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "landerist_infrastructure",
            "Infrastructure",
            "Sql",
            "PageQueryRepository.cs"));

        Assert.DoesNotContain("landerist_library.Configuration", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Config.", source, StringComparison.Ordinal);
        Assert.DoesNotContain("WebsitesThrottle.", source, StringComparison.Ordinal);
        Assert.Contains("PageQueryOptions", source, StringComparison.Ordinal);
    }
    [Fact]
    public void WebsitePersistenceRepositories_AreOwnedByInfrastructureProject()
    {
        string root = FindRepositoryRoot();
        string[] repositories =
        [
            "WebsiteRepository.cs",
            "WebsiteQueryRepository.cs",
            "WebsitePageMetricsRepository.cs",
            "ListingStatisticsRepository.cs"
        ];

        foreach (string file in repositories)
        {
            Assert.True(File.Exists(Path.Combine(
                root,
                "landerist_infrastructure",
                "Infrastructure",
                "Sql",
                file)));
            Assert.False(File.Exists(Path.Combine(
                root,
                "landerist_library",
                "Infrastructure",
                "Sql",
                file)));
        }
    }
    [Fact]
    public void ListingPersistenceGroup_IsOwnedByInfrastructureProject()
    {
        string root = FindRepositoryRoot();
        string[] repositories =
        [
            "ListingRepository.cs",
            "ListingQueryRepository.cs",
            "MediaRepository.cs",
            "SourceRepository.cs"
        ];
        foreach (string file in repositories)
        {
            AssertMoved(root, "Sql", file);
        }

        string[] mappers =
        [
            "ListingDataMapper.cs",
            "MediaDataMapper.cs",
            "SourceDataMapper.cs"
        ];
        foreach (string file in mappers)
        {
            AssertMoved(root, Path.Combine("Sql", "Mapping"), file);
        }

        string[] adapters =
        [
            "LegacyListingUnpublishPolicy.cs",
            "OrelsListingDeletionService.cs",
            "SqlListingMaintenanceService.cs",
            "SqlListingMediaStore.cs",
            "SqlListingQueryService.cs",
            "SqlListingSourceStore.cs",
            "SqlNotListingCacheService.cs",
            "SqlListingStore.cs",
            "SqlPageLinkService.cs",
            "SqlListingAdministrationService.cs",
            "SqlListingEnricher.cs"
        ];
        foreach (string file in adapters)
        {
            AssertMoved(root, "Listings", file);
        }
    }

    private static void AssertMoved(string root, string area, string file)
    {
        Assert.True(File.Exists(Path.Combine(
            root,
            "landerist_infrastructure",
            "Infrastructure",
            area,
            file)));
        Assert.False(File.Exists(Path.Combine(
            root,
            "landerist_library",
            "Infrastructure",
            area,
            file)));
    }
    [Fact]
    public void SqlListingStore_UsesStatisticsPort()
    {
        string source = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "landerist_infrastructure",
            "Infrastructure",
            "Listings",
            "SqlListingStore.cs"));

        Assert.Contains("IGlobalStatisticsRepository", source, StringComparison.Ordinal);
        Assert.DoesNotContain("new GlobalStatisticsRepository", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Infrastructure.Statistics", source, StringComparison.Ordinal);
    }
    [Fact]
    public void SqlPageLinkService_UsesDomainUrlRules()
    {
        string source = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "landerist_infrastructure",
            "Infrastructure",
            "Listings",
            "SqlPageLinkService.cs"));

        Assert.Contains("WebsiteUrlRules", source, StringComparison.Ordinal);
        Assert.DoesNotContain("landerist_library.Tools", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Infrastructure.Indexing", source, StringComparison.Ordinal);
    }
    [Fact]
    public void SqlListingAdministrationService_UsesLoggingPort()
    {
        string source = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "landerist_infrastructure",
            "Infrastructure",
            "Listings",
            "SqlListingAdministrationService.cs"));

        Assert.Contains("IApplicationLogger", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Logs.Log", source, StringComparison.Ordinal);
    }
    [Fact]
    public void SqlListingEnricher_UsesLocationPort()
    {
        string source = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "landerist_infrastructure",
            "Infrastructure",
            "Listings",
            "SqlListingEnricher.cs"));

        Assert.Contains("IListingLocationEnricher", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Parse.Location", source, StringComparison.Ordinal);
        Assert.DoesNotContain("LocationParser", source, StringComparison.Ordinal);
    }
    [Fact]
    public void PageServicesAndStatistics_AreOwnedByInfrastructureProject()
    {
        string root = FindRepositoryRoot();
        string[] pageServices =
        [
            "SqlPageCatalog.cs",
            "SqlPageDeletionService.cs",
            "SqlPageQueryService.cs",
            "SqlPageWaitingStatusService.cs"
        ];
        foreach (string file in pageServices)
        {
            AssertMoved(root, "PageServices", file);
        }

        string[] statistics =
        [
            "GlobalStatisticsRepository.cs",
            "HostStatisticsRepository.cs"
        ];
        foreach (string file in statistics)
        {
            AssertMoved(root, "Statistics", file);
        }
    }
    [Fact]
    public void DecoupledScrapingAdapters_AreOwnedByInfrastructureProject()
    {
        string root = FindRepositoryRoot();
        string[] scrapingAdapters =
        [
            "ConditionalHeaderCheckResult.cs",
            "ConditionalPageHeaderChecker.cs",
            "HttpConditionalPageHeaderService.cs",
            "PageContentClassifier.cs",
            "PageIndexingService.cs",
            "PooledPageDownloader.cs",
            "ScrapeBrowserManager.cs",
            "SqlPageClassificationMetrics.cs",
            "SqlPageLockManager.cs",
            "SqlPageSchedulingService.cs",
            "SqlPageSelectionRepository.cs",
            "SqlScrapeBatchMetrics.cs",
            "SqlScrapeMetrics.cs",
            "SqlScrapePageSource.cs",
            "SqlWebsiteThrottleService.cs",
            "WebsitesThrottle.cs"
        ];

        foreach (string file in scrapingAdapters)
        {
            AssertMoved(root, "Scraping", file);
        }
    }

    [Fact]
    public void ExtractedScraping_DoesNotDependOnLegacyDownloaders()
    {
        string directory = Path.Combine(
            FindRepositoryRoot(),
            "landerist_infrastructure",
            "Infrastructure",
            "Scraping");

        foreach (string file in Directory.GetFiles(directory, "*.cs"))
        {
            string source = File.ReadAllText(file);
            Assert.DoesNotContain("landerist_library.Downloaders", source, StringComparison.Ordinal);
            Assert.DoesNotContain("Logs.Log", source, StringComparison.Ordinal);
            Assert.DoesNotContain("landerist_library.Tools", source, StringComparison.Ordinal);
            Assert.DoesNotContain("landerist_library.Infrastructure.Parsing", source, StringComparison.Ordinal);
        }
    }
    [Fact]
    public void PageIndexingService_DelegatesHtmlExtraction()
    {
        string source = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "landerist_infrastructure",
            "Infrastructure",
            "Scraping",
            "PageIndexingService.cs"));

        Assert.Contains("IPageLinkExtractor", source, StringComparison.Ordinal);
        Assert.DoesNotContain("HtmlAgilityPack", source, StringComparison.Ordinal);
        Assert.DoesNotContain("GetHtmlDocument", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Infrastructure.Indexing", source, StringComparison.Ordinal);
    }
    [Fact]
    public void SitemapInfrastructure_IsOwnedByInfrastructureProject()
    {
        string root = FindRepositoryRoot();
        string[] files =
        [
            "GzipAwareSitemapFetcher.cs",
            "LegacyWebsiteSitemapIndexerFactory.cs",
            "SitemapIndexer.cs"
        ];

        foreach (string file in files)
        {
            AssertMoved(root, "Indexing", file);
        }

        string indexer = File.ReadAllText(Path.Combine(
            root,
            "landerist_infrastructure",
            "Infrastructure",
            "Indexing",
            "SitemapIndexer.cs"));
        Assert.DoesNotContain(": Indexer", indexer, StringComparison.Ordinal);
        Assert.DoesNotContain("Config.", indexer, StringComparison.Ordinal);
        Assert.DoesNotContain("Logs.Log", indexer, StringComparison.Ordinal);
    }
    [Fact]
    public void LegacyHtmlIndexers_DoNotExist()
    {
        string root = FindRepositoryRoot();
        string legacyDirectory = Path.Combine(
            root,
            "landerist_library",
            "Infrastructure",
            "Indexing");
        Assert.False(Directory.Exists(legacyDirectory));

        string administration = File.ReadAllText(Path.Combine(
            root,
            "landerist_library",
            "Infrastructure",
            "Administration",
            "PageAdministrationService.Delete.cs"));
        Assert.Contains("PageLinks.Index(page, newUri)", administration, StringComparison.Ordinal);
        Assert.DoesNotContain("new Indexer", administration, StringComparison.Ordinal);
        Assert.DoesNotContain("Infrastructure.Indexing", administration, StringComparison.Ordinal);
    }
    [Fact]
    public void HtmlNavigationAdapters_AreOwnedByInfrastructureProject()
    {
        string root = FindRepositoryRoot();
        string target = Path.Combine(
            root,
            "landerist_infrastructure",
            "Infrastructure",
            "Parsing");
        Assert.True(File.Exists(Path.Combine(target, "HtmlPageLinkExtractor.cs")));
        Assert.True(File.Exists(Path.Combine(target, "HtmlPageContentInspector.cs")));
        string inspector = File.ReadAllText(Path.Combine(target, "HtmlPageContentInspector.cs"));
        Assert.DoesNotContain("SetListingParserInput", inspector, StringComparison.Ordinal);
        Assert.DoesNotContain("MatchesWebsiteListingUnavailableRule", inspector, StringComparison.Ordinal);
        string htmlTarget = Path.Combine(root, "landerist_infrastructure", "Html");
        Assert.True(File.Exists(Path.Combine(htmlTarget, "PageHtmlDocumentExtensions.cs")));
        Assert.True(File.Exists(Path.Combine(htmlTarget, "PageHtmlSignalExtensions.cs")));
        Assert.False(File.Exists(Path.Combine(
            root,
            "landerist_library",
            "Infrastructure",
            "Parsing",
            "HtmlPageLinkExtractor.cs")));
        Assert.False(File.Exists(Path.Combine(
            root,
            "landerist_library",
            "Infrastructure",
            "Parsing",
            "HtmlPageContentInspector.cs")));
        Assert.False(File.Exists(Path.Combine(
            root,
            "landerist_library",
            "Parse",
            "Pages",
            "PageHtmlDocumentExtensions.cs")));
        Assert.False(File.Exists(Path.Combine(
            root,
            "landerist_library",
            "Parse",
            "Pages",
            "PageHtmlSignalExtensions.cs")));
    }
    [Fact]
    public void BatchWritingInfrastructure_IsOwnedByInfrastructureProject()
    {
        string root = FindRepositoryRoot();
        string[] files =
        [
            "BatchProvider.cs",
            "IBatchInputWriter.cs",
            "IListingBatchUploadProvider.cs",
            "JsonlBatchInputWriter.cs"
        ];

        foreach (string file in files)
        {
            AssertMoved(root, "Parsing", file);
        }

        string writer = File.ReadAllText(Path.Combine(
            root,
            "landerist_infrastructure",
            "Infrastructure",
            "Parsing",
            "JsonlBatchInputWriter.cs"));
        Assert.DoesNotContain("LLMProvider", writer, StringComparison.Ordinal);
        Assert.DoesNotContain("Parse.ListingParser", writer, StringComparison.Ordinal);
    }
    [Fact]
    public void BatchUploadTask_IsOwnedByInfrastructureProject()
    {
        string root = FindRepositoryRoot();
        AssertMoved(root, "Tasks", "BatchUploadOptions.cs");
        AssertMoved(root, "Tasks", "TaskBatchUpload.cs");

        string task = File.ReadAllText(Path.Combine(
            root,
            "landerist_infrastructure",
            "Infrastructure",
            "Tasks",
            "TaskBatchUpload.cs"));
        Assert.Contains("IBatchRegistrationStore", task, StringComparison.Ordinal);
        Assert.Contains("IApplicationLogger", task, StringComparison.Ordinal);
        Assert.DoesNotContain("BatchRepository", task, StringComparison.Ordinal);
        Assert.DoesNotContain("LLMProvider", task, StringComparison.Ordinal);
        Assert.DoesNotContain("Logs.Log", task, StringComparison.Ordinal);
        Assert.DoesNotContain("Log.Write", task, StringComparison.Ordinal);

        string registrationStore = Path.Combine(
            root,
            "landerist_infrastructure",
            "Infrastructure",
            "Sql",
            "SqlBatchRegistrationStore.cs");
        Assert.True(File.Exists(registrationStore));
        Assert.False(File.Exists(Path.Combine(
            root,
            "landerist_library",
            "Infrastructure",
            "Sql",
            "LegacyBatchRegistrationStore.cs")));
        Assert.False(File.Exists(Path.Combine(
            root,
            "landerist_library",
            "Infrastructure",
            "Sql",
            "BatchRepository.cs")));
        Assert.False(File.Exists(Path.Combine(
            root,
            "landerist_library",
            "Database",
            "Batch.cs")));
    }
    [Fact]
    public void BatchCleaner_IsOwnedByInfrastructureProject()
    {
        string root = FindRepositoryRoot();
        AssertMoved(root, "Tasks", "TaskBatchCleaner.cs");

        string cleaner = File.ReadAllText(Path.Combine(
            root,
            "landerist_infrastructure",
            "Infrastructure",
            "Tasks",
            "TaskBatchCleaner.cs"));
        Assert.Contains("IBatchStore", cleaner, StringComparison.Ordinal);
        Assert.Contains("IBatchArtifactCleaner", cleaner, StringComparison.Ordinal);
        Assert.DoesNotContain("BatchRepository", cleaner, StringComparison.Ordinal);
        Assert.DoesNotContain("Config.", cleaner, StringComparison.Ordinal);
        Assert.DoesNotContain("VertexAIBatchCleaner", cleaner, StringComparison.Ordinal);

        Assert.True(File.Exists(Path.Combine(
            root,
            "landerist_infrastructure",
            "Infrastructure",
            "Sql",
            "SqlBatchStore.cs")));
    }
    [Fact]
    public void BatchDownloadTask_IsOwnedByInfrastructureProject()
    {
        string root = FindRepositoryRoot();
        AssertMoved(root, "Tasks", "TaskBatchDownload.cs");

        string task = File.ReadAllText(Path.Combine(
            root,
            "landerist_infrastructure",
            "Infrastructure",
            "Tasks",
            "TaskBatchDownload.cs"));
        string[] forbidden =
        [
            "BatchRepository",
            "LLMProvider",
            "Config.",
            "ParseListing",
            "OpenAIBatchDownload",
            "VertexAIBatchDownload",
            "Logs.Log",
            "Log.Write"
        ];
        Assert.DoesNotContain(
            forbidden,
            token => task.Contains(token, StringComparison.Ordinal));
        Assert.Contains("IBatchStore", task, StringComparison.Ordinal);
        Assert.Contains("BatchDownloadProviderCatalog", task, StringComparison.Ordinal);
        Assert.Contains("IBatchListingResponseParser", task, StringComparison.Ordinal);
        Assert.Contains("IApplicationLogger", task, StringComparison.Ordinal);
    }
    [Fact]
    public void DecoupledTaskJobs_AreOwnedByInfrastructureProject()
    {
        string root = FindRepositoryRoot();
        string[] files =
        [
            "DailyTaskJob.cs",
            "HourlyTaskJob.cs",
            "LocalAiTaskJob.cs",
            "LocalAiParsingTaskOptions.cs",
            "TaskLocalAIParsing.cs",
            "ScrapeTaskJob.cs",
            "SystemRecurringTaskScheduler.cs",
            "TenMinuteTaskJob.cs"
        ];

        foreach (string file in files)
        {
            AssertMoved(root, "Tasks", file);
        }

        string directory = Path.Combine(
            root,
            "landerist_infrastructure",
            "Infrastructure",
            "Tasks");
        foreach (string file in files)
        {
            string source = File.ReadAllText(Path.Combine(directory, file));
            Assert.DoesNotContain("Config.", source, StringComparison.Ordinal);
            Assert.DoesNotContain("Logs.Log", source, StringComparison.Ordinal);
            Assert.DoesNotContain("landerist_library.Database", source, StringComparison.Ordinal);
            Assert.DoesNotContain("Console.", source, StringComparison.Ordinal);
        }
    }
    [Fact]
    public void LocalAiParsingTask_UsesMigrationPorts()
    {
        string source = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "landerist_infrastructure",
            "Infrastructure",
            "Tasks",
            "TaskLocalAIParsing.cs"));
        string[] required =
        [
            "ILocalAiListingParser",
            "ILocalAiTokenBudget",
            "IListingInputPreparer",
            "IApplicationLogger"
        ];
        Assert.All(required, token => Assert.Contains(token, source, StringComparison.Ordinal));

        string[] forbidden =
        [
            "ParseListing",
            "Tokenizer",
            "LLMProvider",
            "GetListingParserInput",
            "Logs.Log",
            "Log.Write"
        ];
        Assert.DoesNotContain(
            forbidden,
            token => source.Contains(token, StringComparison.Ordinal));
    }

    [Fact]
    public void LegacyTaskAdapters_AreGroupedByCapability()
    {
        string root = FindRepositoryRoot();
        Assert.False(Directory.Exists(Path.Combine(
            root,
            "landerist_library",
            "Infrastructure",
            "Tasks")));

        string administration = Path.Combine(
            root,
            "landerist_library",
            "Infrastructure",
            "Administration");
        Assert.True(File.Exists(Path.Combine(
            administration,
            "LegacyAddressDataMaintenance.cs")));

        string parsing = Path.Combine(
            root,
            "landerist_library",
            "Infrastructure",
            "Parsing");
        Assert.True(File.Exists(Path.Combine(parsing, "LegacyBatchListingResponseParser.cs")));
        Assert.True(File.Exists(Path.Combine(parsing, "LegacyListingLocationEnricher.cs")));
        Assert.True(File.Exists(Path.Combine(parsing, "LegacyListingPageParser.cs")));
        Assert.True(File.Exists(Path.Combine(parsing, "LegacyLocalAiListingParser.cs")));
        Assert.True(File.Exists(Path.Combine(parsing, "LegacyLocalAiTokenBudget.cs")));
        Assert.True(File.Exists(Path.Combine(parsing, "LegacyPageTokenLimitPolicy.cs")));
        Assert.True(File.Exists(Path.Combine(
            parsing,
            "VertexAI",
            "LegacyVertexAiBatchArtifactCleaner.cs")));
    }

    [Fact]
    public void LegacyLibrary_ReferencesInfrastructureProject()
    {
        string project = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "landerist_library",
            "landerist_library.csproj"));

        Assert.Contains(
            "..\\landerist_infrastructure\\landerist_infrastructure.csproj",
            project);
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