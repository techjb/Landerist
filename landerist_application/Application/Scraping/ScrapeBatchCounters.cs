namespace landerist_library.Application.Scraping;

public sealed record ScrapeBatchCounters(
    int Total,
    int Processed,
    int ScrapedSuccess,
    int Crashed,
    int DownloadErrors,
    int SkippedByRobotsTxt,
    int SkippedByCrawlDelay,
    int SkippedByBlockedWebsite)
{
    public int Skipped =>
        SkippedByRobotsTxt + SkippedByCrawlDelay + SkippedByBlockedWebsite;

    public int Handled => Processed + Skipped;

    public int Failed => Crashed + DownloadErrors + Skipped;

    public static ScrapeBatchCounters Empty { get; } =
        new(0, 0, 0, 0, 0, 0, 0, 0);

    public ScrapeBatchCounters Add(ScrapeBatchCounters other) =>
        new(
            Total + other.Total,
            Processed + other.Processed,
            ScrapedSuccess + other.ScrapedSuccess,
            Crashed + other.Crashed,
            DownloadErrors + other.DownloadErrors,
            SkippedByRobotsTxt + other.SkippedByRobotsTxt,
            SkippedByCrawlDelay + other.SkippedByCrawlDelay,
            SkippedByBlockedWebsite + other.SkippedByBlockedWebsite);
}
