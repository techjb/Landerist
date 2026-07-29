using landerist_library.Application.Listings;
using landerist_library.Application.Logging;
using landerist_library.Application.Persistence;
using landerist_library.Pages;
using landerist_library.Websites;
using System.Collections.Concurrent;

namespace landerist_library.Application.Scraping
{
    public class Scraper
    {
        private enum ScrapeAttemptResult
        {
            Blocked,
            Crashed,
            Success
        }

        private readonly IPagePersistenceService _pagePersistence;
        private readonly IApplicationLogger _logger;
        private readonly IListingLifecycleService _listingLifecycle;
        private readonly PageScrapePipelineServices _pageScraping;
        private readonly IPageBatchSelector _pageBatchSelector;
        private readonly ScrapeBatchServices _batchServices;
        private readonly ScrapeBatchState _state = new();
        private readonly ScraperLog _scraperLog;
        private CancellationTokenSource _cancellation = new();
        private List<Page> _pageQueue = [];

        public Scraper(
            IPagePersistenceService pagePersistence,
            IApplicationLogger logger,
            IListingLifecycleService listingLifecycle,
            PageScrapePipelineServices pageScraping,
            IPageBatchSelector pageBatchSelector,
            ScrapeBatchServices batchServices,
            IScrapeProgressReporter progress)
        {
            ArgumentNullException.ThrowIfNull(pagePersistence);
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(listingLifecycle);
            ArgumentNullException.ThrowIfNull(pageScraping);
            ArgumentNullException.ThrowIfNull(pageBatchSelector);
            ArgumentNullException.ThrowIfNull(batchServices);
            ArgumentNullException.ThrowIfNull(progress);

            _pagePersistence = pagePersistence;
            _logger = logger;
            _listingLifecycle = listingLifecycle;
            _pageScraping = pageScraping;
            _pageBatchSelector = pageBatchSelector;
            _batchServices = batchServices;
            _scraperLog = new ScraperLog(
                logger,
                progress,
                writePageProgress: !batchServices.Options.IsProduction);
        }

        public void TestSinglePage()
        {
            _scraperLog.WriteTestStart();
            _batchServices.Browser.UpdateChrome();
            Page page = _batchServices.Pages.LoadOrCreate(
                new Uri("https://buscopisos.es/inmueble/venta/piso/cordoba/cordoba/bp01-00250/"));
            var pageScraper = new PageScraper(
                page,
                _pagePersistence,
                _logger,
                _listingLifecycle,
                _pageScraping);
            pageScraper.Scrape();
            _scraperLog.WriteTestPageType(page);
            var listing = _batchServices.Pages.GetListing(page, true, true);
            _scraperLog.WriteTestListing(listing);
            Stop();
        }

        public void Start() => RunBatch();

        public bool RunBatch()
        {
            ResetCancellationTokenSource();
            _batchServices.WebsiteThrottle.Clean();
            _batchServices.Browser.ClearDownloaders();
            _pageQueue = [.. _pageBatchSelector.Select()];
            return ScrapeBatch();
        }

        public void Stop()
        {
            if (!_cancellation.IsCancellationRequested)
            {
                _cancellation.Cancel();
            }

            _batchServices.Browser.ClearDownloaders();
            _batchServices.PageLocks.CleanPageLocks();
            _batchServices.Browser.KillChrome();
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            if (!_cancellation.IsCancellationRequested)
            {
                await _cancellation.CancelAsync().ConfigureAwait(false);
            }

            _batchServices.Browser.ClearDownloaders();
            try
            {
                await _batchServices.PageLocks
                    .CleanPageLocksAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
            finally
            {
                _batchServices.Browser.KillChrome();
            }
        }
        public bool Scrape(Website website)
        {
            _pageQueue = [.. _batchServices.Pages.GetPages(website)];
            return ScrapeBatch();
        }

        private bool ScrapeBatch()
        {
            ResetCancellationTokenSource();
            if (_pageQueue.Count == 0)
            {
                return false;
            }

            _state.Reset(_pageQueue.Count);
            _scraperLog.WriteStart(_pageQueue.Count);
            _pageQueue = PageBatchOrderer.SpreadByHost(_pageQueue);

            try
            {
                var partitioner = Partitioner.Create(
                    _pageQueue,
                    EnumerablePartitionerOptions.NoBuffering);
                Parallel.ForEach(
                    partitioner,
                    new ParallelOptions
                    {
                        MaxDegreeOfParallelism = _batchServices.Parallelism.Calculate(_pageQueue),
                        CancellationToken = _cancellation.Token
                    },
                    page =>
                    {
                        ProcessThread(page);
                        _scraperLog.WritePage(_state.GetCurrent(), page);
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

            ScrapeBatchCounters current = _state.GetCurrent();
            ScrapeBatchCounters totals = _state.AccumulateTotals();
            _scraperLog.WriteTotals(totals);
            _batchServices.Metrics.Record(current);
            _batchServices.Browser.ClearDownloaders();
            _batchServices.Browser.KillChrome();
            return true;
        }

        private void ProcessThread(Page page)
        {
            if (!_batchServices.Robots.IsAllowed(page.Website, page.Uri))
            {
                page.SetPageType(PageType.BlockedByRobotsTxt);
                _pageScraping.Scheduling.SetNextScrapeFromNow(page);
                _pagePersistence.Update(page);
                _state.IncrementSkippedByRobotsTxt();
                return;
            }

            if (_batchServices.Robots.IsCrawlDelayTooBig(page.Website))
            {
                page.SetPageType(PageType.CrawlDelayTooBig);
                _pageScraping.Scheduling.SetNextScrapeFromNow(page);
                _pagePersistence.Update(page);
                _state.IncrementSkippedByCrawlDelay();
                return;
            }

            if (TryApplyPreClassificationBeforeDownload(page))
            {
                return;
            }

            if (_batchServices.WebsiteThrottle.IsBlocked(page.Website))
            {
                _state.IncrementSkippedByBlockedWebsite();
                return;
            }

            Scrape(page, page.Website.UseProxy);
        }

        public bool TryApplyPreClassificationBeforeDownload(Page page)
        {
            var success = new PageScraper(
                page,
                _pagePersistence,
                _logger,
                _listingLifecycle,
                _pageScraping).TryApplyPreClassificationBeforeDownload();
            if (!success)
            {
                return false;
            }

            _state.IncrementProcessed();
            _state.IncrementScrapedSuccess();
            return true;
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
            Page page = _batchServices.Pages.LoadOrCreate(new Uri(url));
            Scrape(page, useProxy);
        }

        public void Scrape(Page page, bool useProxy)
        {
            var result = ScrapeAttempt(page, useProxy);

            if (result == ScrapeAttemptResult.Blocked)
            {
                _state.IncrementSkippedByBlockedWebsite();
                return;
            }

            _state.IncrementProcessed();

            if (result == ScrapeAttemptResult.Success)
            {
                if (page.IsHttpStatusCodeForbidden())
                {
                    _batchServices.WebsiteThrottle.ReportForbidden(page.Website);
                }
                else if (!page.IsHttpStatusCodeNotOK() && !page.IsResponseBodyNullOrEmpty())
                {
                    _batchServices.WebsiteThrottle.ReportSuccess(page.Website);
                }

                if (page.IsHttpStatusCodeNotOK())
                {
                    _state.IncrementDownloadErrors();
                    return;
                }

                _state.IncrementScrapedSuccess();
                return;
            }

            _state.IncrementCrashed();
        }

        private ScrapeAttemptResult ScrapeAttempt(Page page, bool useProxy)
        {
            var acquired = _batchServices.WebsiteThrottle.TryAcquire(page.Website);
            if (!acquired && _batchServices.Options.IsProduction)
            {
                return ScrapeAttemptResult.Blocked;
            }

            var pageScraper = new PageScraper(
                page,
                useProxy,
                _pagePersistence,
                _logger,
                _listingLifecycle,
                _pageScraping);
            return pageScraper.Scrape()
                ? ScrapeAttemptResult.Success
                : ScrapeAttemptResult.Crashed;
        }
    }
}
