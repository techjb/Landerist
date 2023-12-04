using landerist_library.Parse.Listing.ChatGPT;
using landerist_library.Websites;
using landerist_library.Database;


namespace landerist_library.Parse.Listing
{
    public class ListingsParser
    {
        private static int Total;
        private static int Counter;
        private static int SucessCounter;
        private static int ListingsCounter;
        private static int ErrorsCounter;

        public static void Start()
        {
            Console.WriteLine("Reading Listings pages ..");
            var pages = Pages.GetPages(PageType.PageType.Listing);
            pages = FilterNewListings(pages);
            Console.WriteLine("Parsing listings ..");
            Total = pages.Count;
            Counter = 0;
            SucessCounter = 0;
            ListingsCounter = 0;
            ErrorsCounter = 0;
            Parallel.ForEach(pages,
                //new ParallelOptions() { MaxDegreeOfParallelism = 1 },
                page =>
                {
                    ParseListing(page);
                    //Thread.Sleep(8000); // openia limits
                });
        }

        private static List<Page> FilterNewListings(List<Page> pages)
        {
            Console.WriteLine("Filtering new pages ..");
            List<Page> list = new();
            var sync = new object();
            Parallel.ForEach(pages, page =>
            {
                if (!page.ContainsListing())
                {
                    lock (sync)
                    {
                        list.Add(page);
                    }
                }
            });
            return list;
        }

        public static void ParseListing(Page page)
        {
            var result = new ParseListingRequest().Parse(page);
            Interlocked.Increment(ref Counter);
            if (result.Item1)
            {
                Interlocked.Increment(ref SucessCounter);
                if(result.Item2 != null)
                {
                    Interlocked.Increment(ref ListingsCounter);
                    ES_Listings.InsertUpdate(page.Website, result.Item2);
                }
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
            var percentageParsed = SucessCounter * 100 / Counter;
            var percentageErrors = ErrorsCounter * 100 / Counter;
            var percentageListings = ListingsCounter * 100 / SucessCounter;

            Console.WriteLine(
                Counter + "/" + Total + " (" + percentageTotal + "%) " +
                "Sucess: " + SucessCounter + " (" + percentageParsed + "%) " +
                "Errors: " + ErrorsCounter + " (" + percentageErrors + "%) " +
                "Listings: " + ListingsCounter + " (" + percentageListings + "%) "
                );
        }
    }
}
