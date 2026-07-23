using landerist_library.Application.Scraping;
using landerist_library.Downloaders.Multiple;
using landerist_library.Downloaders.Puppeteer;

namespace landerist_library.Infrastructure.Scraping;

public sealed class LegacyScrapeResourceManager : IScrapeResourceManager
{
    public void ClearDownloaders() => DownloadersPool.Clear();

    public void CleanPageLocks() =>
        global::landerist_library.Pages.Pages.CleanLockedBy();

    public void KillChrome() => ChromeKiller.KillChrome();

    public void UpdateChrome() => PuppeteerDownloader.UpdateChrome();
}
