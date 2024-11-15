using landerist_library.Downloaders.Puppeteer;

namespace landerist_library.Downloaders.Multiple
{
    public class SingleDownloader
    {
        private readonly PuppeteerDownloader PuppeteerDownloader = new();
        private bool Available;

        public SingleDownloader()
        {
            Available = PuppeteerDownloader.BrowserInitialized();
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

        public void Download(Websites.Page Page)
        {
            SetUnavailable();
            PuppeteerDownloader.Download(Page);
            SetAvailable();
        }

        public string? GetRedirectUrl()
        {
            return PuppeteerDownloader.RedirectUrl;
        }

        public void CloseBrowser()
        {
            PuppeteerDownloader.CloseBrowser();
        }
    }
}
