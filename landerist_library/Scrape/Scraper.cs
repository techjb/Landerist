using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Downloaders.Multiple;
using landerist_library.Downloaders.Puppeteer;
using landerist_library.Logs;
using landerist_library.Statistics;
using landerist_library.Websites;
using landerist_orels.ES;

namespace landerist_library.Scrape
{
    public class Scraper
    {

        private int Counter = 0;

        private int TotalCounter = 0;

        private int Scraped = 0;

        private int TotalScraped = 0;

        private int Success = 0;

        private int TotalSuccess = 0;

        private int Crashed = 0;

        private int TotalCrashed = 0;

        private int DownloadErrors = 0;

        private int TotalDownloadErrors = 0;

        private int Skipped = 0;

        private int TotalSkipped = 0;

        private CancellationTokenSource _cancellation = new();

        private List<Page> _pageQueue = [];

        public Scraper()
        {

        }

        public void TestSinglePage()
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
            ResetCancellationTokenSource();
            WebsitesBlocker.Clean();
            DownloadersPool.Clear();
            _pageQueue = PageSelector.Select();
            Scrape();
        }

        public void Stop()
        {
            if (!_cancellation.IsCancellationRequested)
            {
                _cancellation.Cancel();
            }

            DownloadersPool.Clear();
            Websites.Pages.CleanLockedBy();
            CromeKiller.KillChrome();
        }

        public bool Scrape(Website website)
        {
            _pageQueue = website.GetPages();
            return Scrape();
        }

        private bool Scrape()
        {
            ResetCancellationTokenSource();
            if (_pageQueue.Count.Equals(0))
            {
                return false;
            }

            var pageCount = _pageQueue.Count;

            Counter = pageCount;
            Scraped = 0;
            Success = 0;
            Crashed = 0;
            DownloadErrors = 0;
            Skipped = 0;

            Console.WriteLine("Scrapping " + Counter + " pages ..");

            try
            {
                Parallel.ForEach(_pageQueue,
                    new ParallelOptions()
                    {
                        MaxDegreeOfParallelism = Config.MAX_DEGREE_OF_PARALLELISM_SCRAPER,
                        CancellationToken = _cancellation.Token
                    },
                    (page, state) =>
                    {
                        ProcessThread(page);
                        WriteConsole(page);
                        page.Dispose();
                    });
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                _pageQueue.Clear();
            }

            AccumulateTotals();
            Log.WriteInfo("scraper", GetLogText());
            InsertStatistics();

            DownloadersPool.Clear();
            CromeKiller.KillChrome();
            return true;
        }

        private void ProcessThread(Page page)
        {
            if (!page.Website.IsAllowedByRobotsTxt(page.Uri))
            {
                page.Update(PageType.BlockedByRobotsTxt, true);
                Interlocked.Increment(ref Skipped);
                return;
            }

            if (page.Website.CrawlDelayTooBig())
            {
                page.Update(PageType.CrawlDelayTooBig, true);
                Interlocked.Increment(ref Skipped);
                return;
            }

            var isBlocked = WebsitesBlocker.IsBlocked(page.Website);
            if (isBlocked && !Config.PROXY_ENABLED)
            {
                Interlocked.Increment(ref Skipped);
                return;
            }

            var useProxy = isBlocked;
            Scrape(page, useProxy);
            Interlocked.Increment(ref Scraped);
        }

        private void WriteConsole(Page page)
        {
            if (Config.IsConfigurationProduction())
            {
                return;
            }

            var crashedPercentage = GetPercentage(Crashed, Scraped);
            var downloadErrorsPercentage = GetPercentage(DownloadErrors, Scraped);
            

            var text =
                $"Crashed: {Crashed} ({crashedPercentage}%) " +
                $"DownloadErrors: {DownloadErrors} ({downloadErrorsPercentage}%) " +
                $"{page.PageType} " +
                $"{page.Uri}";
            Console.WriteLine(text);
        }


        private void AccumulateTotals()
        {
            TotalCounter += Counter;
            TotalScraped += Scraped;
            TotalSkipped += Skipped;
            TotalSuccess += Success;
            TotalCrashed += Crashed;
            TotalDownloadErrors += DownloadErrors;
        }

        private string GetLogText()
        {
            var scrappedPercentage = GetPercentage(TotalScraped, TotalCounter);
            var skippedPercentage = GetPercentage(TotalSkipped, TotalCounter);
            var successPercentage = GetPercentage(TotalSuccess, TotalScraped);
            var crashedPercentage = GetPercentage(TotalCrashed, TotalScraped);
            var downloadErrorsPercentage = GetPercentage(TotalDownloadErrors, TotalSuccess);

            return
                $"{TotalCounter} => " +
                $"[Scraped {TotalScraped} ({scrappedPercentage}%) => " +
                $"[Ok {TotalSuccess} ({successPercentage}%) => " +
                $"[DlErr {TotalDownloadErrors}  ({downloadErrorsPercentage}%)] | " +
                $"Crash {TotalCrashed} ({crashedPercentage}%)] | " +
                $"Skip {TotalSkipped} ({skippedPercentage}%)]";           
        }

        private static double GetPercentage(int value, int total)
        {
            if (total <= 0)
            {
                return 0;
            }

            return Math.Round((double)value * 100 / total, 0);
        }

        private void InsertStatistics()
        {
            StatisticsSnapshot.InsertDailyCounter(StatisticsKey.ScrapedSuccess, Success);
            StatisticsSnapshot.InsertDailyCounter(StatisticsKey.ScrapedCrashed, Crashed);
            StatisticsSnapshot.InsertDailyCounter(StatisticsKey.ScrapedHttpStatusCodeNotOK, DownloadErrors);
        }      

        private void ResetCancellationTokenSource()
        {
            if (!_cancellation.IsCancellationRequested)
            {
                return;
            }

            _cancellation.Dispose();
            _cancellation = new CancellationTokenSource();
        }

        public void Scrape(string url, bool useProxy)
        {
            var page = new Page(url);
            Scrape(page, useProxy);
        }

        public void Scrape(Page page, bool useProxy)
        {
            WebsitesBlocker.Block(page.Website);
            var pageScraper = new PageScraper(page, useProxy);
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
