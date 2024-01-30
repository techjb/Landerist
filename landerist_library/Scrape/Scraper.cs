﻿using landerist_library.Websites;
using System.Collections.Concurrent;
using landerist_library.Logs;

namespace landerist_library.Scrape
{
    public class Scraper
    {
        private static readonly PageBlocker PageBlocker = new();

        private static readonly object SyncPageBlocker = new();

        private static int DownloadErrorCounter = 0;

        private static int MayBeListingCounter = 0;

        private static int ListingCounter = 0;

        private static int OtherPageType = 0;

        private static int TotalCounter = 0;

        private static int Scraped = 0;

        private static int Remaining = 0;

        private static int ThreadCounter = 0;

        private static BlockingCollection<Page> BlockingCollection = [];

        private List<Page> Pages = [];


        public void Start()
        {
            Pages = PageSelector.Select();
            Scrape();
        }

        public void ScrapeUnknowPageType(int? rows = null)
        {
            Pages = Websites.Pages.GetUnknownPageType(rows);
            if (!Scrape())
            {
                return;
            }
            ScrapeUnknowPageType(rows);
        }

        public void ScrapeNonScrapped(Uri uri)
        {
            Website website = new(uri);
            ScrapeNonScrapped(website);
        }

        public void ScrapeNonScrapped(Website website)
        {
            Pages = website.GetNonScrapedPages();
            if (!Scrape())
            {
                return;
            }
            ScrapeNonScrapped(website);
        }

        public void ScrapeUnknowHttpStatusCode()
        {
            Pages = Websites.Pages.GetUnknowHttpStatusCode();
            if (!Scrape())
            {
                return;
            }
            ScrapeUnknowHttpStatusCode();
        }     

        public static void ScrapeMainPage(Website website)
        {
            var page = new Page(website);
            Scrape(page);
        }

        public bool Scrape(Website website)
        {
            Pages = website.GetPages();
            return Scrape();
        }

        public bool ScrapeUnknowPageType(Website website)
        {
            Pages = website.GetPagesUnknowPageType();
            return Scrape();
        }

        private bool Scrape()
        {
            InitBlockingCollection();
            if (BlockingCollection.Count.Equals(0))
            {
                return false;
            }
            TotalCounter = BlockingCollection.Count;
            Scraped = 0;
            Remaining = TotalCounter;
            ThreadCounter = 0;
            DownloadErrorCounter = 0;
            MayBeListingCounter = 0;
            OtherPageType = 0;

            Log.WriteLogInfo("scraper", "TotalCounter: " + TotalCounter);

            var orderablePartitioner = Partitioner.Create(BlockingCollection.GetConsumingEnumerable(), EnumerablePartitionerOptions.NoBuffering);
            Parallel.ForEach(
                orderablePartitioner,
                new ParallelOptions() {
                    MaxDegreeOfParallelism = Environment.ProcessorCount - 1 
                    //MaxDegreeOfParallelism = 1
                },
                (page, state) =>
                {
                    StartThread();
                    ProcessThread(page);
                    EndThread();
                });            
            return true;
        }

        private static void StartThread()
        {
            Interlocked.Increment(ref ThreadCounter);
        }

        private static void ProcessThread(Page page)
        {
            if (!IsScrapeable(page))
            {
                return;
            }

            if (IsBlocked(page) && !BlockingCollection.IsCompleted)
            {
                BlockingCollection.Add(page);
            }
            else
            {
                Scrape(page);
                page.Dispose();
                Interlocked.Increment(ref Scraped);
                Interlocked.Decrement(ref Remaining);
            }

            WriteConsole();
        }

        private static bool IsScrapeable(Page page)
        {
            if (!page.Website.IsAllowedByRobotsTxt(page.Uri))
            {
                page.Update(PageType.BlockedByRobotsTxt);
                return false;
            }
            if (page.Website.CrawlDelayTooBig())
            {
                page.Update(PageType.RobotsTxtDisallow);
                return false;
            }
            return true;
        }

        private static void WriteConsole()
        {
            var scrappedPercentage = Math.Round((float)Scraped * 100 / TotalCounter, 0);
            var downloadErrorPercentage = Math.Round((float)DownloadErrorCounter * 100 / TotalCounter, 0);
            var mayBeListingPercentage = Math.Round((float)MayBeListingCounter * 100 / Scraped, 0);
            var listingPercentage = Math.Round((float)ListingCounter * 100 / Scraped, 0);
            var OtherPageTypePercentage = Math.Round((float)OtherPageType * 100 / Scraped, 0);

            Console.WriteLine(
               "Threads: " + ThreadCounter + " " +
               "Scraped: " + Scraped + "/" + TotalCounter + " (" + scrappedPercentage + "%) " +
               "Errors: " + DownloadErrorCounter + " (" + downloadErrorPercentage + "%) " +
               "MayBeListing: " + MayBeListingCounter + " (" + mayBeListingPercentage + "%) " +
               "Listing: " + ListingCounter + " (" + listingPercentage + "%) " +
               "Others: " + OtherPageType + " (" + OtherPageTypePercentage + "%) "
               );
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

        private void InitBlockingCollection()
        {
            HashSet<Page> hashSet = new(Pages, new PageComparer());
            Pages.Clear();
            BlockingCollection = [.. hashSet];
            hashSet.Clear();
        }

        public static void Scrape(Uri uri)
        {
            var page = new Page(uri);
            Scrape(page);
        }

        public static void Scrape(Page page)
        {
            AddToPageBlocker(page);
            try
            {
                new PageScraper(page).Scrape();
            }
            catch (Exception exception)
            {
                Logs.Log.WriteLogErrors(page.Uri, exception);
            }
            IncrementCounters(page);
        }

        private static bool IsBlocked(Page page)
        {
            return PageBlocker.IsBlocked(page.Website);
        }

        private static void AddToPageBlocker(Page page)
        {
            lock (SyncPageBlocker)
            {
                PageBlocker.Add(page.Website);
            }
        }

        private static void IncrementCounters(Page page)
        {
            switch (page.PageType)
            {
                case PageType.DownloadError:
                    {
                        Interlocked.Increment(ref DownloadErrorCounter);
                    }
                    break;
                case PageType.MayBeListing:
                    {
                        Interlocked.Increment(ref MayBeListingCounter);
                    }
                    break;
                case PageType.Listing:
                    {
                        Interlocked.Increment(ref ListingCounter);
                    }
                    break;
                default:
                    {
                        Interlocked.Increment(ref OtherPageType);
                    }
                    break;
            }
        }
    }
}
