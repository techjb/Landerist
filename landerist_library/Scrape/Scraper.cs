﻿using landerist_library.Websites;
using System.Collections.Concurrent;

namespace landerist_library.Scrape
{
    public class Scraper
    {
        private static readonly IpHostBlocker IpHostBlocker = new();

        private static readonly object SyncIpHostBlocker = new();

        private static int ListingsCounter = 0;

        private static int TotalCounter = 0;

        private static int Scraped = 0;

        private static int Remaining = 0;

        private static int ThreadCounter = 0;

        private static BlockingCollection<Page> BlockingCollection = new();

        public static void ScrapeNonScrapped(bool recursive = false, int? rows = null)
        {
            List<Page> pages = Pages.GetNonScraped(rows);
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


        private static void Scrape(HashSet<Page> pages)
        {
            pages.RemoveWhere(p => !p.CanScrape());
            InitBlockingCollection(pages);
            TotalCounter = BlockingCollection.Count;            
            Scraped = 0;
            Remaining = TotalCounter;
            ThreadCounter = 0;
            Parallel.ForEach(
                Partitioner.Create(BlockingCollection.GetConsumingEnumerable(), EnumerablePartitionerOptions.NoBuffering),
                //new ParallelOptions() { MaxDegreeOfParallelism = 1},
                //new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount },
                (page, state) =>
                {
                    StartThread();
                    ProcessThread(page);
                    EndThread();
                });
        }

        private static void ProcessThread(Page page)
        {
            if (IsBlocked(page) && !BlockingCollection.IsCompleted)
            {
                BlockingCollection.Append(page);
            }
            else
            {
                Scrape(page);
                Interlocked.Increment(ref Scraped);
                Interlocked.Decrement(ref Remaining);
            }

            Console.WriteLine(
                "Remaining: " + TotalCounter + " - " + Scraped + " = " + Remaining + " " +
                "Threads: " + ThreadCounter + " " +
                "BlockingCollection: " + BlockingCollection.Count + " " +
                "Listings: " + ListingsCounter + " ");
                //"Page host: " + page.Host);
        }

        private static void StartThread()
        {
            Interlocked.Increment(ref ThreadCounter);
        }

        private static void EndThread()
        {
            Interlocked.Decrement(ref ThreadCounter);

            if (ThreadCounter == 0 && BlockingCollection.Count == 0)
            {
                BlockingCollection.CompleteAdding();
                Console.WriteLine("Finished");
            }
        }

        private static void InitBlockingCollection(HashSet<Page> pages)
        {
            BlockingCollection = new();
            foreach (var page in pages)
            {
                BlockingCollection.Add(page);
            }
        }

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

        private static bool IsBlocked(Page page)
        {
            return IpHostBlocker.IsBlocked(page.Website);
        }

        private static void AddToBlocker(Page page)
        {
            lock (SyncIpHostBlocker)
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
                    Interlocked.Increment(ref ListingsCounter);
                }
            }
        }
    }
}
