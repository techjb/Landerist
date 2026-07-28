using landerist_library.Websites;
using landerist_orels.ES;
using System.Data;

namespace landerist_library.Infrastructure.Sql.Mapping;

public static class WebsiteDataMapper
{
    public static Website Map(DataRow row)
    {
        ArgumentNullException.ThrowIfNull(row);
        Website website = new(new Uri(row["MainUri"].ToString()!))
        {
            Host = row["Host"].ToString()!,
            LanguageCode = Enum.Parse<LanguageCode>(row["LanguageCode"].ToString()!),
            CountryCode = Enum.Parse<CountryCode>(row["CountryCode"].ToString()!),
            RobotsTxt = GetString(row, "RobotsTxt"),
            RobotsTxtUpdated = GetNullable<DateTime>(row, "RobotsTxtUpdated"),
            SitemapUpdated = GetNullable<DateTime>(row, "SitemapUpdated"),
            IpAddress = GetString(row, "IpAddress"),
            IpAddressUpdated = GetNullable<DateTime>(row, "IpAddressUpdated"),
            IndexUrlRegex = GetString(row, "IndexUrlRegex"),
            SitemapUrlRegex = GetString(row, "SitemapUrlRegex"),
            ListingUrlRegex = GetString(row, "ListingUrlRegex"),
            ListingCoordinateRegex = GetTrimmedString(row, "ListingCoordinateRegex"),
            ListingHtmlRemoveXPath = GetString(row, "ListingHtmlRemoveXPath"),
            ListingUnavailableRegex = GetTrimmedString(row, "ListingUnavailableRegex"),
            NavigationWaitSelector = GetTrimmedString(row, "NavigationWaitSelector"),
            AllowedResourceTypes = GetString(row, "AllowedResourceTypes"),
            BlockedDomains = GetTrimmedString(row, "BlockedDomains"),
            UserAgent = GetTrimmedString(row, "UserAgent"),
            HttpRequestHeaders = GetTrimmedString(row, "HttpRequestHeaders"),
            HtmlIndexingEnabled = GetNullable<bool>(row, "HtmlIndexingEnabled") ?? false,
            UseProxy = GetNullable<bool>(row, "UseProxy") ?? false,
            MinimumRequestIntervalMilliseconds = GetNullable<int>(row, "MinimumRequestIntervalMilliseconds")
        };
        return website;
    }

    private static string? GetString(DataRow row, string column) =>
        HasValue(row, column) ? row[column].ToString() : null;

    private static string? GetTrimmedString(DataRow row, string column)
    {
        string? value = GetString(row, column);
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static T? GetNullable<T>(DataRow row, string column) where T : struct =>
        HasValue(row, column) ? (T)row[column] : null;

    private static bool HasValue(DataRow row, string column) =>
        row.Table.Columns.Contains(column) && row[column] is not DBNull;
}