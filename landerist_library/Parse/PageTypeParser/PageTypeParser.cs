using landerist_library.Parse.ListingParser;
using landerist_library.Websites;

namespace landerist_library.Parse.PageTypeParser
{
    public class PageTypeParser
    {
        public static (PageType? pageType, landerist_orels.ES.Listing? listing, bool waitingAIRequest)
            GetPageType(Page page)
        {
            if (page == null || page.ResponseBodyIsNullOrEmpty() || page.HttpStatusCode != 200)
            {
                return (PageType.DownloadError, null, false);
            }
            if (page.MainPage())
            {
                return (PageType.MainPage, null, false);
            }
            if (page.ContainsMetaRobotsNoIndex())
            {
                return (PageType.NotIndexable, null, false);
            }
            if (page.NotCanonical())
            {
                return (PageType.NotCanonical, null, false);
            }            
            if (page.IncorrectLanguage())
            {
                return (PageType.IncorrectLanguage, null, false);
            }

            page.SetResponseBodyText();

            if (page.ResponseBodyTextHasNotChanged())
            {
                return (page.PageType, null, false);
            }
            if (page.ResponseBodyTextIsError())
            {
                return (PageType.ResponseBodyIsError, null, false);
            }
            if (page.ResponseBodyTextIsTooShort())
            {
                return (PageType.ResponseBodyTooShort, null, false);
            }
            if (page.ResponseBodyTextIsTooLarge())
            {
                return (PageType.ResponseBodyTooLarge, null, false);
            }
            if (page.ReponseBodyTextRepeatedInHost())
            {
                return (PageType.ResponseBodyRepeatedInHost, null, false);
            }
            if (page.ReponseBodyTextRepeatedInListings())
            {
                return (PageType.ResponseBodyRepeatedInListings, null, false);
            }
            if (ParseListing.TooManyTokens(page))
            {
                return (PageType.ResponseBodyTooManyTokens, null, false);
            }
            //if (ListingSimilarity.HtmlNotSimilarToListing(page))
            //{
            //    return (PageType.HtmlNotSimilarToListing, null, false);
            //}
            return ParseListing.Parse(page);
        }
    }
}
