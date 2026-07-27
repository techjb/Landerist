using landerist_library.Application.Websites;

namespace landerist_library.Application.Scraping;

public sealed class ScrapeBatchServices
{
    public ScrapeBatchServices(
        IWebsiteThrottleService websiteThrottle,
        IScrapeBrowserManager browser,
        IPageLockManager pageLocks,
        IScrapeBatchMetrics metrics,
        IScrapePageSource pages,
        IWebsiteRobotsPolicy robots,
        ScraperExecutionOptions options)
    {
        ArgumentNullException.ThrowIfNull(websiteThrottle);
        ArgumentNullException.ThrowIfNull(browser);
        ArgumentNullException.ThrowIfNull(pageLocks);
        ArgumentNullException.ThrowIfNull(metrics);
        ArgumentNullException.ThrowIfNull(pages);
        ArgumentNullException.ThrowIfNull(robots);
        ArgumentNullException.ThrowIfNull(options);

        WebsiteThrottle = websiteThrottle;
        Browser = browser;
        PageLocks = pageLocks;
        Metrics = metrics;
        Pages = pages;
        Robots = robots;
        Options = options;
        Parallelism = new ScrapeParallelismCalculator(options);
    }

    public IWebsiteThrottleService WebsiteThrottle { get; }

    public IScrapeBrowserManager Browser { get; }

    public IPageLockManager PageLocks { get; }

    public IScrapeBatchMetrics Metrics { get; }

    public IScrapePageSource Pages { get; }

    public IWebsiteRobotsPolicy Robots { get; }

    public ScraperExecutionOptions Options { get; }

    public ScrapeParallelismCalculator Parallelism { get; }
}
