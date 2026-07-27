using landerist_library.Application.Scraping;
using landerist_library.Downloaders.Multiple;
using landerist_library.Infrastructure.Browser;

namespace landerist_library.Infrastructure.Scraping;

public sealed class ScrapeBrowserManager(DownloadersPool pool, ChromeMaintenanceService chrome) : IScrapeBrowserManager
{
    public void ClearDownloaders() => pool.Clear();

    public void KillChrome() => chrome.KillChrome();

    public void UpdateChrome() => Logs.Log.WriteInfo("service", "Updating Chrome. Success: " + chrome.UpdateChrome());
}