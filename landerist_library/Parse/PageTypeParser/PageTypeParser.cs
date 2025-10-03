using landerist_library.Parse.ListingParser;
using landerist_library.Statistics;
using landerist_library.Websites;

namespace landerist_library.Parse.PageTypeParser
{
    public class PageTypeParser
    {
        private Page Page { get; }
        public PageTypeParser(Page page) => Page = page;

        public (PageType? pageType, landerist_orels.ES.Listing? listing, bool waitingAIRequest)
            GetPageType()
        {
            if (Page.HttpStatusCode != 200)
            {
                return (PageType.HttpStatusCodeNotOK, null, false);
            }
            if (Page.RedirectToAnotherUrl())
            {
                return (PageType.RedirectToAnotherUrl, null, false);
            }
            if (Page.ResponseBodyIsNullOrEmpty())
            {
                return (PageType.ResponseBodyNullOrEmpty, null, false);
            }
            if (Page.IsMainPage())
            {
                return (PageType.MainPage, null, false);
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

            Page.SetResponseBodyText();

            if (Page.ResponseBodyTextIsTooShort())
            {
                return (PageType.ResponseBodyTooShort, null, false);
            }
            if (Page.ResponseBodyTextIsTooLarge())
            {
                return (PageType.ResponseBodyTooLarge, null, false);
            }
            if (Page.ResponseBodyTextIsError())
            {
                return (PageType.ResponseBodyIsError, null, false);
            }
            if (Page.IsNotListingCache() 
                && Configuration.Config.IsConfigurationProduction()
               )
            {
                StatisticsSnapshot.InsertDailyCounter(StatisticsKey.NotListingCache);
                return (PageType.NotListingByParser, null, false);
            }
            if (Page.ResponseBodyTextAlreadyParsed() && 
                Configuration.Config.IsConfigurationProduction()
                )
            {
                StatisticsSnapshot.InsertDailyCounter(StatisticsKey.ResponseBodyTextAlreadyParsed);
                return (Page.PageType, null, false);
            }
            if (Page.ReponseBodyTextIsAnotherListingInHost())
            {
                StatisticsSnapshot.InsertDailyCounter(StatisticsKey.ReponseBodyTextIsAnotherListingInHost);
                return (PageType.ResponseBodyRepeatedInHost, null, false);
            }
            if (ParseListingSystem.TooManyTokens(Page))
            {
                return (PageType.ResponseBodyTooManyTokens, null, false);
            }
            return ParseListing.Parse(Page);
        }
    }
}
