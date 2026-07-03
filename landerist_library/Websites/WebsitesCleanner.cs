using landerist_library.Infrastructure.Sql;
using landerist_library.Pages;

namespace landerist_library.Websites
{
    public class WebsitesCleanner
    {
        private const string EngelVoelkersHost = "www.engelvoelkers.com";
        private static readonly WebsitePageMetricsRepository PageMetrics = new();

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
                    if (SpecialRulesApplyToAllWebsites() || website.GetNumListings() > 0)
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
                    if (SpecialRulesApplyToAllWebsites() || HasPublishedListings(website))
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
                    if (SpecialRulesApplyToAllWebsites() || HasPageTypeListing(website))
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


        private static bool SpecialRulesApplyToAllWebsites()
        {
            return true;
        }

        private static bool HasPageTypeListing(Website website)
        {
            return PageMetrics.HasPageTypeListing(website.Host);
        }

        private static bool HasPublishedListings(Website website)
        {
            return PageMetrics.HasPublishedListings(website.Host);
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
                    if (SpecialRulesApplyToAllWebsites() || website.GetNumPages() >= minimumPages || website.GetNumListings() > 0)
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
