using landerist_library.Parse.Listing.ChatGPT;
using landerist_library.Websites;
using landerist_orels.ES;
using landerist_library.Database;


namespace landerist_library.Parse.Listing
{
    public class ListingsParser
    {
        private static int Total;
        private static int Counter;
        private static int ParsedCounter;
        private static int ErrorsCounter;

        public static void Start()
        {
            Console.WriteLine("Reading Listings pages..");
            var pages = Pages.GetPages(PageType.PageType.Listing);
            Total = pages.Count;
            Counter = 0;
            ParsedCounter = 0;
            ErrorsCounter = 0;
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
            var listing = new ParseListingRequest().Parse(page);
            Interlocked.Increment(ref Counter);
            if (listing != null)
            {
                Interlocked.Increment(ref ParsedCounter);
                ES_Listings.InsertUpdate(page.Website, listing);
            }
            else
            {
                Interlocked.Increment(ref ErrorsCounter);
            }

            ConsoleOutput();
        }

        private static void ConsoleOutput()
        {
            var percentageTotal = Counter * 100 / Total;
            var percentageParsed = ParsedCounter * 100 / Counter;
            var percentageErrors = ErrorsCounter * 100 / Counter;

            Console.WriteLine(
                Counter + "/" + Total + " (" + percentageTotal + "%) " +
                "Parsed: " + ParsedCounter + " (" + percentageParsed + "%) " +
                "Errors: " + ErrorsCounter + " (" + percentageErrors + "%) "
                );
        }
    }
}
