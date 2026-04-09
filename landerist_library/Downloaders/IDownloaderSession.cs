using landerist_library.Websites;

namespace landerist_library.Downloaders
{
    public interface IDownloaderSession
    {
        public bool BrowserInitialized();

        public bool BrowserHasChrashed();

        public void CloseBrowser();

        public void Download(Page page);
    }
}
