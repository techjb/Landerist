using landerist_library.Application.Listings;
using landerist_library.Application.Scraping;
using landerist_library.Pages;
using landerist_library.Parse.PageTypeParser;
using landerist_library.Statistics;

namespace landerist_library.Infrastructure.Scraping;

public sealed class PageContentClassifier : IPageContentClassifier
{
    private readonly bool _isProduction;
    private readonly INotListingCacheService _notListingCache;
    private readonly IPageClassificationMetrics _metrics;
    private readonly HostStatistics _hostStatistics;

    public PageContentClassifier(
        bool isProduction,
        INotListingCacheService notListingCache,
        IPageClassificationMetrics metrics,
        HostStatistics hostStatistics)
    {
        ArgumentNullException.ThrowIfNull(notListingCache);
        ArgumentNullException.ThrowIfNull(metrics);
        ArgumentNullException.ThrowIfNull(hostStatistics);
        _isProduction = isProduction;
        _notListingCache = notListingCache;
        _metrics = metrics;
        _hostStatistics = hostStatistics;
    }

    public PageClassificationResult Classify(Page page)
    {
        var result = new PageTypeParser(
            page,
            _isProduction,
            _notListingCache,
            _metrics,
            _hostStatistics).GetPageType();
        return new PageClassificationResult(result.pageType, result.listing, result.waitingAIRequest);
    }
}
