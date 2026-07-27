namespace landerist_library.Websites;

public sealed record WebsiteRules(
    string DefaultBrowserUserAgent,
    int HttpClientTimeoutSeconds,
    int MaxCrawlDelaySeconds)
{
    public static WebsiteRules Default { get; } = new(
        DefaultBrowserUserAgent: "Mozilla/5.0 (compatible; AcmeInc/1.0)",
        HttpClientTimeoutSeconds: 10,
        MaxCrawlDelaySeconds: 300);
}
