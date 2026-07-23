using landerist_library.Pages;

namespace landerist_library.Application.Scraping;

public interface IPageDownloader
{
    bool Download(Page page, bool useProxy);
}
