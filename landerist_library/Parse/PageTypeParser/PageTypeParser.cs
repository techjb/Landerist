using landerist_library.Index;
using landerist_library.Parse.Listing.ChatGPT;
using landerist_library.Websites;

namespace landerist_library.Parse.PageTypeParser
{
    public class PageTypeParser
    {
        private static int Total;
        private static int Counter;
        private static int ListingsCounter;
        private static int NotListingsCounter;
        private static int ErrorsCounter;

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
            if (!page.IsIndexable())
            {
                return (PageType.NotIndexable, null);
            }
            if (LastSegment.IsNotListingByLastSegment(page.Uri))
            {
                return (PageType.NotListingByLastSegment, null);
            }
            if (!IsCorrectLanguage(page))
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
            if (!IsListingRequest.IsLengthAllowed(page.ResponseBodyText))
            {
                return (PageType.ResponseBodyTooManyTokens, null);
            }
            if (!ListingSimilarity.IsSimilarHtml(page))
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

        private static bool IsCorrectLanguage(Page page)
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
                        return LanguageValidator.IsValidLanguageAndCountry(page.Website, lang.Value);
                    }
                }
            }
            return true;
        }

        private static bool ResponseBodyIsError(string? responseBodyText)
        {
            if (responseBodyText == null)
            {
                return false;
            }
            return
                responseBodyText.StartsWith("Error", StringComparison.OrdinalIgnoreCase) ||
                responseBodyText.StartsWith("404", StringComparison.OrdinalIgnoreCase) ||
                responseBodyText.Contains("Página no encontrada", StringComparison.OrdinalIgnoreCase) ||
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

        public static void MayBeListingToIsListing()
        {
            Console.WriteLine("Reading MayBeListing pages..");
            var pages = Pages.GetPages(PageType.MayBeListing);
            Total = pages.Count;
            Counter = 0;
            ListingsCounter = 0;
            NotListingsCounter = 0;
            ErrorsCounter = 0;
            Parallel.ForEach(pages,
                //new ParallelOptions() { MaxDegreeOfParallelism = 1 },
                page =>
            {
                MayBeListingToIsListing(page);
                //Thread.Sleep(8000); // openia limits
            });
        }

        public static void MayBeListingToIsListing(Page page)
        {
            bool? IsListing = new IsListingRequest().IsListing(page.ResponseBodyText);
            Interlocked.Increment(ref Counter);
            if (IsListing.HasValue)
            {
                PageType? pageType = (bool)IsListing ? PageType.Listing : PageType.NotListingByParser;
                page.Update(pageType);
                switch (pageType)
                {
                    case PageType.Listing: Interlocked.Increment(ref ListingsCounter); break;
                    case PageType.NotListingByParser: Interlocked.Increment(ref NotListingsCounter); break;
                }
            }
            else
            {
                Interlocked.Increment(ref ErrorsCounter);
            }

            var percentageTotal = Counter * 100 / Total;
            var percentageListings = ListingsCounter * 100 / Counter;
            var percentageNotListings = NotListingsCounter * 100 / Counter;
            var percentageErrors = ErrorsCounter * 100 / Counter;

            Console.WriteLine(
                Counter + "/" + Total + " (" + percentageTotal + "%) " +
                "Listing: " + ListingsCounter + " (" + percentageListings + "%) " +
                "NotListing: " + NotListingsCounter + " (" + percentageNotListings + "%) " +
                "Errors: " + ErrorsCounter + " (" + percentageErrors + "%) "
                );
        }
    }
}
