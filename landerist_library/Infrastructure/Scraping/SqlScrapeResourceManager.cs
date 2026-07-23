using landerist_library.Application.Scraping;
using landerist_library.Database;
using landerist_library.Downloaders.Multiple;
using landerist_library.Downloaders.Puppeteer;
using landerist_library.Infrastructure.Sql;

namespace landerist_library.Infrastructure.Scraping;

public sealed class SqlScrapeResourceManager : IScrapeResourceManager
{
    private readonly PageMaintenanceRepository _pages;
    private readonly string _machineName;

    public SqlScrapeResourceManager(IDatabase database, string machineName)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentException.ThrowIfNullOrWhiteSpace(machineName);
        _pages = new PageMaintenanceRepository(database);
        _machineName = machineName;
    }

    public void ClearDownloaders() => DownloadersPool.Clear();

    public void CleanPageLocks() => _pages.CleanLockedBy(_machineName);

    public void KillChrome() => ChromeKiller.KillChrome();

    public void UpdateChrome() => PuppeteerDownloader.UpdateChrome();
}
