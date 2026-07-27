using landerist_library.Application.Listings;
using landerist_library.Application.Scraping;
using landerist_library.Pages;

namespace landerist_library.Application.Parsing
{
    public class PageTypeParser
    {
        private Page Page { get; }
        private readonly bool _isProduction;
        private readonly INotListingCacheService _notListingCache;
        private readonly IPageClassificationMetrics _metrics;
        private readonly IListingPageParser _listingParser;
        private readonly IPageTokenLimitPolicy _tokenLimitPolicy;
        private readonly IPageContentInspector _contentInspector;

        public PageTypeParser(
            Page page,
            bool isProduction,
            INotListingCacheService notListingCache,
            IPageClassificationMetrics metrics,
            IListingPageParser listingParser,
            IPageTokenLimitPolicy tokenLimitPolicy,
            IPageContentInspector contentInspector)
        {
            ArgumentNullException.ThrowIfNull(page);
            ArgumentNullException.ThrowIfNull(notListingCache);
            ArgumentNullException.ThrowIfNull(metrics);
            ArgumentNullException.ThrowIfNull(listingParser);
            ArgumentNullException.ThrowIfNull(tokenLimitPolicy);
            ArgumentNullException.ThrowIfNull(contentInspector);
            Page = page;
            _isProduction = isProduction;
            _notListingCache = notListingCache;
            _metrics = metrics;
            _listingParser = listingParser;
            _tokenLimitPolicy = tokenLimitPolicy;
            _contentInspector = contentInspector;
        }

        public (PageType? pageType, landerist_orels.ES.Listing? listing, bool waitingAIRequest)
            GetPageType()
        {

            if (Page.HttpStatusCode is null)
            {
                return (PageType.HttpStatusCodeNull, null, false);
            }

            if (Page.HttpStatusCode == 404)
            {
                return (PageType.HttpStatusCodeNotFound, null, false);
            }

            if (Page.HttpStatusCode == 410)
            {
                return (PageType.HttpStatusCodeGone, null, false);
            }

            if (Page.HttpStatusCode != 200)
            {
                return (PageType.HttpStatusCodeOtherNotOK, null, false);
            }

            if (Page.RedirectToAnotherUrl())
            {
                return (PageType.RedirectToAnotherUrl, null, false);
            }

            if (Page.PageType.HasValue && Page.DownloadedHeadersHaveNotChanged() && _isProduction)
            {
                _metrics.RecordPageNotModified(Page);
                return (Page.PageType, null, false);
            }

            if (Page.ResponseBodyIsNullOrEmpty())
            {
                return (PageType.ResponseBodyNullOrEmpty, null, false);
            }

            if (Page.IsMainPage())
            {
                return (PageType.MainPage, null, false);
            }

            if (Page.Website.IsDiscardedByListingUrlRegex(Page.Uri))
            {
                return (PageType.DiscardedByListingUrlRegex, null, false);
            }

            if (_contentInspector.ContainsMetaRobotsNoIndex(Page))
            {
                return (PageType.NotIndexable, null, false);
            }

            if (_contentInspector.IsNotCanonical(Page))
            {
                return (PageType.NotCanonical, null, false);
            }

            if (_contentInspector.HasIncorrectLanguage(Page))
            {
                return (PageType.IncorrectLanguage, null, false);
            }

            _contentInspector.PrepareListingParserInput(Page);

            if (_contentInspector.MatchesListingUnavailableRule(Page))
            {
                return (PageType.NotListingByWebsiteRule, null, false);
            }

            if (Page.ListingParserInputIsTooShort())
            {
                return (PageType.ResponseBodyTooShort, null, false);
            }

            if (Page.ListingParserInputIsTooLarge())
            {
                return (PageType.ResponseBodyTooLarge, null, false);
            }

            if (Page.ListingParserInputIsError())
            {
                return (PageType.ResponseBodyIsError, null, false);
            }

            if (_notListingCache.Contains(Page) && _isProduction)
            {
                _metrics.RecordNotListingCache(Page);
                return (PageType.NotListingByCache, null, false);
            }

            if (Page.ListingParserInputHasNotChanged() && _isProduction)
            {
                _metrics.RecordListingInputAlreadyParsed(Page);
                return (Page.PageType, null, false);
            }
            if (_tokenLimitPolicy.TooManyTokens(Page))
            {
                return (PageType.ResponseBodyTooManyTokens, null, false);
            }

            return _listingParser.Parse(Page);
        }
    }
}
