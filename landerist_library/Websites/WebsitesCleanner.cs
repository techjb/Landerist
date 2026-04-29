using landerist_library.Database;
using landerist_library.Pages;
using landerist_orels.ES;

namespace landerist_library.Websites
{
    public class WebsitesCleanner
    {
        private const string EngelVoelkersHost = "www.engelvoelkers.com";

        public static void DeleteEngelVoelkersPagesDiscardedByIndexUrlRegex()
        {
            DeletePagesDiscardedByIndexUrlRegex(EngelVoelkersHost);
        }

        public static void DeletePagesDiscardedByIndexUrlRegex(string host)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(host);

            Website website = new(host);
            try
            {
                if (string.IsNullOrWhiteSpace(website.IndexUrlRegex))
                {
                    throw new InvalidOperationException("IndexUrlRegex is empty for host: " + host);
                }

                List<Page> pages = website.GetPages();
                int total = pages.Count;
                int processed = 0;
                int deleted = 0;
                int errors = 0;
                int skipped = 0;

                foreach (Page page in pages)
                {
                    try
                    {
                        if (!website.IsDiscardedByIndexUrlRegex(page.Uri))
                        {
                            skipped++;
                            continue;
                        }

                        if (page.Delete())
                        {
                            deleted++;
                        }
                        else
                        {
                            errors++;
                        }
                    }
                    finally
                    {
                        processed++;
                        Console.WriteLine(
                            "Processed: " + processed + "/" + total + " " +
                            "Deleted: " + deleted + " " +
                            "Errors: " + errors + " " +
                            "Skipped: " + skipped + " " +
                            page.Uri);

                       
                    }
                }

                Console.WriteLine(
                    "Finished deleting pages discarded by IndexUrlRegex. " +
                    "Host: " + host + " " +
                    "Deleted: " + deleted + " " +
                    "Errors: " + errors + " " +
                    "Skipped: " + skipped);
            }
            finally
            {
                website.Dispose();
            }
        }
       
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
                    if (website.GetNumListings() > 0 || website.ApplySpecialRules)
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

        public static void DeleteWebsitesWithoutPublishedListings()
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
                    if (HasPublishedListings(website) || website.ApplySpecialRules)
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
                    if (HasPageTypeListing(website) || website.ApplySpecialRules)
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

        private static bool HasPublishedListings(Website website)
        {
            string query =
                "SELECT 1 " +
                "FROM " + Pages.Pages.PAGES + " " +
                "WHERE [Host] = @Host AND [PageType] = @PageType AND [ListingStatus] = @ListingStatus";

            return new DataBase().QueryExists(query, new Dictionary<string, object?> {
                { "Host", website.Host },
                { "PageType", PageType.Listing.ToString() },
                { "ListingStatus", ListingStatus.published.ToString() }
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
