namespace landerist_library.Downloaders
{
    public class Downloader
    {
        private readonly PuppeteerDownloader PuppeteerDownloader = new();

        public bool IsAvailable { get; private set; } = true;


        public void SetUnavailable()
        {
            IsAvailable = false;
        }

        public void SetAvailable()
        {
            IsAvailable = true;
        }

        public bool ContainsBrowser()
        {
            return PuppeteerDownloader.BrowserInitialized();
        }

        public void Download(Websites.Page Page)
        {
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
