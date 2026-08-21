using landerist_library.Application.Listings;
using landerist_library.Application.Logging;
using landerist_library.Application.Persistence;
using landerist_library.Pages;
using landerist_library.Websites;
using System.Collections.Concurrent;

namespace landerist_library.Application.Scraping;

public class Scraper
{
    private readonly IPagePersistenceService _pagePersistence;
    private readonly IApplicationLogger _logger;
    private readonly IListingLifecycleService _listingLifecycle;
    private readonly PageScrapePipelineServices _pageScraping;
    private readonly IPageBatchSelector _pageBatchSelector;
    private readonly Action? _reportProgress;
    private readonly ScrapeBatchServices _batchServices;
    private readonly ScrapeBatchState _state = new();
    private readonly ScraperLog _scraperLog;
    private readonly ScrapePageProcessor _pageProcessor;
    private CancellationTokenSource _cancellation = new();
    private List<Page> _pageQueue = [];

    public Scraper(
        IPagePersistenceService pagePersistence,
        IApplicationLogger logger,
        IListingLifecycleService listingLifecycle,
        PageScrapePipelineServices pageScraping,
        IPageBatchSelector pageBatchSelector,
        ScrapeBatchServices batchServices,
        IScrapeProgressReporter progress,
        Action? reportProgress = null)
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
        _reportProgress = reportProgress;
        _batchServices = batchServices;
        _scraperLog = new ScraperLog(
            logger,
            progress,
            writePageProgress: !batchServices.Options.IsProduction);
        _pageProcessor = new ScrapePageProcessor(
            pagePersistence,
            logger,
            listingLifecycle,
            pageScraping,
            batchServices,
            _state);
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

    public async Task<bool> RunBatchAsync(CancellationToken cancellationToken = default)
    {
        ResetCancellationTokenSource();
        using CancellationTokenSource linkedCancellation =
            CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                _cancellation.Token);
        try
        {
            await _batchServices.WebsiteThrottle
                .CleanAsync(linkedCancellation.Token).ConfigureAwait(false);
            await _batchServices.Browser
                .ClearDownloadersAsync(linkedCancellation.Token).ConfigureAwait(false);
            _pageQueue = [.. _pageBatchSelector.Select()];
            return await ScrapeBatchAsync(linkedCancellation.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (linkedCancellation.IsCancellationRequested)
        {
            return false;
        }
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

        try
        {
            await _batchServices.Browser
                .ClearDownloadersAsync(cancellationToken).ConfigureAwait(false);
            await _batchServices.PageLocks
                .CleanPageLocksAsync(cancellationToken).ConfigureAwait(false);
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

    public void Scrape(string url, bool useProxy)
    {
        Page page = _batchServices.Pages.LoadOrCreate(new Uri(url));
        _pageProcessor.Scrape(page, useProxy);
    }

    public void Scrape(Page page, bool useProxy) =>
        _pageProcessor.Scrape(page, useProxy);

    public bool TryApplyPreClassificationBeforeDownload(Page page) =>
        _pageProcessor.TryApplyPreClassificationBeforeDownload(page);

    private bool ScrapeBatch()
    {
        ResetCancellationTokenSource();
        if (!PrepareBatch())
        {
            return false;
        }

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
                    try
                    {
                        _pageProcessor.Process(page);
                        _reportProgress?.Invoke();
                        _scraperLog.WritePage(_state.GetCurrent(), page);
                    }
                    finally
                    {
                        page.Dispose();
                    }
                });
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            _pageQueue.Clear();
        }

        return CompleteScrapeBatch();
    }

    private async Task<bool> ScrapeBatchAsync(CancellationToken cancellationToken)
    {
        if (!PrepareBatch())
        {
            return false;
        }

        try
        {
            await Parallel.ForEachAsync(
                _pageQueue,
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = _batchServices.Parallelism.Calculate(_pageQueue),
                    CancellationToken = cancellationToken
                },
                async (page, token) =>
                {
                    try
                    {
                        await _pageProcessor.ProcessAsync(page, token).ConfigureAwait(false);
                        _reportProgress?.Invoke();
                        _scraperLog.WritePage(_state.GetCurrent(), page);
                    }
                    finally
                    {
                        page.Dispose();
                    }
                }).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        finally
        {
            _pageQueue.Clear();
        }

        return await CompleteScrapeBatchAsync(cancellationToken).ConfigureAwait(false);
    }

    private bool PrepareBatch()
    {
        if (_pageQueue.Count == 0)
        {
            return false;
        }

        _state.Reset(_pageQueue.Count);
        _scraperLog.WriteStart(_pageQueue.Count);
        _pageQueue = PageBatchOrderer.SpreadByHost(_pageQueue);
        return true;
    }

    private bool CompleteScrapeBatch()
    {
        RecordBatchCompletion();
        _batchServices.Browser.ClearDownloaders();
        _batchServices.Browser.KillChrome();
        return true;
    }

    private async Task<bool> CompleteScrapeBatchAsync(CancellationToken cancellationToken)
    {
        RecordBatchCompletion();
        try
        {
            await _batchServices.Browser
                .ClearDownloadersAsync(cancellationToken).ConfigureAwait(false);
            return true;
        }
        finally
        {
            _batchServices.Browser.KillChrome();
        }
    }

    private void RecordBatchCompletion()
    {
        ScrapeBatchCounters current = _state.GetCurrent();
        ScrapeBatchCounters totals = _state.AccumulateTotals();
        _scraperLog.WriteTotals(totals);
        _batchServices.Metrics.Record(current);
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
}
