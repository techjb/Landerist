using landerist_library.Application.Listings;
using landerist_library.Application.Logging;
using landerist_library.Application.Persistence;
using landerist_library.Pages;

namespace landerist_library.Application.Scraping;

internal sealed class ScrapePageProcessor
{
    private enum ScrapeAttemptResult { Blocked, Crashed, Success }

    private readonly IPagePersistenceService _pagePersistence;
    private readonly IApplicationLogger _logger;
    private readonly IListingLifecycleService _listingLifecycle;
    private readonly PageScrapePipelineServices _pageScraping;
    private readonly ScrapeBatchServices _batchServices;
    private readonly ScrapeBatchState _state;

    public ScrapePageProcessor(
        IPagePersistenceService pagePersistence,
        IApplicationLogger logger,
        IListingLifecycleService listingLifecycle,
        PageScrapePipelineServices pageScraping,
        ScrapeBatchServices batchServices,
        ScrapeBatchState state)
    {
        _pagePersistence = pagePersistence;
        _logger = logger;
        _listingLifecycle = listingLifecycle;
        _pageScraping = pageScraping;
        _batchServices = batchServices;
        _state = state;
    }

    public void Process(Page page)
    {
        if (ApplyRobotsPolicy(page) || TryApplyPreClassificationBeforeDownload(page))
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

    public async Task ProcessAsync(Page page, CancellationToken cancellationToken)
    {
        if (await ApplyRobotsPolicyAsync(page, cancellationToken).ConfigureAwait(false) ||
            await TryApplyPreClassificationBeforeDownloadAsync(page, cancellationToken)
                .ConfigureAwait(false))
        {
            return;
        }

        if (await _batchServices.WebsiteThrottle
            .IsBlockedAsync(page.Website, cancellationToken).ConfigureAwait(false))
        {
            _state.IncrementSkippedByBlockedWebsite();
            return;
        }

        await ScrapeAsync(page, page.Website.UseProxy, cancellationToken)
            .ConfigureAwait(false);
    }

    public bool TryApplyPreClassificationBeforeDownload(Page page)
    {
        bool success = CreatePageScraper(page).TryApplyPreClassificationBeforeDownload();
        RecordPreClassification(success);
        return success;
    }

    public void Scrape(Page page, bool useProxy)
    {
        ScrapeAttemptResult result = ScrapeAttempt(page, useProxy);
        HandleScrapeResult(page, result);
    }

    private async Task<bool> TryApplyPreClassificationBeforeDownloadAsync(
        Page page,
        CancellationToken cancellationToken)
    {
        bool success = await CreatePageScraper(page)
            .TryApplyPreClassificationBeforeDownloadAsync(cancellationToken)
            .ConfigureAwait(false);
        RecordPreClassification(success);
        return success;
    }

    private async Task ScrapeAsync(
        Page page,
        bool useProxy,
        CancellationToken cancellationToken)
    {
        ScrapeAttemptResult result = await ScrapeAttemptAsync(
            page,
            useProxy,
            cancellationToken).ConfigureAwait(false);
        await HandleScrapeResultAsync(page, result, cancellationToken).ConfigureAwait(false);
    }

    private bool ApplyRobotsPolicy(Page page)
    {
        PageType? pageType = GetRobotsPageType(page);
        if (pageType is null)
        {
            return false;
        }

        ApplyRobotsPageType(page, pageType.Value);
        _pagePersistence.Update(page);
        return true;
    }

    private async Task<bool> ApplyRobotsPolicyAsync(
        Page page,
        CancellationToken cancellationToken)
    {
        PageType? pageType = GetRobotsPageType(page);
        if (pageType is null)
        {
            return false;
        }

        ApplyRobotsPageType(page, pageType.Value);
        await _pagePersistence.UpdateAsync(page, cancellationToken).ConfigureAwait(false);
        return true;
    }

    private PageType? GetRobotsPageType(Page page)
    {
        if (!_batchServices.Robots.IsAllowed(page.Website, page.Uri))
        {
            return PageType.BlockedByRobotsTxt;
        }

        return _batchServices.Robots.IsCrawlDelayTooBig(page.Website)
            ? PageType.CrawlDelayTooBig
            : null;
    }

    private void ApplyRobotsPageType(Page page, PageType pageType)
    {
        page.SetPageType(pageType);
        _pageScraping.Scheduling.SetNextScrapeFromNow(page);
        if (pageType == PageType.BlockedByRobotsTxt)
        {
            _state.IncrementSkippedByRobotsTxt();
        }
        else
        {
            _state.IncrementSkippedByCrawlDelay();
        }
    }

    private void RecordPreClassification(bool success)
    {
        if (success)
        {
            _state.IncrementProcessed();
            _state.IncrementScrapedSuccess();
        }
    }

    private ScrapeAttemptResult ScrapeAttempt(Page page, bool useProxy)
    {
        bool acquired = _batchServices.WebsiteThrottle.TryAcquire(page.Website);
        if (!acquired && _batchServices.Options.IsProduction)
        {
            return ScrapeAttemptResult.Blocked;
        }

        return CreatePageScraper(page, useProxy).Scrape()
            ? ScrapeAttemptResult.Success
            : ScrapeAttemptResult.Crashed;
    }

    private async Task<ScrapeAttemptResult> ScrapeAttemptAsync(
        Page page,
        bool useProxy,
        CancellationToken cancellationToken)
    {
        bool acquired = await _batchServices.WebsiteThrottle
            .TryAcquireAsync(page.Website, cancellationToken).ConfigureAwait(false);
        if (!acquired && _batchServices.Options.IsProduction)
        {
            return ScrapeAttemptResult.Blocked;
        }

        return await CreatePageScraper(page, useProxy)
            .ScrapeAsync(cancellationToken).ConfigureAwait(false)
            ? ScrapeAttemptResult.Success
            : ScrapeAttemptResult.Crashed;
    }

    private void HandleScrapeResult(Page page, ScrapeAttemptResult result)
    {
        if (!BeginResultHandling(result))
        {
            return;
        }

        if (result == ScrapeAttemptResult.Success)
        {
            ReportThrottleResult(page);
        }

        CompleteResultHandling(page, result);
    }

    private async Task HandleScrapeResultAsync(
        Page page,
        ScrapeAttemptResult result,
        CancellationToken cancellationToken)
    {
        if (!BeginResultHandling(result))
        {
            return;
        }

        if (result == ScrapeAttemptResult.Success)
        {
            await ReportThrottleResultAsync(page, cancellationToken).ConfigureAwait(false);
        }

        CompleteResultHandling(page, result);
    }

    private bool BeginResultHandling(ScrapeAttemptResult result)
    {
        if (result == ScrapeAttemptResult.Blocked)
        {
            _state.IncrementSkippedByBlockedWebsite();
            return false;
        }

        _state.IncrementProcessed();
        return true;
    }

    private void CompleteResultHandling(Page page, ScrapeAttemptResult result)
    {
        if (result == ScrapeAttemptResult.Crashed)
        {
            _state.IncrementCrashed();
        }
        else if (page.IsHttpStatusCodeNotOK())
        {
            _state.IncrementDownloadErrors();
        }
        else
        {
            _state.IncrementScrapedSuccess();
        }
    }

    private void ReportThrottleResult(Page page)
    {
        if (page.IsHttpStatusCodeForbidden())
        {
            _batchServices.WebsiteThrottle.ReportForbidden(page.Website);
        }
        else if (!page.IsHttpStatusCodeNotOK() && !page.IsResponseBodyNullOrEmpty())
        {
            _batchServices.WebsiteThrottle.ReportSuccess(page.Website);
        }
    }

    private async Task ReportThrottleResultAsync(
        Page page,
        CancellationToken cancellationToken)
    {
        if (page.IsHttpStatusCodeForbidden())
        {
            await _batchServices.WebsiteThrottle
                .ReportForbiddenAsync(page.Website, cancellationToken).ConfigureAwait(false);
        }
        else if (!page.IsHttpStatusCodeNotOK() && !page.IsResponseBodyNullOrEmpty())
        {
            await _batchServices.WebsiteThrottle
                .ReportSuccessAsync(page.Website, cancellationToken).ConfigureAwait(false);
        }
    }

    private PageScraper CreatePageScraper(Page page, bool? useProxy = null) =>
        useProxy is null
            ? new PageScraper(page, _pagePersistence, _logger, _listingLifecycle, _pageScraping)
            : new PageScraper(
                page,
                useProxy.Value,
                _pagePersistence,
                _logger,
                _listingLifecycle,
                _pageScraping);
}
