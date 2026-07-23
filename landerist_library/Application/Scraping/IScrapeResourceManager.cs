namespace landerist_library.Application.Scraping;

public interface IScrapeResourceManager
{
    void ClearDownloaders();

    void CleanPageLocks();

    void KillChrome();

    void UpdateChrome();
}
