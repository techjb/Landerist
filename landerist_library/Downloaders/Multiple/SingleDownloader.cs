using landerist_library.Downloaders.Puppeteer;
using landerist_library.Websites;

namespace landerist_library.Downloaders.Multiple
{
    public class SingleDownloader
    {
        private PuppeteerDownloader Downloader;
        private bool Available;
        public int Id = 0;
        private int Chrashes = 0;
        private int Scraped = 0;
        private readonly bool UseProxy;

        public SingleDownloader(int id, bool useProxy) : this(useProxy)
        {
            Id = id;
        }

        public SingleDownloader(bool useProxy)
        {
            UseProxy = useProxy;
            Downloader = new(this);
            Available = Downloader.BrowserInitialized();
        }

        public void SetUnavailable()
        {
            Available = false;
        }

        public void SetAvailable()
        {
            Available = true;
        }

        public bool IsAvailable(bool useProxy)
        {
            return Available && useProxy == UseProxy;
        }

        public bool GetUseProxy()
        {
            return UseProxy;
        }

        public bool Download(Page page)
        {
            ArgumentNullException.ThrowIfNull(page);

            var restartedBrowser = false;
            SetUnavailable();

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
                    SetAvailable();
                }
            }
        }

        public void CloseBrowser()
        {
            Downloader.CloseBrowser();
            SetUnavailable();
        }

        public bool BrowserHasChrashed()
        {
            return Downloader.BrowserHasChrashed();
        }

        public void RestartBrowser()
        {
            CloseBrowser();
            Downloader = new(this);
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
