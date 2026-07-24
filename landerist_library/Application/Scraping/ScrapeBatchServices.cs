namespace landerist_library.Application.Scraping;

public sealed class ScrapeBatchServices
{
    public ScrapeBatchServices(
        IWebsiteThrottleService websiteThrottle,
        IScrapeBrowserManager browser,
        IPageLockManager pageLocks,
        IScrapeBatchMetrics metrics,
        IScrapePageSource pages,
        ScraperExecutionOptions options)
    {
        ArgumentNullException.ThrowIfNull(websiteThrottle);
        ArgumentNullException.ThrowIfNull(browser);
        ArgumentNullException.ThrowIfNull(pageLocks);
        ArgumentNullException.ThrowIfNull(metrics);
        ArgumentNullException.ThrowIfNull(pages);
        ArgumentNullException.ThrowIfNull(options);

        WebsiteThrottle = websiteThrottle;
        Browser = browser;
        PageLocks = pageLocks;
        Metrics = metrics;
        Pages = pages;
        Options = options;
        Parallelism = new ScrapeParallelismCalculator(options);
    }

    public IWebsiteThrottleService WebsiteThrottle { get; }

    public IScrapeBrowserManager Browser { get; }

    public IPageLockManager PageLocks { get; }

    public IScrapeBatchMetrics Metrics { get; }

    public IScrapePageSource Pages { get; }

    public ScraperExecutionOptions Options { get; }

    public ScrapeParallelismCalculator Parallelism { get; }
}
