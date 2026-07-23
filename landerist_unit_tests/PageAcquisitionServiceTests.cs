using landerist_library.Application.Scraping;
using landerist_library.Pages;
using landerist_library.Websites;

namespace landerist_unit_tests;

public sealed class PageAcquisitionServiceTests
{
    [Fact]
    public void Acquire_WithoutConditionalMetadata_DownloadsDirectly()
    {
        Page page = CreatePage();
        RecordingPageDownloader downloader = new() { Result = true };
        RecordingConditionalHeaders conditionalHeaders = new();
        RecordingScrapeMetrics metrics = new();
        PageAcquisitionService service = new(
            downloader,
            conditionalHeaders,
            metrics,
            conditionalHeadersEnabled: true);

        var result = service.Acquire(page, useProxy: true);

        Assert.Equal(PageAcquisitionStatus.Downloaded, result);
        Assert.Equal(1, downloader.Calls);
        Assert.True(downloader.LastUseProxy);
        Assert.Equal(0, conditionalHeaders.Calls);
        Assert.Equal(0, metrics.ConditionalChecks);
    }

    [Fact]
    public void Acquire_WhenConditionalResponseIsNotModified_UpdatesHeadersAndSkipsDownload()
    {
        Page page = CreatePage();
        page.SetPageType(PageType.Listing);
        page.Etag = "old-etag";
        RecordingPageDownloader downloader = new() { Result = true };
        RecordingConditionalHeaders conditionalHeaders = new()
        {
            Result = new ConditionalPageHeaderResult
            {
                NotModified = true,
                Etag = "new-etag",
                LastModified = "new-last-modified",
                RedirectUrl = "https://example.com/listing/2"
            }
        };
        RecordingScrapeMetrics metrics = new();
        PageAcquisitionService service = new(
            downloader,
            conditionalHeaders,
            metrics,
            conditionalHeadersEnabled: true);

        var result = service.Acquire(page, useProxy: false);

        Assert.Equal(PageAcquisitionStatus.NotModified, result);
        Assert.Equal(0, downloader.Calls);
        Assert.Equal(1, conditionalHeaders.Calls);
        Assert.Equal(1, metrics.ConditionalChecks);
        Assert.Equal(1, metrics.NotModifiedPages);
        Assert.Equal("new-etag", page.Etag);
        Assert.Equal("new-last-modified", page.LastModified);
        Assert.Equal("https://example.com/listing/2", page.RedirectUrl);
    }

    [Fact]
    public void Acquire_WhenConditionalResponseChanged_DownloadsPage()
    {
        Page page = CreatePage();
        page.SetPageType(PageType.Listing);
        page.LastModified = "previous";
        RecordingPageDownloader downloader = new() { Result = true };
        RecordingConditionalHeaders conditionalHeaders = new()
        {
            Result = new ConditionalPageHeaderResult { NotModified = false }
        };
        RecordingScrapeMetrics metrics = new();
        PageAcquisitionService service = new(
            downloader,
            conditionalHeaders,
            metrics,
            conditionalHeadersEnabled: true);

        var result = service.Acquire(page, useProxy: false);

        Assert.Equal(PageAcquisitionStatus.Downloaded, result);
        Assert.Equal(1, conditionalHeaders.Calls);
        Assert.Equal(1, metrics.ConditionalChecks);
        Assert.Equal(0, metrics.NotModifiedPages);
        Assert.Equal(1, downloader.Calls);
    }

    [Fact]
    public void Acquire_WhenDownloadFails_ReturnsDownloadFailed()
    {
        Page page = CreatePage();
        RecordingPageDownloader downloader = new() { Result = false };
        PageAcquisitionService service = new(
            downloader,
            new RecordingConditionalHeaders(),
            new RecordingScrapeMetrics(),
            conditionalHeadersEnabled: false);

        var result = service.Acquire(page, useProxy: false);

        Assert.Equal(PageAcquisitionStatus.DownloadFailed, result);
        Assert.Equal(1, downloader.Calls);
    }

    private static Page CreatePage() =>
        new(
            new Website(new Uri("https://example.com")),
            new Uri("https://example.com/listing/1"));

    private sealed class RecordingPageDownloader : IPageDownloader
    {
        public bool Result { get; init; }

        public int Calls { get; private set; }

        public bool LastUseProxy { get; private set; }

        public bool Download(Page page, bool useProxy)
        {
            Calls++;
            LastUseProxy = useProxy;
            return Result;
        }
    }

    private sealed class RecordingConditionalHeaders : IConditionalPageHeaderService
    {
        public ConditionalPageHeaderResult Result { get; init; } = new();

        public int Calls { get; private set; }

        public ConditionalPageHeaderResult Check(Page page, bool useProxy)
        {
            Calls++;
            return Result;
        }
    }

    private sealed class RecordingScrapeMetrics : IScrapeMetrics
    {
        public int ConditionalChecks { get; private set; }

        public int NotModifiedPages { get; private set; }

        public void RecordConditionalHeaderCheck() => ConditionalChecks++;

        public void RecordPageNotModified(Page page) => NotModifiedPages++;
    }
}
