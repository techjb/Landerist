using landerist_library.Application.Scraping;
using landerist_library.Pages;

namespace landerist_library.Infrastructure.Scraping;

public sealed class PooledPageDownloader(IDownloaderPool pool) : IPageDownloader
{
    public bool Download(Page page, bool useProxy) =>
        pool.Download(page, useProxy);

    public Task<bool> DownloadAsync(
        Page page,
        bool useProxy,
        CancellationToken cancellationToken = default) =>
        pool.DownloadAsync(page, useProxy, cancellationToken);
}
