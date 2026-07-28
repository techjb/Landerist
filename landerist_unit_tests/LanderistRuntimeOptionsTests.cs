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
