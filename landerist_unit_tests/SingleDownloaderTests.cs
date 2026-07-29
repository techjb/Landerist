using landerist_library.Infrastructure.Downloaders;
using landerist_library.Infrastructure.Downloaders.Multiple;
using landerist_library.Pages;
using landerist_library.Websites;

namespace landerist_unit_tests;

public sealed class SingleDownloaderTests
{
    [Fact]
    public async Task DownloadAsync_WhenCancelled_ReleasesReservation()
    {
        RecordingDownloaderSession session = new();
        SingleDownloader downloader = new(
            useProxy: false,
            new StubDownloaderSessionFactory(session));
        Assert.True(downloader.TryReserve(useProxy: false));
        using CancellationTokenSource cancellation = new();
        Page page = new(
            new Website(new Uri("https://example.com")),
            new Uri("https://example.com/listing/1"));

        Task<bool> download = downloader.DownloadAsync(page, cancellation.Token);
        await session.Entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
        cancellation.Cancel();

        await Assert.ThrowsAsync<TaskCanceledException>(() => download);
        Assert.True(downloader.TryReserve(useProxy: false));
    }

    private sealed class StubDownloaderSessionFactory(IDownloaderSession session)
        : IDownloaderSessionFactory
    {
        public IDownloaderSession Create(bool useProxy) => session;
    }

    private sealed class RecordingDownloaderSession : IDownloaderSession
    {
        public TaskCompletionSource Entered { get; } = new(
            TaskCreationOptions.RunContinuationsAsynchronously);

        public bool BrowserInitialized() => true;

        public bool BrowserHasChrashed() => false;

        public void CloseBrowser()
        {
        }

        public void Download(Page page)
        {
        }

        public async Task DownloadAsync(
            Page page,
            CancellationToken cancellationToken = default)
        {
            Entered.SetResult();
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
        }
    }
}