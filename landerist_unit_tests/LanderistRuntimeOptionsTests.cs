using landerist_library.Infrastructure.Runtime;

namespace landerist_unit_tests;

public sealed class LanderistRuntimeOptionsTests
{
    [Fact]
    public void Validate_AcceptsCompleteRuntimeConfiguration()
    {
        LanderistRuntimeOptions options = CreateOptions();

        options.Validate();
    }

    [Fact]
    public void Validate_RejectsMissingDatabaseDataSource()
    {
        LanderistRuntimeOptions options = CreateOptions() with
        {
            Database = CreateOptions().Database with { DataSource = string.Empty }
        };

        Assert.Throws<ArgumentException>(options.Validate);
    }

    [Fact]
    public void Validate_RejectsProxyHostWithoutPort()
    {
        LanderistRuntimeOptions options = CreateOptions() with
        {
            Proxy = CreateOptions().Proxy with
            {
                Host = "proxy.example.test",
                Port = 0
            }
        };

        Assert.Throws<InvalidOperationException>(options.Validate);
    }

    [Fact]
    public void Validate_RejectsInvalidStickyProxyRange()
    {
        LanderistRuntimeOptions options = CreateOptions() with
        {
            Proxy = CreateOptions().Proxy with
            {
                RandomizeStickyPorts = true,
                StickyPortMin = 8200,
                StickyPortMax = 8100
            }
        };

        Assert.Throws<InvalidOperationException>(options.Validate);
    }

    [Fact]
    public void Validate_RejectsNonPositiveBrowserTimeout()
    {
        LanderistRuntimeOptions options = CreateOptions() with
        {
            Browser = CreateOptions().Browser with { TimeoutMilliseconds = 0 }
        };

        Assert.Throws<ArgumentOutOfRangeException>(options.Validate);
    }

    [Fact]
    public void Validate_RejectsScrapeMinimumAboveMaximum()
    {
        LanderistRuntimeOptions options = CreateOptions() with
        {
            Scraping = ScrapingRuntimeOptions.Default with
            {
                MaxPagesPerScrape = 10,
                MinPagesPerScrape = 11
            }
        };

        Assert.Throws<InvalidOperationException>(options.Validate);
    }

    [Fact]
    public void Validate_AcceptsNormalizedBatchValues()
    {
        LanderistRuntimeOptions options = CreateOptions() with
        {
            Batch = new BatchRuntimeOptions(
                true,
                "batch",
                100,
                1,
                1024,
                StatusUpdateParallelism: -1,
                UpdateWaitingResponse: true,
                CleanupAfterDays: 30,
                VertexBucketName: "bucket")
        };

        options.Validate();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    public void Validate_RejectsInvalidBatchParallelism(int parallelism)
    {
        LanderistRuntimeOptions options = CreateOptions() with
        {
            Batch = BatchRuntimeOptions.Disabled with
            {
                StatusUpdateParallelism = parallelism
            }
        };

        Assert.Throws<ArgumentOutOfRangeException>(options.Validate);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-30)]
    public void Validate_RejectsNonPositiveBatchCleanupAge(int cleanupDays)
    {
        LanderistRuntimeOptions options = CreateOptions() with
        {
            Batch = BatchRuntimeOptions.Disabled with
            {
                CleanupAfterDays = cleanupDays
            }
        };

        Assert.Throws<ArgumentOutOfRangeException>(options.Validate);
    }

    [Fact]
    public void Validate_RejectsAmbiguousExecutionEnvironment()
    {
        LanderistRuntimeOptions options = CreateOptions() with
        {
            Execution = ExecutionRuntimeOptions.Default with
            {
                IsLocal = true,
                IsProduction = true
            }
        };

        Assert.Throws<InvalidOperationException>(options.Validate);
    }

    [Fact]
    public void Validate_RejectsMissingDistributionDirectories()
    {
        LanderistRuntimeOptions options = CreateOptions() with
        {
            Distribution = DistributionOptions.Empty with
            {
                ExportDirectory = string.Empty
            }
        };

        Assert.Throws<ArgumentException>(options.Validate);
    }

    [Fact]
    public void Validate_RejectsNonPositiveBackupRetention()
    {
        LanderistRuntimeOptions options = CreateOptions() with
        {
            Backup = DatabaseBackupOptions.Disabled with { RetentionDays = 0 }
        };

        Assert.Throws<ArgumentOutOfRangeException>(options.Validate);
    }

    private static LanderistRuntimeOptions CreateOptions() => new(
        new DatabaseRuntimeOptions(
            "sql.example.test",
            "landerist-user",
            "secret",
            "landerist-db",
            Encrypt: true,
            TrustServerCertificate: false),
        new ProxyRuntimeOptions(
            Host: string.Empty,
            Port: 0,
            RandomizeStickyPorts: false,
            StickyPortMin: 0,
            StickyPortMax: 0,
            Username: string.Empty,
            Password: string.Empty),
        new BrowserRuntimeOptions(
            Headless: true,
            IsLocal: false,
            TimeoutMilliseconds: 10_000,
            ProcessCleanupEnabled: true,
            UseTaskKillFallback: false),
        LanderistExecutionRole.Scraper);
}
