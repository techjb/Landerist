using landerist_library.Application.Listings;
using landerist_library.Application.Scraping;
using landerist_library.Pages;
using landerist_library.Application.Parsing;

namespace landerist_library.Infrastructure.Scraping;

public sealed class PageContentClassifier : IPageContentClassifier
{
    private readonly bool _isProduction;
    private readonly INotListingCacheService _notListingCache;
    private readonly IPageClassificationMetrics _metrics;
    private readonly IListingPageParser _listingParser;
    private readonly IPageTokenLimitPolicy _tokenLimitPolicy;
    private readonly IPageContentInspector _contentInspector;

    public PageContentClassifier(
        bool isProduction,
        INotListingCacheService notListingCache,
        IPageClassificationMetrics metrics,
        IListingPageParser listingParser,
        IPageTokenLimitPolicy tokenLimitPolicy,
        IPageContentInspector contentInspector)
    {
        ArgumentNullException.ThrowIfNull(notListingCache);
        ArgumentNullException.ThrowIfNull(metrics);
        ArgumentNullException.ThrowIfNull(listingParser);
        ArgumentNullException.ThrowIfNull(tokenLimitPolicy);
        ArgumentNullException.ThrowIfNull(contentInspector);
        _isProduction = isProduction;
        _notListingCache = notListingCache;
        _metrics = metrics;
        _listingParser = listingParser;
        _tokenLimitPolicy = tokenLimitPolicy;
        _contentInspector = contentInspector;
    }

    public PageClassificationResult Classify(Page page)
    {
        var result = new PageTypeParser(
            page,
            _isProduction,
            _notListingCache,
            _metrics,
            _listingParser,
            _tokenLimitPolicy,
            _contentInspector).GetPageType();
        return new PageClassificationResult(result.pageType, result.listing, result.waitingAIRequest);
    }
}
