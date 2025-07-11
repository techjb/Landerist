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
        private readonly bool UseProxy = false;

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

        public bool Download(Page Page)
        {
            SetUnavailable();
            Downloader.Download(Page);
            Scraped++;

            if (BrowserHasChrashed())
            {
                Chrashes++;
                RestartBrowser();
                return false;
            }
            SetAvailable();
            return true;
        }       

        public void CloseBrowser()
        {
            Downloader.CloseBrowser();
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
