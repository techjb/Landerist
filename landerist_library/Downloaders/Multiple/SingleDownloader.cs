using landerist_library.Downloaders;
using landerist_library.Downloaders.Puppeteer;
using landerist_library.Websites;

namespace landerist_library.Downloaders.Multiple
{
    public class SingleDownloader
    {
        private readonly IDownloaderSessionFactory DownloaderSessionFactory;
        private IDownloaderSession Downloader;
        private bool Available;
        public int Id = 0;
        private int Chrashes = 0;
        private int Scraped = 0;
        private readonly bool UseProxy;

        public SingleDownloader(int id, bool useProxy) : this(useProxy)
        {
            Id = id;
        }

        public SingleDownloader(bool useProxy) : this(useProxy, new PuppeteerDownloaderFactory())
        {
        }

        public SingleDownloader(bool useProxy, IDownloaderSessionFactory downloaderSessionFactory)
        {
            UseProxy = useProxy;
            DownloaderSessionFactory = downloaderSessionFactory;
            Downloader = DownloaderSessionFactory.Create(UseProxy);
            Available = Downloader.BrowserInitialized();
        }

        public bool TryReserve(bool useProxy)
        {
            if (!Available || useProxy != UseProxy)
            {
                return false;
            }

            Available = false;
            return true;
        }

        private void Release()
        {
            Available = true;
        }

        public bool GetUseProxy()
        {
            return UseProxy;
        }

        public bool Download(Page page)
        {
            ArgumentNullException.ThrowIfNull(page);

            var restartedBrowser = false;

            try
            {
                Downloader.Download(page);
                Scraped++;

                if (BrowserHasChrashed())
                {
                    Chrashes++;
                    RestartBrowser();
                    restartedBrowser = true;
                    return false;
                }

                return true;
            }
            catch //(Exception ex)
            {
                Chrashes++;
                RestartBrowser();
                restartedBrowser = true;
                return false;
            }
            finally
            {
                if (!restartedBrowser)
                {
                    Release();
                }
            }
        }

        public void CloseBrowser()
        {
            Downloader.CloseBrowser();
            Available = false;
        }

        public bool BrowserHasChrashed()
        {
            return Downloader.BrowserHasChrashed();
        }

        public void RestartBrowser()
        {
            CloseBrowser();
            Downloader = DownloaderSessionFactory.Create(UseProxy);
            Available = Downloader.BrowserInitialized();
        }

        public int ScrapedCounter()
        {
            return Scraped;
        }

        public int CrashesCounter()
        {
            return Chrashes;
        }
    }
}
