using landerist_library.Application.Scraping;
using landerist_library.Downloaders.Multiple;
using landerist_library.Pages;

namespace landerist_library.Infrastructure.Scraping;

public sealed class PooledPageDownloader(DownloadersPool pool) : IPageDownloader
{
    public bool Download(Page page, bool useProxy) =>
        pool.Download(page, useProxy);
}
