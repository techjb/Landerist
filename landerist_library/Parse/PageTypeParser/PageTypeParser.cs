using landerist_library.Index;
using landerist_library.Parse.Listing.ChatGPT;
using landerist_library.Websites;

namespace landerist_library.Parse.PageTypeParser
{
    public class PageTypeParser
    {
        public static (PageType? pageType, landerist_orels.ES.Listing? listing) GetPageType(Page page)
        {
            if (page == null || page.ResponseBodyIsNullOrEmpty() || page.HttpStatusCode != 200)
            {
                return (PageType.DownloadError, null);
            }
            if (page.MainPage())
            {
                return (PageType.MainPage, null);
            }
            if (page.ContainsMetaRobotsNoIndex())
            {
                return (PageType.NotIndexable, null);
            }
            if (page.NotCanonical())
            {
                return (PageType.NotCanonical, null);
            }
            if (LastSegment.NotListingByLastSegment(page.Uri))
            {
                return (PageType.NotListingByLastSegment, null);
            }
            if (page.IncorrectLanguage())
            {
                return (PageType.IncorrectLanguage, null);
            }

            page.SetResponseBodyText();

            if (page.ResponseBodyTextHasNotChanged())
            {
                return (page.PageType, null);
            }
            if (page.ResponseBodyTextIsError())
            {
                return (PageType.ResponseBodyIsError, null);
            }
            if (page.ResponseBodyTextIsTooShort())
            {
                return (PageType.ResponseBodyTooShort, null);
            }
            if (page.ResponseBodyTextIsTooLarge())
            {
                return (PageType.ResponseBodyTooLarge, null);
            }
            if (page.ReponseBodyTextRepeatedInHost())
            {
                return (PageType.ResponseBodyRepeatedInHost, null);
            }
            if (ParseListingRequest.TooManyTokens(page.ResponseBodyText))
            {
                return (PageType.ResponseBodyTooManyTokens, null);
            }
            if (ListingSimilarity.HtmlNotSimilarToListing(page))
            {
                return (PageType.HtmlNotSimilarToListing, null);
            }
            return new ParseListingRequest().Parse(page);
        }
    }
}
