using landerist_library.Application.Scraping;
using landerist_library.Index;
using landerist_library.Pages;

namespace landerist_library.Infrastructure.Scraping;

public sealed class LegacyPageIndexingService : IPageIndexingService
{
    public void Index(Page page) => new Indexer(page).IndexPages();
}
