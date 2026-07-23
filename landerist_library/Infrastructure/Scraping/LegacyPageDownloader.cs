using landerist_library.Application.Scraping;
using landerist_library.Downloaders.Multiple;
using landerist_library.Pages;

namespace landerist_library.Infrastructure.Scraping;

public sealed class LegacyPageDownloader : IPageDownloader
{
    public bool Download(Page page, bool useProxy) =>
        DownloadersPool.Download(page, useProxy);
}
