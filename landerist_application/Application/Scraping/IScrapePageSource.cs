using landerist_library.Pages;
using landerist_library.Websites;
using landerist_orels.ES;

namespace landerist_library.Application.Scraping;

public interface IScrapePageSource
{
    Page LoadOrCreate(Uri uri);

    IReadOnlyList<Page> GetPages(Website website);

    Listing? GetListing(Page page, bool loadMedia, bool loadSources);
}
