namespace landerist_library.Application.Scraping;

public sealed class PageSelectionOptions
{
    public PageSelectionOptions(
        int maximumPages,
        int maximumPagesPerHost,
        int minimumPages,
        bool enforceMinimumPages)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maximumPages, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(maximumPagesPerHost, 1);
        ArgumentOutOfRangeException.ThrowIfNegative(minimumPages);

        MaximumPages = maximumPages;
        MaximumPagesPerHost = maximumPagesPerHost;
        MinimumPages = minimumPages;
        EnforceMinimumPages = enforceMinimumPages;
    }

    public int MaximumPages { get; }

    public int MaximumPagesPerHost { get; }

    public int MinimumPages { get; }

    public bool EnforceMinimumPages { get; }
}
