using landerist_library.Pages;

namespace landerist_library.Application.Scraping;

public interface IPageIndexingService
{
    void Index(Page page);
}
