using landerist_library.Application.Logging;
using landerist_library.Application.Scraping;
using landerist_library.Infrastructure.Browser;

namespace landerist_library.Infrastructure.Scraping;

public sealed class ScrapeBrowserManager(
    IDownloaderPool pool,
    ChromeMaintenanceService chrome,
    IApplicationLogger logger) : IScrapeBrowserManager
{
    public void ClearDownloaders() => pool.Clear();

    public void KillChrome() => chrome.KillChrome();

    public void UpdateChrome() => logger.WriteInfo(
        "service",
        "Updating Chrome. Success: " + chrome.UpdateChrome());
}
