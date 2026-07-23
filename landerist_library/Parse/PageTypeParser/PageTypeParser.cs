using landerist_library.Application.Listings;
using landerist_library.Application.Scraping;
using landerist_library.Pages;
using landerist_library.Parse.ListingParser;

namespace landerist_library.Parse.PageTypeParser
{
    public class PageTypeParser
    {
        private Page Page { get; }
        private readonly bool _isProduction;
        private readonly INotListingCacheService _notListingCache;
        private readonly IPageClassificationMetrics _metrics;

        public PageTypeParser(
            Page page,
            bool isProduction,
            INotListingCacheService notListingCache,
            IPageClassificationMetrics metrics)
        {
            ArgumentNullException.ThrowIfNull(page);
            ArgumentNullException.ThrowIfNull(notListingCache);
            ArgumentNullException.ThrowIfNull(metrics);
            Page = page;
            _isProduction = isProduction;
            _notListingCache = notListingCache;
            _metrics = metrics;
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

            if (Page.ContainsMetaRobotsNoIndex())
            {
                return (PageType.NotIndexable, null, false);
            }

            if (Page.NotCanonical())
            {
                return (PageType.NotCanonical, null, false);
            }

            if (Page.IncorrectLanguage())
            {
                return (PageType.IncorrectLanguage, null, false);
            }

            Page.SetListingParserInput();

            if (Page.MatchesWebsiteListingUnavailableRule())
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

            //if (Page.ListingParserInputIsAnotherListingInHost())
            //{
            //    GlobalStatistics.InsertDailyCounter(StatisticsKey.ListingParserInputIsAnotherListingInHost);
            //    HostStatistics.InsertDailyCounter(Page.Host, HostStatisticsKey.ListingParserInputIsAnotherListingInHost);
            //    return (PageType.ResponseBodyRepeatedInHost, null, false);
            //}
            if (Tokenizer.TooManyTokens(Page))
            {
                return (PageType.ResponseBodyTooManyTokens, null, false);
            }

            return ParseListing.Parse(Page);
        }
    }
}
