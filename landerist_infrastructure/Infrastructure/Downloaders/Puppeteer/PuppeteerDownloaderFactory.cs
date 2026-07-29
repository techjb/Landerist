using landerist_library.Application.Logging;
using landerist_library.Infrastructure.Downloaders;

namespace landerist_library.Infrastructure.Downloaders.Puppeteer
{
    public class PuppeteerDownloaderFactory(
        PuppeteerBrowserOptions options,
        IApplicationLogger logger) : IDownloaderSessionFactory
    {
        public IDownloaderSession Create(bool useProxy)
        {
            return new PuppeteerDownloader(useProxy, options, logger);
        }
        public async Task<IDownloaderSession> CreateAsync(
            bool useProxy,
            CancellationToken cancellationToken = default)
        {
            var downloader = new PuppeteerDownloader(
                useProxy,
                options,
                logger,
                initializeSynchronously: false);
            await downloader.InitializeAsync(cancellationToken).ConfigureAwait(false);
            return downloader;
        }
    }
}
