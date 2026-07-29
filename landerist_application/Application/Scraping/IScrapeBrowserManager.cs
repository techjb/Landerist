namespace landerist_library.Application.Scraping;

public interface IScrapeBrowserManager
{
    void ClearDownloaders();

    Task ClearDownloadersAsync(CancellationToken cancellationToken = default);

    void KillChrome();

    void UpdateChrome();
}
