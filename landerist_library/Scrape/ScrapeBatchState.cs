using landerist_library.Application.Scraping;

namespace landerist_library.Scrape;

internal sealed class ScrapeBatchState
{
    private int _total;
    private int _processed;
    private int _scrapedSuccess;
    private int _crashed;
    private int _downloadErrors;
    private int _skippedByRobotsTxt;
    private int _skippedByCrawlDelay;
    private int _skippedByBlockedWebsite;
    private ScrapeBatchCounters _totals = ScrapeBatchCounters.Empty;

    public void Reset(int total)
    {
        Interlocked.Exchange(ref _total, total);
        Interlocked.Exchange(ref _processed, 0);
        Interlocked.Exchange(ref _scrapedSuccess, 0);
        Interlocked.Exchange(ref _crashed, 0);
        Interlocked.Exchange(ref _downloadErrors, 0);
        Interlocked.Exchange(ref _skippedByRobotsTxt, 0);
        Interlocked.Exchange(ref _skippedByCrawlDelay, 0);
        Interlocked.Exchange(ref _skippedByBlockedWebsite, 0);
    }

    public void IncrementProcessed() => Interlocked.Increment(ref _processed);

    public void IncrementScrapedSuccess() => Interlocked.Increment(ref _scrapedSuccess);

    public void IncrementCrashed() => Interlocked.Increment(ref _crashed);

    public void IncrementDownloadErrors() => Interlocked.Increment(ref _downloadErrors);

    public void IncrementSkippedByRobotsTxt() =>
        Interlocked.Increment(ref _skippedByRobotsTxt);

    public void IncrementSkippedByCrawlDelay() =>
        Interlocked.Increment(ref _skippedByCrawlDelay);

    public void IncrementSkippedByBlockedWebsite() =>
        Interlocked.Increment(ref _skippedByBlockedWebsite);

    public ScrapeBatchCounters GetCurrent() =>
        new(
            Volatile.Read(ref _total),
            Volatile.Read(ref _processed),
            Volatile.Read(ref _scrapedSuccess),
            Volatile.Read(ref _crashed),
            Volatile.Read(ref _downloadErrors),
            Volatile.Read(ref _skippedByRobotsTxt),
            Volatile.Read(ref _skippedByCrawlDelay),
            Volatile.Read(ref _skippedByBlockedWebsite));

    public ScrapeBatchCounters AccumulateTotals()
    {
        _totals = _totals.Add(GetCurrent());
        return _totals;
    }
}
