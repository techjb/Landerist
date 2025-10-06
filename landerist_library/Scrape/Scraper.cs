using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Downloaders.Multiple;
using landerist_library.Downloaders.Puppeteer;
using landerist_library.Logs;
using landerist_library.Statistics;
using landerist_library.Websites;
using landerist_orels.ES;
using System.Collections.Concurrent;


namespace landerist_library.Scrape
{
    public class Scraper
    {
        private static int TotalCounter = 0;

        private static int Scraped = 0;

        private static int Success = 0;

        private static int Crashed = 0;

        private static int DownloadErrors = 0;

        private static int ThreadCounter = 0;

        private BlockingCollection<Page> BlockingCollection = [];

        private readonly CancellationTokenSource CancellationTokenSource = new();

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
            WebsitesBlocker.Clean();
            MultipleDownloader.Clear();
            Pages = PageSelector.Select();
            Scrape();
        }

        public void Stop()
        {
            CancellationTokenSource.Cancel();
            MultipleDownloader.Clear();
            Websites.Pages.CleanLockedBy();
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

        //public void ScrapeResponseBodyRepeatedInListings()
        //{
        //    var Pages = Websites.Pages.GetPages(PageType.Listing);
        //    HashSet<string> hashSets = [];
        //    foreach (var page in Pages)
        //    {
        //        if (page.ResponseBodyTextHash is null)
        //        {
        //            continue;
        //        }
        //        if (!hashSets.Add(page.ResponseBodyTextHash))
        //        {
        //            page.SetResponseBodyTextHashToNull();
        //            Pages.Add(page);
        //        }
        //    }
        //    Scrape();
        //}

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

            Console.WriteLine("Scrapping " + TotalCounter + " pages ..");
            var orderablePartitioner = Partitioner.Create(BlockingCollection.GetConsumingEnumerable(), EnumerablePartitionerOptions.NoBuffering);

            Parallel.ForEach(
                orderablePartitioner,
                new ParallelOptions()
                {
                    MaxDegreeOfParallelism = Config.MAX_DEGREE_OF_PARALLELISM_SCRAPER,
                    //MaxDegreeOfParallelism = 10,
                    CancellationToken = CancellationTokenSource.Token
                },
                (page, state) =>
                {
                    StartThread();
                    ProcessThread(page);
                    WriteConsole(page);
                    EndThread(page, state);
                });

            LogResults();
            InsertStatistics();

            MultipleDownloader.Clear();
            PuppeteerDownloader.KillChrome();
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
            
            var isBlocked = WebsitesBlocker.IsBlocked(page.Website);           
            bool useProxy = isBlocked && Config.PROXY_ENABLED;
            Scrape(page, useProxy);            
            Interlocked.Increment(ref Scraped);
        }


        private static void WriteConsole(Page page)
        {
            if (Config.IsConfigurationProduction())
            {
                return;
            }
            var crashedPercentage = Math.Round((float)Crashed * 100 / Scraped, 0);
            var (downloadErrorsPercentage, downloadErrorsBasis) = GetDownloadErrorsMetrics();

            var text =
                $"Crashed: {Crashed} ({crashedPercentage}%) " +
                $"DownloadErrors: {DownloadErrors} ({downloadErrorsPercentage}% of {downloadErrorsBasis}) " +
                $"{page.PageType} " +
                $"{page.Uri}";
            Console.WriteLine(text);
        }

        private static void LogResults()
        {
            Log.WriteInfo("scraper", GetLogText());
        }

        private static string GetLogText()
        {
            var scrappedPercentage = Math.Round((float)Scraped * 100 / TotalCounter, 0);
            var successPercentage = Math.Round((float)Success * 100 / Scraped, 0);
            var crashedPercentage = Math.Round((float)Crashed * 100 / Scraped, 0);
            var (downloadErrorsPercentage, downloadErrorsBasis) = GetDownloadErrorsMetrics();

            return
                $"Scraped {Scraped}/{TotalCounter} ({scrappedPercentage}%) " +
                //$"Blocked {blocked} " +
                $"Success: {Success} ({successPercentage}%) " +
                $"Crashed: {Crashed} ({crashedPercentage}%) " +
                $"DownloadErrors: {DownloadErrors} ({downloadErrorsPercentage}% of {downloadErrorsBasis}) ";
                //$"Downloaders: {downloaders} " +
                //$"MaxDownloads: {maxDownloads} " +
                //$"MaxCrashes: {maxCrashes}"
                ;
        }

        private static (double Percentage, string Basis) GetDownloadErrorsMetrics()
        {
            if (Success > 0)
            {
                var percentage = Math.Round((double)DownloadErrors * 100 / Success, 0);
                return (percentage, nameof(Success));
            }

            if (Scraped > 0)
            {
                var percentage = Math.Round((double)DownloadErrors * 100 / Scraped, 0);
                return (percentage, nameof(Scraped));
            }

            return (0, nameof(Scraped));
        }

        private static void InsertStatistics()
        {
            StatisticsSnapshot.InsertDailyCounter(StatisticsKey.ScrapedSuccess, Success);
            StatisticsSnapshot.InsertDailyCounter(StatisticsKey.ScrapedCrashed, Crashed);
            StatisticsSnapshot.InsertDailyCounter(StatisticsKey.ScrapedHttpStatusCodeNotOK, DownloadErrors);
        }


        private void EndThread(Page page, ParallelLoopState parallelLoopState)
        {
            page.Dispose();
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

        public void Scrape(string url, bool useProxy)
        {
            var page = new Page(url);
            Scrape(page, useProxy);
        }

        public void Scrape(Page page, bool useProxy)
        {
            WebsitesBlocker.Block(page.Website);
            var pageScraper = new PageScraper(page, this, useProxy);
            if (pageScraper.Scrape())
            {
                Interlocked.Increment(ref Success);
                if (page.PageType.Equals(PageType.HttpStatusCodeNotOK))
                {
                    Interlocked.Increment(ref DownloadErrors);
                }
            }
            else
            {
                Interlocked.Increment(ref Crashed);
            }            
        }
    }
}
