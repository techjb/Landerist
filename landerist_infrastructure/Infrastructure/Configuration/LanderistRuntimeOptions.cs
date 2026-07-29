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
    bool ResolveLocalAiHost)
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
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(StatusUpdateParallelism);
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
