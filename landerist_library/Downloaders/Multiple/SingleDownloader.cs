using landerist_library.Downloaders.Puppeteer;
using landerist_library.Websites;


namespace landerist_library.Downloaders.Multiple
{
    public class SingleDownloader
    {
        private readonly PuppeteerDownloader Downloader;
        private bool Available;
        private readonly List<Page> Scrapped = [];
        public int Id = 0;
        public int ErrorsCounter = 0;
        public int SucessCounter = 0;

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

        public void Download(Page Page)
        {
            SetUnavailable();
            Downloader.Download(Page);
            //if (BrowserHasErrors())
            //{
            //    CloseBrowser();
            //    return;
            //}
            if (BrowserHasErrors())
            {
                ErrorsCounter++;
                Downloader.ClosePage();
                Task.Run(() => Task.Delay(500)).Wait();
            }
            else
            {
                SucessCounter++;
            }
            Scrapped.Add(Page);
            SetAvailable();
        }

        public string? GetRedirectUrl()
        {
            return Downloader.RedirectUrl;
        }

        public void CloseBrowser()
        {
            Downloader.CloseBrowser();
        }

        public bool BrowserHasErrors()
        {
            return Downloader.BrowserWithErrors();
        }

        public int ScrapedCount()
        {
            return Scrapped.Count;
        }
    }
}
