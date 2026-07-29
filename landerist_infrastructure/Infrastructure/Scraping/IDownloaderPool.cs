using landerist_library.Pages;

namespace landerist_library.Infrastructure.Scraping;

public interface IDownloaderPool
{
    bool Download(Page page, bool useProxy);

    Task<bool> DownloadAsync(
        Page page,
        bool useProxy,
        CancellationToken cancellationToken = default);

    void Clear();
}
