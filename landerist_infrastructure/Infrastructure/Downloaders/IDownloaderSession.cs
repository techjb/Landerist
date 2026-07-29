using landerist_library.Pages;

namespace landerist_library.Infrastructure.Downloaders
{
    public interface IDownloaderSession
    {
        public bool BrowserInitialized();

        public bool BrowserHasChrashed();

        public void CloseBrowser();

        public Task CloseBrowserAsync();

        public void Download(Page page);

        public Task DownloadAsync(
            Page page,
            CancellationToken cancellationToken = default);
    }
}
