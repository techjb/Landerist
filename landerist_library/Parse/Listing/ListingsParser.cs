using landerist_library.Parse.Listing.ChatGPT;
using landerist_library.Websites;
using landerist_library.Database;


namespace landerist_library.Parse.Listing
{
    public class ListingsParser
    {
        private static int Total;
        private static int Counter;
        private static int ListingsCounter;

        public static void Start()
        {
            Console.WriteLine("Reading MayBeListing pages ..");
            var pages = Pages.GetPages(PageType.MayBeListing);
            Console.WriteLine("Parsing listings ..");
            Total = pages.Count;
            Counter = 0;
            ListingsCounter = 0;
            Parallel.ForEach(pages,
                //new ParallelOptions() { MaxDegreeOfParallelism = 1 },
                page =>
                {
                    ParseListing(page);
                    //Thread.Sleep(8000); // openia limits
                });
        }

        public static void ParseListing(Page page)
        {
            var (pageType, listing) = new ParseListingRequest().Parse(page);
            Interlocked.Increment(ref Counter);
            page.Update(pageType);
            if (listing != null)
            {
                Interlocked.Increment(ref ListingsCounter);
                ES_Listings.InsertUpdate(page.Website, listing);
            }
            ConsoleOutput();
        }

        private static void ConsoleOutput()
        {
            var percentageTotal = Counter * 100 / Total;
            var percentageListings = ListingsCounter * 100 / Counter;

            Console.WriteLine(
                Counter + "/" + Total + " (" + percentageTotal + "%) " +
                "Listings: " + ListingsCounter + " (" + percentageListings + "%) ");
        }
    }
}
