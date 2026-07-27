using landerist_library.Downloaders;

namespace landerist_library.Downloaders.Puppeteer
{
    public class PuppeteerDownloaderFactory(PuppeteerBrowserOptions options) : IDownloaderSessionFactory
    {
        public IDownloaderSession Create(bool useProxy)
        {
            return new PuppeteerDownloader(useProxy, options);
        }
    }
}
