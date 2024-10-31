using landerist_library.Configuration;
using landerist_library.Downloaders.Multiple;
using landerist_library.Downloaders.Puppeteer;
using landerist_library.Logs;
using landerist_library.Websites;
using landerist_orels.ES;
using System.Collections.Concurrent;


namespace landerist_library.Scrape
{
    public class Scraper
    {
        private static readonly PageBlocker PageBlocker = new();

        private static readonly object SyncPageBlocker = new();

        private static int DownloadErrorCounter = 0;

        private static int ListingCounter = 0;

        private static int TotalCounter = 0;

        private static int Scraped = 0;

        private static int Remaining = 0;

        private static int ThreadCounter = 0;

        private static BlockingCollection<Page> BlockingCollection = [];

        private static readonly CancellationTokenSource CancellationTokenSource = new();

        public MultipleDownloader MultipleDownloader = new();


        private List<Page> Pages = [];

        public Scraper()
        {

        }

        public static void FinalizeBlockingCollection()
        {
            if (ThreadCounter.Equals(0) && BlockingCollection.Count.Equals(0))
            {
                BlockingCollection.CompleteAdding();                
            }
        }

        public void DoTest()
        {
            Log.WriteLogInfo("service", "Starting test..");
            PuppeteerDownloader.UpdateChrome();
            var page = new Page("https://buscopisos.es/inmueble/venta/piso/cordoba/cordoba/bp01-00250/");
            var pageScraper = new PageScraper(page, this);
            pageScraper.Scrape();
            Log.WriteLogInfo("service", "PageType: " + page.PageType.ToString());
            var listing = pageScraper.GetListing();
            string json = new Schema(listing).Serialize();
            Log.WriteLogInfo("service", "Listing: " + json);
            Stop();
        }

        public void Start()
        {
            PageBlocker.Clean();
            Pages = PageSelector.Select();
            Scrape();
        }

        public void Stop()
        {
            CancellationTokenSource.Cancel();
            MultipleDownloader.Clear();
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

        public void ScrapeMainPage(Website website)
        {
            var page = new Page(website);
            Scrape(page);
        }

        public void ScrapeResponseBodyRepeatedInListings()
        {
            var pages = Websites.Pages.GetPages(PageType.Listing);
            HashSet<string> hashSets = [];
            foreach (var page in pages)
            {
                if (page.ResponseBodyTextHash is null)
                {
                    continue;
                }
                if (!hashSets.Add(page.ResponseBodyTextHash))
                {
                    page.SetResponseBodyTextHashToNull();
                    Pages.Add(page);
                }
            }
            Scrape();
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

            var orderablePartitioner = Partitioner.Create(BlockingCollection.GetConsumingEnumerable(), EnumerablePartitionerOptions.NoBuffering);

            MultipleDownloader.Clear();

            Log.WriteLogInfo("scraper", $"Scraping {TotalCounter} pages");

            Parallel.ForEach(
                orderablePartitioner,
                new ParallelOptions()
                {
                    MaxDegreeOfParallelism = Config.MAX_DEGREE_OF_PARALLELISM,
                    CancellationToken = CancellationTokenSource.Token
                },
                (page, state) =>
                {
                    StartThread();
                    ProcessThread(page);
                    WriteConsole();
                    EndThread();
                });

            Log.WriteLogInfo("scraper", $"Scraped {Scraped} pages");
            return true;
        }

        private static void StartThread()
        {
            Interlocked.Increment(ref ThreadCounter);
        }

        private void ProcessThread(Page page)
        {
            if (!page.Website.IsAllowedByRobotsTxt(page.Uri))
            {
                page.Update(PageType.BlockedByRobotsTxt, true);
                return;
            }
            if (page.Website.CrawlDelayTooBig())
            {
                page.Update(PageType.CrawlDelayTooBig, true);
                return;
            }
            if (IsBlocked(page))
            {
                if (!BlockingCollection.IsAddingCompleted)
                {
                    BlockingCollection.Add(page);
                }
                return;
            }

            Scrape(page);
            page.Dispose();
            Interlocked.Increment(ref Scraped);
            Interlocked.Decrement(ref Remaining);
        }

        private static void WriteConsole()
        {
            if (Config.IsConfigurationProduction())
            {
                return;
            }
            var scrappedPercentage = Math.Round((float)Scraped * 100 / TotalCounter, 0);
            var downloadErrorPercentage = Math.Round((float)DownloadErrorCounter * 100 / TotalCounter, 0);
            var listingPercentage = Math.Round((float)ListingCounter * 100 / Scraped, 0);

            Console.WriteLine(
               "Threads: " + ThreadCounter + " " +
               "Scraped: " + Scraped + "/" + TotalCounter + " (" + scrappedPercentage + "%) " +
               "Errors: " + DownloadErrorCounter + " (" + downloadErrorPercentage + "%) " +
               "Listing: " + ListingCounter + " (" + listingPercentage + "%) "
               );
        }

        private static void EndThread()
        {
            Interlocked.Decrement(ref ThreadCounter);

            //if (ThreadCounter.Equals(0) && BlockingCollection.Count.Equals(0))
            //{
            //    BlockingCollection.CompleteAdding();
            //    Console.WriteLine("Finished");
            //}
        }

        private void InitBlockingCollection()
        {
            HashSet<Page> hashSet = new(Pages, new PageComparer());
            Pages.Clear();
            BlockingCollection = [.. hashSet];            
            hashSet.Clear();
        }

        public void Scrape(Uri uri)
        {
            var page = new Page(uri);
            Scrape(page);
        }

        public void Scrape(Page page)
        {
            AddToPageBlocker(page);
            try
            {
                new PageScraper(page, this).Scrape();
            }
            catch (Exception exception)
            {
                Log.WriteLogErrors("Scraper Scrape", page.Uri, exception);
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
                case PageType.Listing:
                    {
                        Interlocked.Increment(ref ListingCounter);
                    }
                    break;
            }
        }
    }
}
