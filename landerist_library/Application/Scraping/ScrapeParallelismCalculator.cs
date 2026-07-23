using landerist_library.Pages;

namespace landerist_library.Application.Scraping;

public sealed class ScrapeParallelismCalculator
{
    private readonly ScraperExecutionOptions _options;

    public ScrapeParallelismCalculator(ScraperExecutionOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
    }

    public int Calculate(IReadOnlyCollection<Page> pages)
    {
        ArgumentNullException.ThrowIfNull(pages);

        if (_options.IsLocal || pages.Count <= 1)
        {
            return 1;
        }

        var configuredMaximum = _options.MaximumDegreeOfParallelism;
        var maximum = configuredMaximum < 1
            ? pages.Count
            : Math.Min(configuredMaximum, pages.Count);
        if (maximum <= 1)
        {
            return 1;
        }

        var distinctHostCount = pages
            .Select(static page => page.Host)
            .Distinct()
            .Count();
        if (distinctHostCount == pages.Count)
        {
            return maximum;
        }

        var wavesBeforeSameHost = (int)Math.Ceiling(
            (double)_options.MinimumHostThrottleSeconds /
            _options.EstimatedMinimumScrapeSeconds);
        var hostLimitedMaximum = Math.Max(
            1,
            distinctHostCount / wavesBeforeSameHost);

        return Math.Min(maximum, hostLimitedMaximum);
    }
}
