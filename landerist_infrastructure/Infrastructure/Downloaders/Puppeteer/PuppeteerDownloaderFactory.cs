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
    }
}
