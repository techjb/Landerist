using landerist_library.Parsing;
using landerist_library.Application.Distribution;
using landerist_library.Infrastructure.Logging;

namespace landerist_library.Infrastructure.Runtime;

public enum LanderistExecutionRole
{
    Principal,
    Scraper,
    LocalAi
}

public sealed record LanderistRuntimeOptions(
    DatabaseRuntimeOptions Database,
    ProxyRuntimeOptions Proxy,
    BrowserRuntimeOptions Browser,
    LanderistExecutionRole Role)
{
    public AiRuntimeOptions Ai { get; init; } = AiRuntimeOptions.Empty;

    public BatchRuntimeOptions Batch { get; init; } = BatchRuntimeOptions.Disabled;

    public ScrapingRuntimeOptions Scraping { get; init; } = ScrapingRuntimeOptions.Default;

    public IntegrationRuntimeOptions Integrations { get; init; } = IntegrationRuntimeOptions.Empty;

    public ExecutionRuntimeOptions Execution { get; init; } = ExecutionRuntimeOptions.Default;

    public DistributionOptions Distribution { get; init; } = DistributionOptions.Empty;

    public DatabaseBackupOptions Backup { get; init; } = DatabaseBackupOptions.Disabled;

    public AdministrationOptions Administration { get; init; } = AdministrationOptions.Default;

    public LogRetentionOptions LogRetention { get; init; } = LogRetentionOptions.Default;

    public HealthRuntimeOptions Health { get; init; } = HealthRuntimeOptions.Default;

    public void Validate()
    {
        ArgumentNullException.ThrowIfNull(Database);
        ArgumentNullException.ThrowIfNull(Proxy);
        ArgumentNullException.ThrowIfNull(Browser);
        Database.Validate();
        Proxy.Validate();
        Browser.Validate();
        Ai.Validate();
        Batch.Validate();
        Scraping.Validate();
        Integrations.Validate();
        Execution.Validate();
        Distribution.Validate();
        Backup.Validate();
        Administration.Validate();
        LogRetention.Validate();
        Health.Validate();
    }
}

public sealed record HealthRuntimeOptions(
    string FilePath,
    int IntervalSeconds,
    string HealthchecksPingUrl = "")
{
    public static HealthRuntimeOptions Default { get; } = new(
        "landerist-health.json", 60);

    public void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(FilePath);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(IntervalSeconds);
        ArgumentNullException.ThrowIfNull(HealthchecksPingUrl);
        if (!string.IsNullOrWhiteSpace(HealthchecksPingUrl) &&
            (!Uri.TryCreate(HealthchecksPingUrl, UriKind.Absolute, out Uri? pingUri) ||
             pingUri.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException(
                "The Healthchecks ping URL must be an absolute HTTPS URL.");
        }
    }
}

public sealed record AdministrationOptions(
    int UnpublishedListingRetentionDays,
    string WebsiteCleanupFilePath)
{
    public static AdministrationOptions Default { get; } = new(180, "HostMainUri.csv");

    public void Validate()
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(UnpublishedListingRetentionDays);
        ArgumentException.ThrowIfNullOrWhiteSpace(WebsiteCleanupFilePath);
    }
}

public sealed record DatabaseBackupOptions(
    string DatabaseName,
    string LocalDirectory,
    string BucketName,
    int RetentionDays)
{
    public static DatabaseBackupOptions Disabled { get; } = new(
        "unconfigured", ".", "unconfigured", 60);

    public void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(DatabaseName);
        ArgumentException.ThrowIfNullOrWhiteSpace(LocalDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(BucketName);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(RetentionDays);
    }
}

public sealed record AiRuntimeOptions(
    string OpenAiApiKey,
    string VertexCredential,
    string VertexProjectId,
    string VertexLocation,
    string VertexPublisher,
    string VertexListingModel,
    string VertexAddressModel,
    string LocalAiHost,
    bool ResolveLocalAiHost,
    LLMProvider Provider = LLMProvider.OpenAI)
{
    public static AiRuntimeOptions Empty { get; } = new(
        string.Empty, string.Empty, string.Empty, string.Empty, "google",
        string.Empty, string.Empty, "localhost", false);

    public void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(VertexPublisher);
        ArgumentException.ThrowIfNullOrWhiteSpace(LocalAiHost);
    }
}

public sealed record ScrapingRuntimeOptions(
    bool NotListingCacheEnabled,
    int MaxPagesPerWebsite,
    bool IndexerEnabled,
    int MaxPagesPerHostPerScrape,
    int MaxPagesPerScrape,
    int MinPagesPerScrape,
    int MaxDegreeOfParallelism,
    int HttpTimeoutSeconds)
{
    public static ScrapingRuntimeOptions Default { get; } = new(
        false, 40_000, true, 2, 2_500, 10, 1, 100);

    public void Validate()
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(MaxPagesPerWebsite);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(MaxPagesPerHostPerScrape);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(MaxPagesPerScrape);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(MinPagesPerScrape);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(MaxDegreeOfParallelism);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(HttpTimeoutSeconds);

        if (MinPagesPerScrape > MaxPagesPerScrape)
        {
            throw new InvalidOperationException(
                "The minimum scrape page count cannot exceed the maximum.");
        }
    }
}

public sealed record IntegrationRuntimeOptions(
    string ScrapingBeeApiKey,
    string AwsAccessKeyId,
    string AwsSecretAccessKey,
    string AwsDownloadsBucket,
    string AwsWebsiteBucket,
    string GoolzoomApiKey,
    string GoogleCloudLanderistApiKey)
{
    public static IntegrationRuntimeOptions Empty { get; } = new(
        string.Empty, string.Empty, string.Empty, string.Empty,
        string.Empty, string.Empty, string.Empty);

    public void Validate()
    {
        ArgumentNullException.ThrowIfNull(ScrapingBeeApiKey);
        ArgumentNullException.ThrowIfNull(AwsAccessKeyId);
        ArgumentNullException.ThrowIfNull(AwsSecretAccessKey);
        ArgumentNullException.ThrowIfNull(AwsDownloadsBucket);
        ArgumentNullException.ThrowIfNull(AwsWebsiteBucket);
        ArgumentNullException.ThrowIfNull(GoolzoomApiKey);
        ArgumentNullException.ThrowIfNull(GoogleCloudLanderistApiKey);
    }
}

public sealed record ExecutionRuntimeOptions(
    bool IsLocal,
    bool IsProduction,
    string MachineName,
    bool LogsEnabled,
    bool LogErrorsToConsole,
    bool LogInformationToConsole,
    int LocalAiMaxModelLength,
    string Version)
{
    public static ExecutionRuntimeOptions Default { get; } = new(
        false, true, Environment.MachineName, true, false, true, 60_000, "unknown");

    public void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(MachineName);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(LocalAiMaxModelLength);
        ArgumentException.ThrowIfNullOrWhiteSpace(Version);

        if (IsLocal == IsProduction)
        {
            throw new InvalidOperationException(
                "Execution must be either local or production.");
        }
    }
}
public sealed record BatchRuntimeOptions(
    bool Enabled,
    string Directory,
    int MaxPages,
    int MinPages,
    long MaxFileSizeBytes,
    int StatusUpdateParallelism,
    bool UpdateWaitingResponse,
    int CleanupAfterDays,
    string VertexBucketName)
{
    public static BatchRuntimeOptions Disabled { get; } = new(
        false, string.Empty, 1, 1, 1, 1, false, 1, string.Empty);

    public void Validate()
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(MaxPages);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(MinPages);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(MaxFileSizeBytes);
        if (StatusUpdateParallelism is 0 or < -1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(StatusUpdateParallelism),
                StatusUpdateParallelism,
                "Parallelism must be -1 (unbounded) or a positive value.");
        }
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(CleanupAfterDays);

        if (MinPages > MaxPages)
        {
            throw new InvalidOperationException(
                "The minimum batch page count cannot exceed the maximum.");
        }

        if (Enabled && string.IsNullOrWhiteSpace(Directory))
        {
            throw new InvalidOperationException(
                "A batch directory is required when batch processing is enabled.");
        }
    }
}

public sealed record DatabaseRuntimeOptions(
    string DataSource,
    string UserId,
    string Password,
    string DatabaseName,
    bool Encrypt,
    bool TrustServerCertificate,
    int ConnectionTimeoutSeconds = 30,
    int CommandTimeoutSeconds = 120)
{
    public void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(DataSource);
        ArgumentException.ThrowIfNullOrWhiteSpace(UserId);
        ArgumentException.ThrowIfNullOrWhiteSpace(Password);
        ArgumentException.ThrowIfNullOrWhiteSpace(DatabaseName);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(ConnectionTimeoutSeconds);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(CommandTimeoutSeconds);
    }
}

public sealed record ProxyRuntimeOptions(
    string Host,
    int Port,
    bool RandomizeStickyPorts,
    int StickyPortMin,
    int StickyPortMax,
    string Username,
    string Password)
{
    public void Validate()
    {
        ArgumentOutOfRangeException.ThrowIfNegative(Port);
        ArgumentOutOfRangeException.ThrowIfNegative(StickyPortMin);
        ArgumentOutOfRangeException.ThrowIfNegative(StickyPortMax);

        if (RandomizeStickyPorts && StickyPortMin > StickyPortMax)
        {
            throw new InvalidOperationException(
                "The minimum sticky proxy port cannot exceed the maximum.");
        }

        if (!string.IsNullOrWhiteSpace(Host) && Port == 0)
        {
            throw new InvalidOperationException(
                "A proxy port is required when a proxy host is configured.");
        }
    }
}

public sealed record BrowserRuntimeOptions(
    bool Headless,
    bool IsLocal,
    int TimeoutMilliseconds,
    bool ProcessCleanupEnabled,
    bool UseTaskKillFallback)
{
    public void Validate()
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(TimeoutMilliseconds);
    }
}
