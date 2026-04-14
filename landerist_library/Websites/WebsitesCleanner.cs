using landerist_library.Database;
using landerist_library.Pages;

namespace landerist_library.Websites
{
    public class WebsitesCleanner
    {
       
        public static void DeleteWebsitesWithoutListings()
        {
            var websites = Websites.GetAll();
            int total = websites.Count;
            int deleted = 0;
            int errors = 0;
            int skipped = 0;
            int processed = 0;

            Parallel.ForEach(websites, website =>
            {
                try
                {
                    if (website.GetNumListings() > 0)
                    {
                        Interlocked.Increment(ref skipped);
                        return;
                    }

                    if (website.Delete())
                    {
                        Interlocked.Increment(ref deleted);
                    }
                    else
                    {
                        Interlocked.Increment(ref errors);
                    }
                }
                finally
                {
                    int current = Interlocked.Increment(ref processed);
                    Console.WriteLine("Processed: " + current + "/" + total + " Deleted: " + deleted + " Errors: " + errors + " Skipped: " + skipped);
                    website.Dispose();
                }
            });

            Console.WriteLine("Total websites borrados: " + deleted);
        }

        public static void DeleteWebsitesWithLessThanTenPages()
        {
            DeleteWebsitesWithLessThanPages(10);
        }

        public static void DeleteWebsitesWithoutPageTypeListing()
        {
            var websites = Websites.GetAll();
            int total = websites.Count;
            int deleted = 0;
            int errors = 0;
            int skipped = 0;
            int processed = 0;

            Parallel.ForEach(websites, 
                //new ParallelOptions { MaxDegreeOfParallelism = 1 }, 
                website =>
            {
                try
                {
                    if (HasPageTypeListing(website))
                    {
                        Interlocked.Increment(ref skipped);
                        return;
                    }

                    if (website.Delete())
                    {
                        Interlocked.Increment(ref deleted);
                    }
                    else
                    {
                        Interlocked.Increment(ref errors);
                    }
                }
                finally
                {
                    int current = Interlocked.Increment(ref processed);
                    Console.WriteLine("Processed: " + current + "/" + total + " Deleted: " + deleted + " Errors: " + errors + " Skipped: " + skipped);
                    website.Dispose();
                }
            });

            Console.WriteLine("Total websites borrados: " + deleted);
        }


        private static bool HasPageTypeListing(Website website)
        {
            string query =
                "SELECT 1 " +
                "FROM " + Pages.Pages.PAGES + " " +
                "WHERE [Host] = @Host AND [PageType] = @PageType";

            return new DataBase().QueryExists(query, new Dictionary<string, object?> {
                { "Host", website.Host },
                { "PageType", PageType.Listing.ToString() }
            });
        }


        public static void DeleteWebsitesWithLessThanPages(int minimumPages)
        {
            if (minimumPages < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumPages));
            }

            var websites = Websites.GetAll();
            int total = websites.Count;
            int deleted = 0;
            int errors = 0;
            int skipped = 0;
            int processed = 0;

            Parallel.ForEach(websites, website =>
            {
                try
                {
                    if (website.GetNumPages() >= minimumPages || website.GetNumListings() > 0)
                    {
                        Interlocked.Increment(ref skipped);
                        return;
                    }

                    if (website.Delete())
                    {
                        Interlocked.Increment(ref deleted);
                    }
                    else
                    {
                        Interlocked.Increment(ref errors);
                    }
                }
                finally
                {
                    int current = Interlocked.Increment(ref processed);
                    Console.WriteLine("Processed: " + current + "/" + total + " Deleted: " + deleted + " Errors: " + errors + " Skipped: " + skipped);
                    website.Dispose();
                }
            });

            Console.WriteLine("Total websites borrados: " + deleted);
        }

    }
}
