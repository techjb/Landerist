using landerist_library.Downloaders.Puppeteer;
using landerist_library.Websites;

namespace landerist_library.Downloaders.Multiple
{
    public class SingleDownloader
    {
        private readonly PuppeteerDownloader PuppeteerDownloader;
        private bool Available;
        private readonly List<Page> Scrapped = [];
        public int Id = 0;

        public SingleDownloader(int id) : this()
        {
            Id = id;
        }

        public SingleDownloader()
        {
            PuppeteerDownloader = new();
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

        public void Download(Page Page)
        {
            SetUnavailable();
            PuppeteerDownloader.Download(Page);
            Scrapped.Add(Page);
            if (BrowserHasErrors())
            {
                CloseBrowser();
                return;
            }
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

        public bool BrowserHasErrors()
        {
            return PuppeteerDownloader.BrowserWithErrors();
        }

        public int Count()
        {
            return Scrapped.Count;
        }
    }
}
