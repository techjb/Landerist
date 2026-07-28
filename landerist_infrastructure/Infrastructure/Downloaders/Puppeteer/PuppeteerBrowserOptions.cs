namespace landerist_library.Infrastructure.Downloaders.Puppeteer;

public sealed record PuppeteerBrowserOptions(
    bool Headless,
    bool IsLocal,
    int TimeoutMilliseconds,
    string ProxyHost,
    int ProxyPort,
    bool RandomizeStickyPorts,
    int StickyPortMin,
    int StickyPortMax,
    string ProxyUsername,
    string ProxyPassword)
{
    public int GetProxyPort(Func<int, int, int>? nextPort = null)
    {
        if (!RandomizeStickyPorts || StickyPortMin > StickyPortMax)
        {
            return ProxyPort;
        }

        return (nextPort ?? Random.Shared.Next)(
            StickyPortMin,
            checked(StickyPortMax + 1));
    }

    public int GetTimeoutMilliseconds(bool useProxy)
    {
        if (IsLocal)
        {
            return 1_000_000;
        }

        if (TimeoutMilliseconds <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(TimeoutMilliseconds),
                "Browser timeout must be positive.");
        }

        return useProxy
            ? checked(TimeoutMilliseconds * 2)
            : TimeoutMilliseconds;
    }
}
