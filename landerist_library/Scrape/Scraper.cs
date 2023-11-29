using landerist_library.Parse.PageType;
using landerist_library.Websites;
using System.Collections.Concurrent;

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

        private static bool Recursive = false;

        private List<Page> Pages = new();

        public Scraper()
        {

        }

        public Scraper(bool recursive)
        {
            Recursive = recursive;
        }

        public void ScrapeUnknowPageType(int? rows = null)
        {
            Pages = Websites.Pages.GetUnknowPageType(rows);
            if (!Scrape())
            {
                return;
            }            
            if (Recursive)
            {
                ScrapeUnknowPageType(rows);
            }
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
            if (Recursive)
            {
                ScrapeNonScrapped(website);
            }
        }

        public void ScrapeUnknowHttpStatusCode()
        {
            Pages = Websites.Pages.GetUnknowHttpStatusCode();
            if (!Scrape())
            {
                return;
            }
            if (Recursive)
            {
                ScrapeUnknowHttpStatusCode();
            }
        }

        // Can be infinite loops
        public void ScrapeUnknowIsListing(Uri uri)
        {
            Website website = new(uri);
            ScrapeUnknowIsListing(website);
        }

        public void ScrapeUnknowIsListing(Website website)
        {
            Pages = website.GetUnknowIsListingPages();
            if (!Scrape())
            {
                return;
            }
            if (Recursive)
            {
                ScrapeUnknowIsListing(website);
            }
        }

        public void ScrapeIsNotListing(Uri uri)
        {
            Website website = new(uri);
            ScrapeIsNotListing(website);
        }

        public void ScrapeIsNotListing(Website website)
        {
            Pages = website.GetIsNotListingPages();
            if (!Scrape())
            {
                return;
            }
            if (Recursive)
            {
                ScrapeIsNotListing(website);
            }
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
            ResponseBodyValidCounter = 0;
            OtherPageType = 0;
            var orderablePartitioner = Partitioner.Create(BlockingCollection.GetConsumingEnumerable(), EnumerablePartitionerOptions.NoBuffering);
            Parallel.ForEach(
                orderablePartitioner,
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
            var responseBodyValidPercentage = Math.Round((float)ResponseBodyValidCounter * 100 / TotalCounter, 0);
            var OtherPageTypePercentage = Math.Round((float)OtherPageType * 100 / TotalCounter, 0);
            
            Console.WriteLine(
               "Scraped: " + Scraped + "/" + TotalCounter + " (" + scrappedPercentage + "%) " +
               "Threads: " + ThreadCounter + " " +
               //"BlockingCollection: " + BlockingCollection.Count + " " +
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

        private void InitBlockingCollection()
        {
            HashSet<Page> hashSet = new(Pages, new PageComparer());
            Pages.Clear();
            BlockingCollection = new();
            foreach (var page in hashSet)
            {
                BlockingCollection.Add(page);
            }
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
