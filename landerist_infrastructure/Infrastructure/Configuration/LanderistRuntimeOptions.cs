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
    public void Validate()
    {
        ArgumentNullException.ThrowIfNull(Database);
        ArgumentNullException.ThrowIfNull(Proxy);
        ArgumentNullException.ThrowIfNull(Browser);
        Database.Validate();
        Proxy.Validate();
        Browser.Validate();
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
