using landerist_library.Application.Scraping;
using landerist_library.Pages;

namespace landerist_library.Infrastructure.Scraping;

public sealed class LegacyPageSelectionRepository : IPageSelectionRepository
{
    public void CleanLockedPages() =>
        global::landerist_library.Pages.Pages.CleanLockedBy();

    public IReadOnlyList<Page> GetScrapePages(int maximumCount) =>
        global::landerist_library.Pages.Pages.GetScrapePages(maximumCount);
}
