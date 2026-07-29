using landerist_library.Pages;

namespace landerist_library.Application.Scraping;

public sealed class PageAcquisitionService : IPageAcquisitionService
{
    private readonly IPageDownloader _downloader;
    private readonly IConditionalPageHeaderService _conditionalHeaders;
    private readonly IScrapeMetrics _metrics;
    private readonly bool _conditionalHeadersEnabled;

    public PageAcquisitionService(
        IPageDownloader downloader,
        IConditionalPageHeaderService conditionalHeaders,
        IScrapeMetrics metrics,
        bool conditionalHeadersEnabled)
    {
        ArgumentNullException.ThrowIfNull(downloader);
        ArgumentNullException.ThrowIfNull(conditionalHeaders);
        ArgumentNullException.ThrowIfNull(metrics);

        _downloader = downloader;
        _conditionalHeaders = conditionalHeaders;
        _metrics = metrics;
        _conditionalHeadersEnabled = conditionalHeadersEnabled;
    }

    public PageAcquisitionStatus Acquire(Page page, bool useProxy)
    {
        ArgumentNullException.ThrowIfNull(page);

        if (CanCheckConditionalHeaders(page))
        {
            _metrics.RecordConditionalHeaderCheck();
            var result = _conditionalHeaders.Check(page, useProxy);
            if (result.NotModified)
            {
                ApplyConditionalHeaders(page, result);
                _metrics.RecordPageNotModified(page);
                return PageAcquisitionStatus.NotModified;
            }
        }

        return _downloader.Download(page, useProxy)
            ? PageAcquisitionStatus.Downloaded
            : PageAcquisitionStatus.DownloadFailed;
    }

    public async Task<PageAcquisitionStatus> AcquireAsync(
        Page page,
        bool useProxy,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(page);
        cancellationToken.ThrowIfCancellationRequested();

        if (CanCheckConditionalHeaders(page))
        {
            _metrics.RecordConditionalHeaderCheck();
            ConditionalPageHeaderResult result = await _conditionalHeaders
                .CheckAsync(page, useProxy, cancellationToken)
                .ConfigureAwait(false);
            if (result.NotModified)
            {
                ApplyConditionalHeaders(page, result);
                _metrics.RecordPageNotModified(page);
                return PageAcquisitionStatus.NotModified;
            }
        }

        bool downloaded = await _downloader
            .DownloadAsync(page, useProxy, cancellationToken)
            .ConfigureAwait(false);
        return downloaded
            ? PageAcquisitionStatus.Downloaded
            : PageAcquisitionStatus.DownloadFailed;
    }
    private bool CanCheckConditionalHeaders(Page page) =>
        _conditionalHeadersEnabled &&
        page.PageType.HasValue &&
        (!string.IsNullOrWhiteSpace(page.Etag) ||
            !string.IsNullOrWhiteSpace(page.LastModified));

    private static void ApplyConditionalHeaders(
        Page page,
        ConditionalPageHeaderResult result)
    {
        if (!string.IsNullOrWhiteSpace(result.Etag))
        {
            page.Etag = result.Etag;
        }

        if (!string.IsNullOrWhiteSpace(result.LastModified))
        {
            page.LastModified = result.LastModified;
        }

        page.RedirectUrl = result.RedirectUrl;
    }
}
