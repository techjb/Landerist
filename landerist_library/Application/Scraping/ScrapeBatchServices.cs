namespace landerist_library.Application.Scraping;

public sealed class ScrapeBatchServices
{
    public ScrapeBatchServices(
        IWebsiteThrottleService websiteThrottle,
        IScrapeResourceManager resources,
        IScrapeBatchMetrics metrics,
        IScrapePageSource pages,
        ScraperExecutionOptions options)
    {
        ArgumentNullException.ThrowIfNull(websiteThrottle);
        ArgumentNullException.ThrowIfNull(resources);
        ArgumentNullException.ThrowIfNull(metrics);
        ArgumentNullException.ThrowIfNull(pages);
        ArgumentNullException.ThrowIfNull(options);

        WebsiteThrottle = websiteThrottle;
        Resources = resources;
        Metrics = metrics;
        Pages = pages;
        Options = options;
        Parallelism = new ScrapeParallelismCalculator(options);
    }

    public IWebsiteThrottleService WebsiteThrottle { get; }

    public IScrapeResourceManager Resources { get; }

    public IScrapeBatchMetrics Metrics { get; }

    public IScrapePageSource Pages { get; }

    public ScraperExecutionOptions Options { get; }

    public ScrapeParallelismCalculator Parallelism { get; }
}
