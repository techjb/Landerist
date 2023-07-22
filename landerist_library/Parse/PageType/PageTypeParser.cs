using landerist_library.Parse.Listing.ChatGPT;
using landerist_library.Websites;
using landerist_orels.ES;


namespace landerist_library.Parse.PageType
{
    public enum PageType
    {
        DownloadError,
        IncorrectLanguage,
        BlockedByRobotsTxt,
        RobotsTxtCrawlDelayTooBig,
        MainPage,
        NotIndexable,
        ForbiddenLastSegment,
        ResponseBodyError,
        ResponseBodyTooLarge,
        ResponseBodyTooShort,
        ResponseBodyValid,
        MayContainListing,
        Listing,
        NotListing,
    };

    public class PageTypeParser
    {
        private static int Total;
        private static int Counter;
        private static int ListingsCounter;
        private static int NotListingsCounter;
        private static int ErrorsCounter;

        public static PageType? GetPageType(Page page)
        {
            if (page == null)
            {
                return null;
            }
            if (page.IsMainPage())
            {
                return PageType.MainPage;
            }
            if (!page.CanIndexContent())
            {
                return PageType.NotIndexable;
            }
            if (LastSegment.LastSegmentIsForbidden(page.Uri))
            {
                return PageType.ForbiddenLastSegment;
            }

            page.SetResponseBodyText();

            if (ResponseBodyIsError(page.ResponseBodyText))
            {
                return PageType.ResponseBodyError;
            }
            if (ResponseBodyIsTooLarge(page.ResponseBodyText))
            {
                return PageType.ResponseBodyTooLarge;
            }
            if (ResponseBodyIsTooShort(page.ResponseBodyText))
            {
                return PageType.ResponseBodyTooShort;
            }

            return PageType.ResponseBodyValid;
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

        public static void ResponseBodyValidToIsListing()
        {
            var pages = Pages.GetPages(PageType.ResponseBodyValid);
            Total = pages.Count;
            Counter = 0;
            ListingsCounter = 0;
            NotListingsCounter = 0;
            ErrorsCounter = 0;
            Parallel.ForEach(pages,
                //new ParallelOptions() { MaxDegreeOfParallelism = 1 },
                page =>
            {
                ResponseBodyValidToIsListing(page);
                Thread.Sleep(1000);
            });
        }

        public static void ResponseBodyValidToIsListing(Page page)
        {
            bool? IsListing = new ChatGPTIsListing().IsListing(page.ResponseBodyText);
            Interlocked.Increment(ref Counter);
            if (IsListing.HasValue)
            {
                PageType? pageType = (bool)IsListing ? PageType.Listing : PageType.NotListing;
                page.Update(pageType);
                switch (pageType)
                {
                    case PageType.Listing: Interlocked.Increment(ref ListingsCounter); break;
                    case PageType.NotListing: Interlocked.Increment(ref NotListingsCounter); break;
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
