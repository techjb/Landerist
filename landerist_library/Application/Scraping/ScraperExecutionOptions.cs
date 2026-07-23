namespace landerist_library.Application.Scraping;

public sealed class ScraperExecutionOptions
{
    public ScraperExecutionOptions(
        bool isProduction,
        bool isLocal,
        int maximumDegreeOfParallelism,
        int estimatedMinimumScrapeSeconds = 2,
        int minimumHostThrottleSeconds = 3)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(estimatedMinimumScrapeSeconds, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(minimumHostThrottleSeconds, 1);

        IsProduction = isProduction;
        IsLocal = isLocal;
        MaximumDegreeOfParallelism = maximumDegreeOfParallelism;
        EstimatedMinimumScrapeSeconds = estimatedMinimumScrapeSeconds;
        MinimumHostThrottleSeconds = minimumHostThrottleSeconds;
    }

    public bool IsProduction { get; }

    public bool IsLocal { get; }

    public int MaximumDegreeOfParallelism { get; }

    public int EstimatedMinimumScrapeSeconds { get; }

    public int MinimumHostThrottleSeconds { get; }
}
