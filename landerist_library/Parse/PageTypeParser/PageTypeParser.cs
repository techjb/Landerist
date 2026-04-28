using landerist_library.Pages;
using landerist_library.Parse.ListingParser;
using landerist_library.Statistics;

namespace landerist_library.Parse.PageTypeParser
{
    public class PageTypeParser
    {
        private Page Page { get; }

        public PageTypeParser(Page page)
        {
            ArgumentNullException.ThrowIfNull(page);
            Page = page;
        }

        public (PageType? pageType, landerist_orels.ES.Listing? listing, bool waitingAIRequest)
            GetPageType()
        {

            var isProduction = Configuration.Config.IsConfigurationProduction();            
            //var isProduction = true;

            if (Page.HttpStatusCode is null)
            {
                return (PageType.Timeout, null, false);
            }

            if (Page.HttpStatusCode != 200)
            {
                return (PageType.HttpStatusCodeNotOK, null, false);
            }

            if (Page.RedirectToAnotherUrl())
            {
                return (PageType.RedirectToAnotherUrl, null, false);
            }

            if (Page.PageType.HasValue && Page.EtagHasNotChanged())
            {
                GlobalStatistics.InsertDailyCounter(StatisticsKey.EtagHasNotChanged);
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

            if (Page.IsNotListingCache() && isProduction)
            {
                GlobalStatistics.InsertDailyCounter(StatisticsKey.NotListingCache);
                HostStatistics.InsertDailyCounter(Page.Host, HostStatisticsKey.NotListingCache);
                return (PageType.NotListingByCache, null, false);
            }

            if (Page.ListingParserInputHasNotChanged() && isProduction)
            {
                GlobalStatistics.InsertDailyCounter(StatisticsKey.ListingParserInputAlreadyParsed);
                HostStatistics.InsertDailyCounter(Page.Host, HostStatisticsKey.ListingParserInputAlreadyParsed);
                return (Page.PageType, null, false);
            }

            if (Page.ListingParserInputIsAnotherListingInHost())
            {
                GlobalStatistics.InsertDailyCounter(StatisticsKey.ListingParserInputIsAnotherListingInHost);
                HostStatistics.InsertDailyCounter(Page.Host, HostStatisticsKey.ListingParserInputIsAnotherListingInHost);
                return (PageType.ResponseBodyRepeatedInHost, null, false);
            }

            if (Tokenizer.TooManyTokens(Page))
            {
                return (PageType.ResponseBodyTooManyTokens, null, false);
            }

            return ParseListing.Parse(Page);
        }
    }
}
