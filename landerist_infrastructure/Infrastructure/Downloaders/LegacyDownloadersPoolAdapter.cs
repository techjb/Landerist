using landerist_library.Infrastructure.Downloaders.Multiple;
using landerist_library.Infrastructure.Scraping;
using landerist_library.Pages;

namespace landerist_library.Infrastructure.Downloaders;

public sealed class LegacyDownloadersPoolAdapter(DownloadersPool pool) : IDownloaderPool
{
    public bool Download(Page page, bool useProxy) => pool.Download(page, useProxy);

    public Task<bool> DownloadAsync(
        Page page,
        bool useProxy,
        CancellationToken cancellationToken = default) =>
        pool.DownloadAsync(page, useProxy, cancellationToken);

    public void Clear() => pool.Clear();

    public Task ClearAsync(CancellationToken cancellationToken = default) =>
        pool.ClearAsync(cancellationToken);
}
