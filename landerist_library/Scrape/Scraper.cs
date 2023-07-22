using landerist_library.Parse.PageType;
using landerist_library.Websites;
using System.Collections.Concurrent;
using ThirdParty.BouncyCastle.Utilities.IO.Pem;

namespace landerist_library.Scrape
{
    public class Scraper
    {
        private static readonly PageBlocker PageBlocker = new();

        private static readonly object SyncPageBlocker = new();

        private static int DownloadErrorCounter = 0;

        private static int ResponseBodyValidCounter = 0;

        private static int OtherPageType = 0;

        private static int TotalCounter = 0;

        private static int Scraped = 0;

        private static int Remaining = 0;

        private static int ThreadCounter = 0;

        private static BlockingCollection<Page> BlockingCollection = new();

        public static void ScrapeNonScrapped(int? rows = null, bool recursive = false)
        {
            List<Page> pages = Pages.GetNonScraped(rows);
            if (pages.Count.Equals(0))
            {
                return;
            }
            if (!Scrape(pages))
            {
                return;
            }
            if (recursive)
            {
                ScrapeNonScrapped(rows, recursive);
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
            if (!Scrape(pages))
            {
                return;
            }
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
            if (!Scrape(pages))
            {
                return;
            }
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
            if (!Scrape(pages))
            {
                return;
            }
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
            if (!Scrape(pages))
            {
                return;
            }
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

        public static bool Scrape(Website website)
        {
            var pages = website.GetPages();
            return Scrape(pages);
        }

        private static bool Scrape(List<Page> pages)
        {
            HashSet<Page> hashSet = new(pages, new PageComparer());
            return Scrape(hashSet);
        }

        private static bool Scrape(HashSet<Page> pages)
        {
            //pages.RemoveWhere(p => !p.CanScrape());
            InitBlockingCollection(pages);
            if(pages.Count == 0)
            {
                return false;
            }
            TotalCounter = pages.Count;
            Scraped = 0;
            Remaining = TotalCounter;
            ThreadCounter = 0;
            Parallel.ForEach(
                Partitioner.Create(BlockingCollection.GetConsumingEnumerable(), EnumerablePartitionerOptions.NoBuffering),
                //new ParallelOptions() { MaxDegreeOfParallelism = 1},                
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
                page.Update(PageType.RobotsTxtCrawlDelayTooBig);
                return false;
            }
            return true;
        }

        private static void WriteConsole()
        {
            var scrappedPercentage = Math.Round((float)Scraped * 100 / TotalCounter, 0);
            var downloadErrorPercentage = Math.Round((float)DownloadErrorCounter * 100 / TotalCounter, 0);
            var responseBodyValidPercentage = Math.Round((float)ResponseBodyValidCounter * 100 / TotalCounter, 0);
            var OtherPageTypePercentage = Math.Round((float)OtherPageType * 100 / TotalCounter, 0);

            Console.WriteLine(
               "Scraped: " + Scraped + "/" + TotalCounter + " (" + scrappedPercentage + "%) " +
               "Threads: " + ThreadCounter + " " +
               "BlockingCollection: " + BlockingCollection.Count + " " +
               "Errors: " + DownloadErrorCounter + " (" + downloadErrorPercentage + "%) " +
               "Valids: " + ResponseBodyValidCounter + " (" + responseBodyValidPercentage + "%) " +
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
            AddToPageBlocker(page);
            try
            {
                new PageScraper(page).Scrape();
            }
            catch (Exception exception)
            {
                Logs.Log.WriteLogErrors(page.Uri, exception);
            }
            IncrementListingsCounter(page);
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

        private static void IncrementListingsCounter(Page page)
        {
            switch (page.PageType)
            {
                case PageType.DownloadError:
                    {
                        Interlocked.Increment(ref DownloadErrorCounter);
                    }
                    break;
                case PageType.ResponseBodyValid:
                    {
                        Interlocked.Increment(ref ResponseBodyValidCounter);
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
