using landerist_library.Index;
using landerist_library.Parse.Listing.ChatGPT;
using landerist_library.Websites;

namespace landerist_library.Parse.PageTypeParser
{
    public class PageTypeParser
    {
        public static (PageType? pageType, landerist_orels.ES.Listing? listing) GetPageType(Page page)
        {
            if (page == null || page.ResponseBodyIsNullOrEmpty())
            {
                return (PageType.DownloadError, null);
            }
            if (page.IsMainPage())
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
            if (IncorrectLanguage(page))
            {
                return (PageType.IncorrectLanguage, null);
            }

            page.SetResponseBodyText();

            if (ResponseBodyTextHasNotChanged(page))
            {
                return (page.PageType, null);
            }

            if (ResponseBodyIsError(page.ResponseBodyText))
            {
                return (PageType.ResponseBodyIsError, null);
            }
            if (ResponseBodyIsTooShort(page.ResponseBodyText))
            {
                return (PageType.ResponseBodyTooShort, null);
            }
            if (ResponseBodyIsTooLarge(page.ResponseBodyText))
            {
                return (PageType.ResponseBodyTooLarge, null);
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

        private static bool ResponseBodyTextHasNotChanged(Page page)
        {
            return
                !page.ResponseBodyTextHasChanged &&
                page.PageType != null &&
                !page.PageType.Equals(PageType.MayBeListing) &&
                Configuration.Config.IsConfigurationProduction();
        }

        private static bool IncorrectLanguage(Page page)
        {
            var htmlDocument = page.GetHtmlDocument();
            if (htmlDocument != null)
            {
                var htmlNode = htmlDocument.DocumentNode.SelectSingleNode("/html");
                if (htmlNode != null)
                {
                    var lang = htmlNode.Attributes["lang"];
                    if (lang != null)
                    {
                        return !LanguageValidator.IsValidLanguageAndCountry(page.Website, lang.Value);
                    }
                }
            }
            return false;
        }

        private static bool ResponseBodyIsError(string? responseBodyText)
        {
            if (responseBodyText == null)
            {
                return false;
            }
            return
                responseBodyText.StartsWith("Not found", StringComparison.OrdinalIgnoreCase) ||
                responseBodyText.StartsWith("Error", StringComparison.OrdinalIgnoreCase) ||
                responseBodyText.StartsWith("404", StringComparison.OrdinalIgnoreCase) ||
                responseBodyText.Contains("no encontrada", StringComparison.OrdinalIgnoreCase) ||
                responseBodyText.Contains("no existe", StringComparison.OrdinalIgnoreCase) ||
                responseBodyText.Contains("algo salió mal", StringComparison.OrdinalIgnoreCase) ||
                responseBodyText.Contains("Page Not found", StringComparison.OrdinalIgnoreCase)
                ;
        }

        private static bool ResponseBodyIsTooLarge(string? responseBodyText)
        {
            if (responseBodyText == null)
            {
                return false;
            }
            return responseBodyText.Length > Configuration.Config.MAX_RESPONSEBODYTEXT_LENGTH;
        }

        private static bool ResponseBodyIsTooShort(string? responseBodyText)
        {
            if (string.IsNullOrEmpty(responseBodyText))
            {
                return true;
            }
            return responseBodyText.Length < Configuration.Config.MIN_RESPONSEBODYTEXT_LENGTH;
        }
    }
}
