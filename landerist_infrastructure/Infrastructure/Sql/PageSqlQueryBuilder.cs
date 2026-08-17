using landerist_library.Pages;
using landerist_library.Websites;
using PageTable = landerist_library.Pages.Pages;
using WebsiteTable = landerist_library.Websites.Websites;

namespace landerist_library.Infrastructure.Sql;

internal static class PageSqlQueryBuilder
{
    public static string Select(int? topRows = null)
    {
        if (topRows.HasValue)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(topRows.Value);
        }

        string top = topRows.HasValue ? "TOP " + topRows.Value : string.Empty;
        return
            "SELECT " + top + " " +
            SelectColumns() + " " +
            "FROM " + PageTable.PAGES + " " +
            "INNER JOIN " + WebsiteTable.WEBSITES +
            " ON " + PageTable.PAGES + ".[Host] = " + WebsiteTable.WEBSITES + ".[Host] ";
    }

    public static string OutputColumns(string pagesAlias) =>
        "OUTPUT " + SelectPageColumns(pagesAlias) + ", " + SelectWebsiteColumns("W") + " ";

    public static string SelectColumns(string pagesTableName = "")
    {
        string pages = string.IsNullOrEmpty(pagesTableName)
            ? PageTable.PAGES
            : pagesTableName;
        return SelectPageColumns(pages) + ", " + SelectWebsiteColumns(WebsiteTable.WEBSITES);
    }

    private static string SelectPageColumns(string table) =>
        table + ".[Host], " +
        table + ".[Uri], " +
        table + ".[UriHash], " +
        table + ".[Inserted], " +
        table + ".[LastScrape], " +
        table + ".[LastParseListing], " +
        table + ".[NextScrape], " +
        table + ".[HttpStatusCode], " +
        table + ".[Etag], " +
        table + ".[LastModified], " +
        table + ".[PageType], " +
        table + ".[PageTypeCounter], " +
        table + ".[LockedBy], " +
        table + ".[WaitingStatus], " +
        table + ".[ListingParserInputHash], " +
        table + ".[ListingParserInputNotChangedCounter], " +
        table + ".[TransientErrorCounter], " +
        table + ".[ResponseBodyZipped], " +
        table + ".[TokenCount]";

    private static string SelectWebsiteColumns(string table) =>
        table + ".[MainUri], " +
        table + ".[LanguageCode], " +
        table + ".[CountryCode], " +
        table + ".[RobotsTxt], " +
        table + ".[RobotsTxtUpdated], " +
        table + ".[SitemapUpdated], " +
        table + ".[IpAddress], " +
        table + ".[IpAddressUpdated], " +
        table + ".[IndexUrlRegex], " +
        table + ".[SitemapUrlRegex], " +
        table + ".[ListingUrlRegex], " +
        table + ".[ListingCoordinateRegex], " +
        table + ".[ListingHtmlRemoveXPath], " +
        table + ".[ListingUnavailableRegex], " +
        table + ".[NavigationWaitSelector], " +
        table + ".[AllowedResourceTypes], " +
        table + ".[BlockedDomains], " +
        table + ".[UserAgent], " +
        table + ".[HttpRequestHeaders], " +
        table + ".[HtmlIndexingEnabled], " +
        table + ".[UseProxy], " +
        table + ".[MinimumRequestIntervalMilliseconds]";
}
