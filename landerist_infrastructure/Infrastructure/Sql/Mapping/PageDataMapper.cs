using landerist_library.Pages;
using landerist_library.Websites;
using System.Data;

namespace landerist_library.Infrastructure.Sql.Mapping;

public static class PageDataMapper
{
    public static Page Map(DataRow row, Website website)
    {
        ArgumentNullException.ThrowIfNull(row);
        ArgumentNullException.ThrowIfNull(website);

        Page page = new(website, new Uri(row["Uri"].ToString()!))
        {
            Host = row["Host"].ToString()!,
            UriHash = row["UriHash"].ToString()!,
            Inserted = (DateTime)row["Inserted"],
            LastScrape = GetNullable<DateTime>(row, "LastScrape"),
            LastParseListing = GetNullable<DateTime>(row, "LastParseListing"),
            NextScrape = GetNullable<DateTime>(row, "NextScrape"),
            HttpStatusCode = GetNullable<short>(row, "HttpStatusCode"),
            Etag = GetString(row, "Etag"),
            LastModified = GetString(row, "LastModified"),
            PageType = ParsePageType(GetString(row, "PageType")),
            PageTypeCounter = GetNullable<short>(row, "PageTypeCounter"),
            LockedBy = GetString(row, "LockedBy"),
            WaitingStatus = ParseWaitingStatus(GetString(row, "WaitingStatus")),
            ListingParserInputHash = GetString(row, "ListingParserInputHash"),
            ListingParserInputNotChangedCounter = GetNullable<short>(row, "ListingParserInputNotChangedCounter"),
            TransientErrorCounter = GetNullable<short>(row, "TransientErrorCounter"),
            ResponseBodyZipped = HasValue(row, "ResponseBodyZipped") ? (byte[])row["ResponseBodyZipped"] : null,
            TokenCount = GetNullable<int>(row, "TokenCount")
        };
        return page;
    }

    private static PageType? ParsePageType(string? value)
    {
        if (value is null)
        {
            return null;
        }
        return value == "HttpStatusCodeNotOK"
            ? Pages.PageType.HttpStatusCodeOtherNotOK
            : Enum.Parse<PageType>(value);
    }

    private static WaitingStatus? ParseWaitingStatus(string? value) =>
        value is null ? null : Enum.Parse<WaitingStatus>(value);

    private static string? GetString(DataRow row, string column) =>
        HasValue(row, column) ? row[column].ToString() : null;

    private static T? GetNullable<T>(DataRow row, string column) where T : struct =>
        HasValue(row, column) ? (T)row[column] : null;

    private static bool HasValue(DataRow row, string column) =>
        row.Table.Columns.Contains(column) && row[column] is not DBNull;
}