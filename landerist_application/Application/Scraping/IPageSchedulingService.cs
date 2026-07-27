using landerist_library.Pages;

namespace landerist_library.Application.Scraping;

public interface IPageSchedulingService
{
    void SetNextScrape(Page page);

    void SetNextScrapeFromNow(Page page);
}
