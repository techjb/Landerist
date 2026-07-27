using PuppeteerSharp;

namespace landerist_library.Infrastructure.Browser;

public sealed class PuppeteerChromeBrowserInstaller : IChromeBrowserInstaller
{
    public bool Update()
    {
        try
        {
            BrowserFetcher browserFetcher = new();
            browserFetcher.DownloadAsync().GetAwaiter().GetResult();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
