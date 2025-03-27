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

        public SingleDownloader(int id) : this()
        {
            Id = id;
        }

        public SingleDownloader()
        {
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

        public bool IsAvailable()
        {
            return Available;
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

        public string? GetRedirectUrl()
        {
            return Downloader.RedirectUrl;
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
