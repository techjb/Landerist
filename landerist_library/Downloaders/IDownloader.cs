using landerist_library.Websites;

namespace landerist_library.Downloaders
{
    internal interface IDownloader
    {
        public void SetResponseBodyAndStatusCode(Page page);

        public string? GetRedirectUrl();
        
    }
}
