using landerist_library.Downloaders.Puppeteer;

namespace landerist_unit_tests;

public sealed class PuppeteerBrowserOptionsTests
{
    [Fact]
    public void GetTimeoutMilliseconds_DoublesTimeoutForProxy()
    {
        PuppeteerBrowserOptions options = CreateOptions();

        Assert.Equal(10_000, options.GetTimeoutMilliseconds(useProxy: false));
        Assert.Equal(20_000, options.GetTimeoutMilliseconds(useProxy: true));
    }

    [Fact]
    public void GetTimeoutMilliseconds_UsesLocalDevelopmentTimeout()
    {
        PuppeteerBrowserOptions options =
            CreateOptions() with { IsLocal = true };

        Assert.Equal(
            1_000_000,
            options.GetTimeoutMilliseconds(useProxy: false));
        Assert.Equal(
            1_000_000,
            options.GetTimeoutMilliseconds(useProxy: true));
    }

    [Fact]
    public void GetProxyPort_UsesInjectedRandomSource()
    {
        PuppeteerBrowserOptions options =
            CreateOptions() with { RandomizeStickyPorts = true };
        int minimum = 0;
        int maximum = 0;

        int port = options.GetProxyPort((min, max) =>
        {
            minimum = min;
            maximum = max;
            return 8150;
        });

        Assert.Equal(8150, port);
        Assert.Equal(8100, minimum);
        Assert.Equal(8200, maximum);
    }

    [Fact]
    public void GetProxyPort_UsesFixedPortWhenRandomizationIsDisabled()
    {
        Assert.Equal(8000, CreateOptions().GetProxyPort());
    }

    private static PuppeteerBrowserOptions CreateOptions() =>
        new(
            Headless: true,
            IsLocal: false,
            TimeoutMilliseconds: 10_000,
            ProxyHost: "proxy.example",
            ProxyPort: 8000,
            RandomizeStickyPorts: false,
            StickyPortMin: 8100,
            StickyPortMax: 8199,
            ProxyUsername: "user",
            ProxyPassword: "password");
}
