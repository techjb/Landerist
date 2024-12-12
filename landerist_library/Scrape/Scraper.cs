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

        private static int TotalCounter = 0;

        private static int Scraped = 0;

        private static int Success = 0;

        private static int Crashed = 0;

        private static int DownloadErrors = 0;

        private static int ThreadCounter = 0;

        private BlockingCollection<Page> BlockingCollection = [];

        private readonly CancellationTokenSource CancellationTokenSource = new();

        //public static readonly MultipleDownloader MultipleDownloader = new();


        private List<Page> Pages = [];

        public Scraper()
        {

        }

        public void FinalizeBlockingCollection()
        {
            if (BlockingCollection.Count < Config.MAX_DEGREE_OF_PARALLELISM_SCRAPER)
            {
                BlockingCollection.CompleteAdding();
            }
        }

        public void DoTest()
        {
            Log.WriteInfo("service", "Starting test..");
            PuppeteerDownloader.UpdateChrome();
            var page = new Page("https://buscopisos.es/inmueble/venta/piso/cordoba/cordoba/bp01-00250/");
            var pageScraper = new PageScraper(page);
            pageScraper.Scrape();
            Log.WriteInfo("service", "PageType: " + page.PageType.ToString());
            var listing = pageScraper.GetListing();
            string json = new Schema(listing).Serialize();
            Log.WriteInfo("service", "Listing: " + json);
            Stop();
        }

        public void Start()
        {
            PageBlocker.Clean();
            MultipleDownloader.Clear();
            Pages = PageSelector.Select();
            Scrape();
        }

        public void Stop()
        {
            CancellationTokenSource.Cancel();
            MultipleDownloader.Clear();
            PuppeteerDownloader.KillChrome();
        }

        public void ScrapeUnknowPageType()
        {
            Pages = Websites.Pages.GetUnknownPageType();
            if (!Scrape())
            {
                return;
            }
            ScrapeUnknowPageType();
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
            Success = 0;
            Crashed = 0;
            DownloadErrors = 0;
            ThreadCounter = 0;

            Log.Console("Scrapping " + TotalCounter + " pages ..");
            var orderablePartitioner = Partitioner.Create(BlockingCollection.GetConsumingEnumerable(), EnumerablePartitionerOptions.NoBuffering);

            Parallel.ForEach(
                orderablePartitioner,
                new ParallelOptions()
                {
                    MaxDegreeOfParallelism = Config.MAX_DEGREE_OF_PARALLELISM_SCRAPER,
                    CancellationToken = CancellationTokenSource.Token
                },
                (page, state) =>
                {
                    StartThread();
                    ProcessThread(page);
                    WriteConsole();
                    EndThread(state);
                });

            LogResults();
            //MultipleDownloader.Print();
            MultipleDownloader.Clear();
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
            if (PageBlocker.IsBlocked(page))
            {
                return;
            }
            Scrape(page);
            page.Dispose();
            Interlocked.Increment(ref Scraped);
        }


        private static void WriteConsole()
        {
            if (Config.IsConfigurationProduction())
            {
                return;
            }

            var scrappedPercentage = Math.Round((float)Scraped * 100 / TotalCounter, 0);
            Console.WriteLine(
               "Threads: " + ThreadCounter + " " +
               "Scraped: " + Scraped + "/" + TotalCounter + " (" + scrappedPercentage + "%) ");
        }

        private static void LogResults()
        {
            //var downloaders = MultipleDownloader.GetDownloadersCounter();
            //var maxDownloads = MultipleDownloader.GetMaxDownloads();
            //var maxCrashes = MultipleDownloader.GetMaxCrashCounter();

            //int blocked = PageBlocker.CountBlockedPages();
            var scrappedPercentage = Math.Round((float)Scraped * 100 / TotalCounter, 0);
            var successPercentage = Math.Round((float)Success * 100 / Scraped, 0);
            var crashedPercentage = Math.Round((float)Crashed * 100 / Scraped, 0);
            var downloadErrorsPercentage = Math.Round((float)DownloadErrors * 100 / Success, 0);

            Log.WriteInfo("scraper",
                $"Scraped {Scraped}/{TotalCounter} ({scrappedPercentage}%) " +
                //$"Blocked {blocked} " +
                $"Success: {Success} ({successPercentage}%) " +
                $"Crashed: {Crashed} ({crashedPercentage}%) " +
                $"DownloadErrors: {DownloadErrors} ({downloadErrorsPercentage}%) "
                //$"Downloaders: {downloaders} " +
                //$"MaxDownloads: {maxDownloads} " +
                //$"MaxCrashes: {maxCrashes}"
                );
        }


        private void EndThread(ParallelLoopState parallelLoopState)
        {
            AddUnblockedPages();
            Interlocked.Decrement(ref ThreadCounter);
            if (BlockingCollection.IsAddingCompleted)
            {
                parallelLoopState.Stop();
            }
        }

        private void InitBlockingCollection()
        {
            HashSet<Page> hashSet = new(Pages, new PageComparer());
            BlockingCollection = [.. hashSet];
            Pages.Clear();
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
            var pageScraper = new PageScraper(page, this);
            if (pageScraper.Scrape())
            {
                Interlocked.Increment(ref Success);
                if (page.PageType == PageType.DownloadError)
                {
                    Interlocked.Increment(ref DownloadErrors);
                }
            }
            else
            {
                Interlocked.Increment(ref Crashed);
            }
        }

        private static void AddToPageBlocker(Page page)
        {
            lock (SyncPageBlocker)
            {
                PageBlocker.Add(page.Website);
            }
        }

        private void AddUnblockedPages()
        {
            if (BlockingCollection.IsAddingCompleted)
            {
                return;
            }

            lock (SyncPageBlocker)
            {
                var pages = PageBlocker.GetUnblockedPages();
                if (pages.Count.Equals(0))
                {
                    return;
                }
                pages.ForEach(BlockingCollection.Add);
            }
        }
    }
}
