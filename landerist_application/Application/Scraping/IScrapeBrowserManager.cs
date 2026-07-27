namespace landerist_library.Application.Scraping;

public interface IScrapeBrowserManager
{
    void ClearDownloaders();

    void KillChrome();

    void UpdateChrome();
}
