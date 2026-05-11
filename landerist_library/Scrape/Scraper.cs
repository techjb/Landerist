using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Downloaders.Multiple;
using landerist_library.Downloaders.Puppeteer;
using landerist_library.Pages;
using landerist_library.Statistics;
using landerist_library.Websites;
using System.Collections.Concurrent;

namespace landerist_library.Scrape
{
    public class Scraper
    {
        private enum ScrapeAttemptResult
        {
            Blocked,
            Crashed,
            Success
        }

        private const int EstimatedMinimumScrapeSeconds = 2;

        private const int MinimumHostThrottleSeconds = 3;

        private int Counter = 0;

        private int TotalCounter = 0;

        private int Processed = 0;

        private int TotalProcessed = 0;

        private int ScrapedSuccess = 0;

        private int TotalScrapedSuccess = 0;

        private int Crashed = 0;

        private int TotalCrashed = 0;

        private int DownloadErrors = 0;

        private int TotalDownloadErrors = 0;

        private int SkippedByRobotsTxt = 0;

        private int TotalSkippedByRobotsTxt = 0;

        private int SkippedByCrawlDelay = 0;

        private int TotalSkippedByCrawlDelay = 0;

        private int SkippedByBlockedWebsite = 0;

        private int TotalSkippedByBlockedWebsite = 0;

        private CancellationTokenSource _cancellation = new();

        private List<Page> _pageQueue = [];

        public Scraper()
        {

        }

        public void TestSinglePage()
        {
            ScraperLog.WriteTestStart();
            PuppeteerDownloader.UpdateChrome();
            var page = new Page("https://buscopisos.es/inmueble/venta/piso/cordoba/cordoba/bp01-00250/");
            var pageScraper = new PageScraper(page);
            pageScraper.Scrape();
            ScraperLog.WriteTestPageType(page);
            var listing =  page.GetListing(true, true);
            ScraperLog.WriteTestListing(listing);
            Stop();
        }

        public void Start()
        {
            ResetCancellationTokenSource();
            WebsitesThrottle.Clean();
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
            Pages.Pages.CleanLockedBy();
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
            Processed = 0;
            ScrapedSuccess = 0;
            Crashed = 0;
            DownloadErrors = 0;
            SkippedByRobotsTxt = 0;
            SkippedByCrawlDelay = 0;
            SkippedByBlockedWebsite = 0;

            ScraperLog.WriteStart(Counter);
            _pageQueue = SpreadPagesByHost(_pageQueue);

            try
            {
                var partitioner = Partitioner.Create(_pageQueue, EnumerablePartitionerOptions.NoBuffering);
                Parallel.ForEach(partitioner,
                    new ParallelOptions()
                    {
                        MaxDegreeOfParallelism = GetMaxDegreeOfParallelism(_pageQueue),
                        CancellationToken = _cancellation.Token
                    },
                    (page, state) =>
                    {
                        ProcessThread(page);
                        ScraperLog.WritePage(GetCurrentCounters(), page);
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
            ScraperLog.WriteTotals(GetTotalCounters());
            InsertStatistics();

            DownloadersPool.Clear();
            CromeKiller.KillChrome();
            return true;
        }

        private static List<Page> SpreadPagesByHost(List<Page> pages)
        {
            Dictionary<string, Queue<Page>> pagesByHost = [];
            List<string> hosts = [];

            foreach (var page in pages)
            {
                var host = page.Host;
                if (!pagesByHost.TryGetValue(host, out var hostPages))
                {
                    hostPages = [];
                    pagesByHost[host] = hostPages;
                    hosts.Add(host);
                }

                hostPages.Enqueue(page);
            }

            List<Page> spreadPages = new(pages.Count);
            while (spreadPages.Count < pages.Count)
            {
                foreach (var host in hosts)
                {
                    var hostPages = pagesByHost[host];
                    if (hostPages.Count > 0)
                    {
                        spreadPages.Add(hostPages.Dequeue());
                    }
                }
            }

            return spreadPages;
        }

        private static int GetMaxDegreeOfParallelism(List<Page> pages)
        {
            if (Config.IsConfigurationLocal())
            {
                return 1;
            }

            var pageCount = pages.Count;
            var configuredMaxDegreeOfParallelism = Config.MAX_DEGREE_OF_PARALLELISM_SCRAPER;
            var maxDegreeOfParallelism = configuredMaxDegreeOfParallelism < 1
                ? pageCount
                : Math.Min(configuredMaxDegreeOfParallelism, pageCount);
            if (maxDegreeOfParallelism <= 1)
            {
                return 1;
            }

            HashSet<string> hosts = [];
            foreach (var page in pages)
            {
                hosts.Add(page.Host);
            }

            var distinctHostCount = hosts.Count;
            if (distinctHostCount == pageCount)
            {
                return maxDegreeOfParallelism;
            }

            var wavesBeforeSameHost = (int)Math.Ceiling((double)MinimumHostThrottleSeconds / EstimatedMinimumScrapeSeconds);
            var hostLimitedMaxDegreeOfParallelism = Math.Max(1, distinctHostCount / wavesBeforeSameHost);

            return Math.Min(maxDegreeOfParallelism, hostLimitedMaxDegreeOfParallelism);
        }

        private void ProcessThread(Page page)
        {
            if (!page.Website.IsAllowedByRobotsTxt(page.Uri))
            {
                page.SetPageTypeAndNextScrape(PageType.BlockedByRobotsTxt);
                Interlocked.Increment(ref SkippedByRobotsTxt);
                return;
            }

            if (page.Website.CrawlDelayTooBig())
            {
                page.SetPageTypeAndNextScrape(PageType.CrawlDelayTooBig);
                Interlocked.Increment(ref SkippedByCrawlDelay);
                return;
            }

            if (TryApplyPreClassificationBeforeDownload(page))
            {
                return;
            }

            var isBlocked = WebsitesThrottle.IsBlocked(page.Website);
            

            if (isBlocked)
            {
                Interlocked.Increment(ref SkippedByBlockedWebsite);
                return;
            }

            bool useProxy = page.Website.UseProxy;
            Scrape(page, useProxy);
        }

        private bool TryApplyPreClassificationBeforeDownload(Page page)
        {
            var success = new PageScraper(page).TryApplyPreClassificationBeforeDownload();
            if (success)
            {
                Interlocked.Increment(ref Processed);
                Interlocked.Increment(ref ScrapedSuccess);
                return true;
            }
            return false;
        }

        private void AccumulateTotals()
        {
            TotalCounter += Counter;
            TotalProcessed += Processed;
            TotalSkippedByRobotsTxt += SkippedByRobotsTxt;
            TotalSkippedByCrawlDelay += SkippedByCrawlDelay;
            TotalSkippedByBlockedWebsite += SkippedByBlockedWebsite;
            TotalScrapedSuccess += ScrapedSuccess;
            TotalCrashed += Crashed;
            TotalDownloadErrors += DownloadErrors;
        }

        private ScraperLogCounters GetCurrentCounters()
        {
            return new ScraperLogCounters
            {
                Total = Counter,
                Processed = Processed,
                ScrapedSuccess = ScrapedSuccess,
                Crashed = Crashed,
                DownloadErrors = DownloadErrors,
                SkippedByRobotsTxt = SkippedByRobotsTxt,
                SkippedByCrawlDelay = SkippedByCrawlDelay,
                SkippedByBlockedWebsite = SkippedByBlockedWebsite
            };
        }

        private ScraperLogCounters GetTotalCounters()
        {
            return new ScraperLogCounters
            {
                Total = TotalCounter,
                Processed = TotalProcessed,
                ScrapedSuccess = TotalScrapedSuccess,
                Crashed = TotalCrashed,
                DownloadErrors = TotalDownloadErrors,
                SkippedByRobotsTxt = TotalSkippedByRobotsTxt,
                SkippedByCrawlDelay = TotalSkippedByCrawlDelay,
                SkippedByBlockedWebsite = TotalSkippedByBlockedWebsite
            };
        }

        private void InsertStatistics()
        {
            GlobalStatistics.InsertDailyCounter(StatisticsKey.Processed, Processed);
            GlobalStatistics.InsertDailyCounter(StatisticsKey.ScrapedSuccess, ScrapedSuccess);
            GlobalStatistics.InsertDailyCounter(StatisticsKey.ScrapedCrashed, Crashed);
            GlobalStatistics.InsertDailyCounter(StatisticsKey.ScrapedHttpStatusCodeNotOK, DownloadErrors);
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
            var result = ScrapeAttempt(page, useProxy);

            if (result.Equals(ScrapeAttemptResult.Blocked))
            {
                Interlocked.Increment(ref SkippedByBlockedWebsite);
                return;
            }

            Interlocked.Increment(ref Processed);

            if (result.Equals(ScrapeAttemptResult.Success))
            {
                if (page.IsHttpStatusCodeForbidden())
                {
                    WebsitesThrottle.ReportForbidden(page.Website);
                }
                else if (!page.IsHttpStatusCodeNotOK() && !page.IsResponseBodyNullOrEmpty())
                {
                    WebsitesThrottle.ReportSuccess(page.Website);
                }

                if (page.PageType.Equals(PageType.HttpStatusCodeNotOK))
                {
                    Interlocked.Increment(ref DownloadErrors);
                    return;
                }

                Interlocked.Increment(ref ScrapedSuccess);
            }
            else
            {
                Interlocked.Increment(ref Crashed);
            }
        }

        private static ScrapeAttemptResult ScrapeAttempt(Page page, bool useProxy)
        {
            var acquired = WebsitesThrottle.Block(page.Website);
            if (!acquired && Config.IsConfigurationProduction())
            {
                return ScrapeAttemptResult.Blocked;
            }

            var pageScraper = new PageScraper(page, useProxy);
            return pageScraper.Scrape()
                ? ScrapeAttemptResult.Success
                : ScrapeAttemptResult.Crashed;
        }        
    }
}
