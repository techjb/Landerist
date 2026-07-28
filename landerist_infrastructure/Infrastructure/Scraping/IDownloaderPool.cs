using landerist_library.Pages;

namespace landerist_library.Infrastructure.Scraping;

public interface IDownloaderPool
{
    bool Download(Page page, bool useProxy);

    void Clear();
}
