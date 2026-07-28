using landerist_library.Application.Persistence;
using landerist_library.Database;
using landerist_library.Websites;

namespace landerist_library.Infrastructure.Sql;

public sealed class WebsiteRepository : IWebsiteRepository
{
    private readonly IDatabase _database;

    public WebsiteRepository(IDatabase database)
    {
        ArgumentNullException.ThrowIfNull(database);
        _database = database;
    }

    public bool Insert(Website website)
    {
        ArgumentNullException.ThrowIfNull(website);
        string query =
            "INSERT INTO " + Websites.Websites.WEBSITES + " (" +
            "[MainUri], [Host], [LanguageCode], [CountryCode], [RobotsTxt], [RobotsTxtUpdated], " +
            "[SitemapUpdated], [IpAddress], [IpAddressUpdated], [IndexUrlRegex], [SitemapUrlRegex], [ListingUrlRegex], [ListingCoordinateRegex], [ListingHtmlRemoveXPath], [ListingUnavailableRegex], [NavigationWaitSelector], [AllowedResourceTypes], [BlockedDomains], [UserAgent], [HttpRequestHeaders], [HtmlIndexingEnabled], [UseProxy], [MinimumRequestIntervalMilliseconds]) VALUES (" +
            "@MainUri, @Host, @LanguageCode, @CountryCode, @RobotsTxt, @RobotsTxtUpdated, " +
            "@SitemapUpdated, @IpAddress, @IpAddressUpdated, @IndexUrlRegex, @SitemapUrlRegex, @ListingUrlRegex, @ListingCoordinateRegex, @ListingHtmlRemoveXPath, @ListingUnavailableRegex, @NavigationWaitSelector, @AllowedResourceTypes, @BlockedDomains, @UserAgent, @HttpRequestHeaders, @HtmlIndexingEnabled, @UseProxy, @MinimumRequestIntervalMilliseconds)";

        return _database.Query(query, GetParameters(website));
    }

    public bool Update(Website website)
    {
        ArgumentNullException.ThrowIfNull(website);
        string query =
            "UPDATE " + Websites.Websites.WEBSITES + " SET " +
            "[MainUri] = @MainUri, " +
            "[LanguageCode] = @LanguageCode, " +
            "[CountryCode] = @CountryCode, " +
            "[RobotsTxt] = @RobotsTxt, " +
            "[RobotsTxtUpdated] = @RobotsTxtUpdated, " +
            "[SitemapUpdated] = @SitemapUpdated, " +
            "[IpAddress] = @IpAddress, " +
            "[IpAddressUpdated] = @IpAddressUpdated, " +
            "[IndexUrlRegex] = @IndexUrlRegex, " +
            "[SitemapUrlRegex] = @SitemapUrlRegex, " +
            "[ListingUrlRegex] = @ListingUrlRegex, " +
            "[ListingCoordinateRegex] = @ListingCoordinateRegex, " +
            "[ListingHtmlRemoveXPath] = @ListingHtmlRemoveXPath, " +
            "[ListingUnavailableRegex] = @ListingUnavailableRegex, " +
            "[NavigationWaitSelector] = @NavigationWaitSelector, " +
            "[AllowedResourceTypes] = @AllowedResourceTypes, " +
            "[BlockedDomains] = @BlockedDomains, " +
            "[UserAgent] = @UserAgent, " +
            "[HttpRequestHeaders] = @HttpRequestHeaders, " +
            "[HtmlIndexingEnabled] = @HtmlIndexingEnabled, " +
            "[UseProxy] = @UseProxy, " +
            "[MinimumRequestIntervalMilliseconds] = @MinimumRequestIntervalMilliseconds " +
            "WHERE [Host] = @Host";

        return _database.Query(query, GetParameters(website));
    }

    public bool Delete(string host)
    {
        string query =
            "DELETE FROM " + Websites.Websites.WEBSITES + " " +
            "WHERE [Host] = @Host";

        return _database.Query(query, new Dictionary<string, object?> { ["Host"] = host });
    }

    private static Dictionary<string, object?> GetParameters(Website website) => new()
    {
        ["MainUri"] = website.MainUri.ToString(),
        ["Host"] = website.Host,
        ["LanguageCode"] = website.LanguageCode.ToString(),
        ["CountryCode"] = website.CountryCode.ToString(),
        ["RobotsTxt"] = website.RobotsTxt,
        ["RobotsTxtUpdated"] = website.RobotsTxtUpdated,
        ["SitemapUpdated"] = website.SitemapUpdated,
        ["IpAddress"] = website.IpAddress,
        ["IpAddressUpdated"] = website.IpAddressUpdated,
        ["IndexUrlRegex"] = website.IndexUrlRegex,
        ["SitemapUrlRegex"] = website.SitemapUrlRegex,
        ["ListingUrlRegex"] = website.ListingUrlRegex,
        ["ListingCoordinateRegex"] = NullIfWhiteSpace(website.ListingCoordinateRegex),
        ["ListingHtmlRemoveXPath"] = website.ListingHtmlRemoveXPath,
        ["ListingUnavailableRegex"] = NullIfWhiteSpace(website.ListingUnavailableRegex),
        ["NavigationWaitSelector"] = NullIfWhiteSpace(website.NavigationWaitSelector),
        ["AllowedResourceTypes"] = website.AllowedResourceTypes,
        ["BlockedDomains"] = NullIfWhiteSpace(website.BlockedDomains),
        ["UserAgent"] = NullIfWhiteSpace(website.UserAgent),
        ["HttpRequestHeaders"] = NullIfWhiteSpace(website.HttpRequestHeaders),
        ["HtmlIndexingEnabled"] = website.HtmlIndexingEnabled,
        ["UseProxy"] = website.UseProxy,
        ["MinimumRequestIntervalMilliseconds"] = website.MinimumRequestIntervalMilliseconds
    };

    private static string? NullIfWhiteSpace(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}