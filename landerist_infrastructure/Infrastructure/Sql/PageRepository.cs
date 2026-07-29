using landerist_library.Application.Persistence;
using landerist_library.Database;
using landerist_library.Pages;
using landerist_orels.ES;

namespace landerist_library.Infrastructure.Sql;

public sealed class PageRepository : IPageRepository
{
    private readonly IDatabase _database;

    public PageRepository(IDatabase database)
    {
        ArgumentNullException.ThrowIfNull(database);
        _database = database;
    }

    public bool Insert(Page page)
    {
        ArgumentNullException.ThrowIfNull(page);
        string query =
            "INSERT INTO " + Pages.Pages.PAGES + " (" +
            "[Host], [Uri], [UriHash], [Inserted], [LastScrape], [LastParseListing], [NextScrape], [HttpStatusCode], [Etag], [LastModified], [PageType], " +
            "[PageTypeCounter], [LockedBy], [WaitingStatus], [ListingParserInputHash], " +
            "[ListingParserInputNotChangedCounter], [TransientErrorCounter], [ResponseBodyZipped], [TokenCount]) " +
            "VALUES(@Host, @Uri, @UriHash, @Inserted, @LastScrape, @LastParseListing, @NextScrape, @HttpStatusCode, @Etag, @LastModified, @PageType, " +
            "@PageTypeCounter, @LockedBy, @WaitingStatus, @ListingParserInputHash, " +
            "@ListingParserInputNotChangedCounter, @TransientErrorCounter, CONVERT(varbinary(max), @ResponseBodyZipped), @TokenCount)";

        return _database.Query(query, GetParameters(page));
    }

    public bool Update(Page page, out Exception? exception)
    {
        ArgumentNullException.ThrowIfNull(page);
        return _database.Query(
            GetUpdateQuery(),
            GetParameters(page),
            out exception);
    }

    public Task<bool> UpdateAsync(
        Page page,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(page);
        return _database.QueryAsync(
            GetUpdateQuery(),
            GetParameters(page),
            cancellationToken);
    }

    private static string GetUpdateQuery() =>
        "UPDATE " + Pages.Pages.PAGES + " SET " +
        "[LastScrape] = @LastScrape, " +
        "[LastParseListing] = @LastParseListing, " +
        "[NextScrape] = @NextScrape, " +
        "[HttpStatusCode] = @HttpStatusCode, " +
        "[Etag] = @Etag, " +
        "[LastModified] = @LastModified, " +
        "[PageType] = @PageType, " +
        "[PageTypeCounter] = @PageTypeCounter, " +
        "[LockedBy] = @LockedBy, " +
        "[WaitingStatus] = @WaitingStatus, " +
        "[ListingParserInputHash] = @ListingParserInputHash, " +
        "[ListingParserInputNotChangedCounter] = @ListingParserInputNotChangedCounter, " +
        "[TransientErrorCounter] = @TransientErrorCounter, " +
        "[ResponseBodyZipped] = CASE WHEN @ResponseBodyZipped IS NULL THEN NULL ELSE CONVERT(varbinary(max), @ResponseBodyZipped) END, " +
        "[TokenCount] = @TokenCount " +
        "WHERE [UriHash] = @UriHash";
    public bool UpdateNextScrape(string uriHash, DateTime? nextScrape)
    {
        string query =
            "UPDATE " + Pages.Pages.PAGES + " SET " +
            "[NextScrape] = @NextScrape " +
            "WHERE [UriHash] = @UriHash";

        return _database.Query(query, new Dictionary<string, object?>
        {
            ["UriHash"] = uriHash,
            ["NextScrape"] = nextScrape
        });
    }

    public bool Delete(string uriHash)
    {
        string query =
            "DELETE FROM " + Pages.Pages.PAGES + " " +
            "WHERE [UriHash] = @UriHash";

        return _database.Query(query, new Dictionary<string, object?> { ["UriHash"] = uriHash });
    }

    public bool ListingParserInputExistsOnAnotherListing(string host, string uriHash, string? listingParserInputHash)
    {
        string query =
            "SELECT 1 " +
            "FROM " + Pages.Pages.PAGES + " " +
            "WHERE [Host] = @Host AND " +
            "[UriHash] <> @UriHash AND " +
            "[ListingParserInputHash] = @ListingParserInputHash AND " +
            "EXISTS (SELECT 1 FROM " + SqlTableNames.Listings + " L " +
            "WHERE L.[guid] = " + Pages.Pages.PAGES + ".[UriHash])";

        return _database.QueryExists(query, new Dictionary<string, object?>
        {
            ["Host"] = host,
            ["UriHash"] = uriHash,
            ["ListingParserInputHash"] = listingParserInputHash
        });
    }

    private static Dictionary<string, object?> GetParameters(Page page) => new()
    {
        ["Host"] = page.Host,
        ["Uri"] = page.Uri.ToString(),
        ["UriHash"] = page.UriHash,
        ["Inserted"] = page.Inserted,
        ["LastScrape"] = page.LastScrape,
        ["LastParseListing"] = page.LastParseListing,
        ["NextScrape"] = page.NextScrape,
        ["HttpStatusCode"] = page.HttpStatusCode,
        ["Etag"] = page.Etag,
        ["LastModified"] = page.LastModified,
        ["PageType"] = page.PageType?.ToString(),
        ["PageTypeCounter"] = page.PageTypeCounter,
        ["LockedBy"] = page.LockedBy,
        ["WaitingStatus"] = page.WaitingStatus?.ToString(),
        ["ListingParserInputHash"] = page.ListingParserInputHash,
        ["ListingParserInputNotChangedCounter"] = page.ListingParserInputNotChangedCounter,
        ["TransientErrorCounter"] = page.TransientErrorCounter,
        ["ResponseBodyZipped"] = page.ResponseBodyZipped,
        ["TokenCount"] = page.TokenCount
    };
}