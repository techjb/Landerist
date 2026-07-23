using landerist_library.Application.Scraping;
using landerist_library.Pages;
using landerist_library.Websites;
using landerist_orels.ES;

namespace landerist_library.Infrastructure.Scraping;

public sealed class LegacyScrapePageSource : IScrapePageSource
{
    public Page LoadOrCreate(Uri uri) =>
        global::landerist_library.Pages.Pages.LoadOrCreate(uri);

    public IReadOnlyList<Page> GetPages(Website website) =>
        global::landerist_library.Websites.Websites.GetPages(website);

    public Listing? GetListing(Page page, bool loadMedia, bool loadSources) =>
        global::landerist_library.Pages.Pages.GetListing(page, loadMedia, loadSources);
}
