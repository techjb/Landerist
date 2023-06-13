using landerist_library.Websites;
using System.Collections.Concurrent;

namespace landerist_library.Scrape
{
    public class Scraper
    {
        private static readonly IpHostBlocker IpHostBlocker = new();

        private static readonly object SyncTempBlocker = new();

        private static readonly HashSet<Page> PendingPages = new();

        private static readonly object SyncPendingPages = new();

        private static int ListingsCounter = 0;

        private static readonly object SyncCounter = new();

        private static readonly object SyncListingsCounter = new();

        public static void ScrapeNonScrapped(bool recursive = false)
        {
            List<Page> pages = Pages.GetNonScraped();
            if (pages.Count.Equals(0))
            {
                return;
            }
            Scrape(pages);
            if (recursive)
            {
                ScrapeNonScrapped(recursive);
            }
        }

        public static void ScrapeNonScrapped(Uri uri, bool recursive = false)
        {
            Website website = new(uri);
            ScrapeNonScrapped(website, recursive);
        }

        public static void ScrapeNonScrapped(Website website, bool recursive)
        {
            var pages = website.GetNonScrapedPages();
            if (pages.Count == 0)
            {
                return;
            }
            Scrape(pages);
            if (recursive)
            {
                ScrapeNonScrapped(website, recursive);
            }
        }

        public static void ScrapeUnknowHttpStatusCode(bool recursive = false)
        {
            List<Page> pages = Pages.GetUnknowHttpStatusCode();
            if (pages.Count.Equals(0))
            {
                return;
            }
            Scrape(pages);
            if (recursive)
            {
                ScrapeUnknowHttpStatusCode(recursive);
            }
        }

        // Can be infinite loops
        public static void ScrapeUnknowIsListing(Uri uri, bool recursive = false)
        {
            Website website = new(uri);
            ScrapeUnknowIsListing(website, recursive);
        }

        public static void ScrapeUnknowIsListing(Website website, bool recursive)
        {
            var pages = website.GetUnknowIsListingPages();
            if (pages.Count == 0)
            {
                return;
            }
            Scrape(pages);
            if (recursive)
            {
                ScrapeUnknowIsListing(website, recursive);
            }
        }

        public static void ScrapeIsNotListing(Uri uri, bool recursive = false)
        {
            Website website = new(uri);
            ScrapeIsNotListing(website, recursive);
        }

        public static void ScrapeIsNotListing(Website website, bool recursive)
        {
            var pages = website.GetIsNotListingPages();
            if (pages.Count == 0)
            {
                return;
            }
            Scrape(pages);
            if (recursive)
            {
                ScrapeIsNotListing(website, recursive);
            }
        }
        public static void ScrapeMainPage(Website website)
        {
            var page = new Page(website);
            Scrape(page);
        }

        public static void Scrape(Website website)
        {
            var pages = website.GetPages();
            Scrape(pages);
        }

        private static void Scrape(List<Page> pages)
        {
            HashSet<Page> hashSet = new(pages, new PageComparer());
            Scrape(hashSet);
        }

        //private static void Scrape(HashSet<Page> pages)
        //{
        //    pages.RemoveWhere(p => !p.CanScrape());
        //    PendingPages.Clear();
        //    int totalPages = pages.Count;
        //    int Counter = 0;
        //    DateTime dateStart = DateTime.Now;
        //    Parallel.ForEach(pages,
        //        //new ParallelOptions() { MaxDegreeOfParallelism = 1 },
        //        page =>
        //        {
        //            lock (SyncCounter)
        //            {
        //                Counter++;
        //            }
        //            Console.WriteLine(
        //                "Scraped: " + Counter + "/" + totalPages + " " +
        //                "Pending: " + PendingPages.Count + " " +
        //                "Listings: " + ListingsCounter);

        //            if (IsBlocked(page))
        //            {
        //                AddToPendingPages(page);
        //                return;
        //            }
        //            Scrape(page);
        //        });

        //    ScrapePendingPages(dateStart);
        //}


        private static void Scrape(HashSet<Page> pages)
        {
            var blockingCollection = GetBlockingCollection(pages);
            int Counter = 0;
            DateTime dateStart = DateTime.Now;
            Parallel.ForEach(blockingCollection.GetConsumingEnumerable(),
                //new ParallelOptions() { MaxDegreeOfParallelism = 1 },
                page =>
                {
                    lock (SyncCounter)
                    {
                        Counter++;
                    }
                    Console.WriteLine(
                        "Scraped: " + Counter + "/" + blockingCollection.Count + " " +
                        "Listings: " + ListingsCounter);

                    if (IsBlocked(page))
                    {
                        blockingCollection.Add(page);
                        return;
                    }
                    Scrape(page);
                });
        }

        private static BlockingCollection<Page> GetBlockingCollection(HashSet<Page> pages)
        {
            pages.RemoveWhere(p => !p.CanScrape());
            BlockingCollection<Page> blockingCollection = new();
            foreach (var page in pages)
            {
                blockingCollection.Add(page);
            }
            return blockingCollection;
        }


        //private static void ScrapePendingPages(DateTime dateStart)
        //{
        //    if (PendingPages.Count.Equals(0))
        //    {
        //        return;
        //    }
        //    if (dateStart.AddSeconds(2) > DateTime.Now)
        //    {
        //        Thread.Sleep(2000);
        //    }
        //    HashSet<Page> newList = new(PendingPages);
        //    Scrape(newList);
        //}

        public static void Scrape(Uri uri)
        {
            var page = new Page(uri);
            Scrape(page);
        }

        public static void Scrape(Page page)
        {
            AddToBlocker(page);
            try
            {
                new PageScraper(page).Scrape();
            }
            catch (Exception exception)
            {
                Logs.Log.WriteLogErrors(page.Uri, exception);
            }            
            AddIsListingCounter(page);
        }

        private static void AddToPendingPages(Page page)
        {
            lock (SyncPendingPages)
            {
                PendingPages.Add(page);
            }
        }

        private static bool IsBlocked(Page page)
        {
            return IpHostBlocker.IsBlocked(page.Website);
        }

        private static void AddToBlocker(Page page)
        {
            lock (SyncTempBlocker)
            {
                IpHostBlocker.Add(page.Website);
            }
        }

        private static void AddIsListingCounter(Page page)
        {
            if (page.IsListing != null)
            {
                if ((bool)page.IsListing)
                {
                    lock (SyncListingsCounter)
                    {
                        ListingsCounter++;
                    }
                }
            }
        }
    }
}
