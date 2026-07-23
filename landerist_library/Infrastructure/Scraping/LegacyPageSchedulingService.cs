using landerist_library.Application.Scraping;
using landerist_library.Pages;

namespace landerist_library.Infrastructure.Scraping;

public sealed class LegacyPageSchedulingService : IPageSchedulingService
{
    public void SetNextScrape(Page page) =>
        global::landerist_library.Pages.Pages.SetNextScrape(page);

    public void SetNextScrapeFromNow(Page page) =>
        global::landerist_library.Pages.Pages.SetNextScrapeFromNow(page);
}
