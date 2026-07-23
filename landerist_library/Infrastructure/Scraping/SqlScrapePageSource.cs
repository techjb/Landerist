using landerist_library.Application.Listings;
using landerist_library.Application.Scraping;
using landerist_library.Database;
using landerist_library.Infrastructure.Sql;
using landerist_library.Infrastructure.Sql.Mapping;
using landerist_library.Pages;
using landerist_library.Websites;
using landerist_orels.ES;
using System.Data;

namespace landerist_library.Infrastructure.Scraping;

public sealed class SqlScrapePageSource : IScrapePageSource
{
    private readonly PageQueryRepository _pages;
    private readonly WebsiteQueryRepository _websites;
    private readonly IListingStore _listings;

    public SqlScrapePageSource(IDatabase database, IListingStore listings)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(listings);
        _pages = new PageQueryRepository(database);
        _websites = new WebsiteQueryRepository(database);
        _listings = listings;
    }

    public Page LoadOrCreate(Uri uri)
    {
        ArgumentNullException.ThrowIfNull(uri);
        DataTable pageRows = _pages.GetPageByUriHash(Tools.Strings.GetHash(uri.ToString()));
        if (pageRows.Rows.Count == 1)
        {
            Website existingWebsite = WebsiteDataMapper.Map(pageRows.Rows[0]);
            return PageDataMapper.Map(pageRows.Rows[0], existingWebsite);
        }

        DataTable websiteRows = _websites.GetWebsite(uri.Host);
        if (websiteRows.Rows.Count != 1)
        {
            throw new KeyNotFoundException($"Website not found for host: {uri.Host}");
        }

        return new Page(WebsiteDataMapper.Map(websiteRows.Rows[0]), uri);
    }

    public IReadOnlyList<Page> GetPages(Website website)
    {
        ArgumentNullException.ThrowIfNull(website);
        DataTable rows = _pages.GetPagesByHost(website.Host);
        List<Page> result = new(rows.Rows.Count);
        foreach (DataRow row in rows.Rows)
        {
            result.Add(PageDataMapper.Map(row, website));
        }
        return result;
    }

    public Listing? GetListing(Page page, bool loadMedia, bool loadSources) =>
        _listings.Get(page, loadMedia, loadSources);
}
