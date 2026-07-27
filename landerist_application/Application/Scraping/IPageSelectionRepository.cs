using landerist_library.Pages;

namespace landerist_library.Application.Scraping;

public interface IPageSelectionRepository
{
    void CleanLockedPages();

    IReadOnlyList<Page> GetScrapePages(int maximumCount);
}
