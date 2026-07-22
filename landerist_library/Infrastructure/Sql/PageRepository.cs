using landerist_library.Database;
using landerist_library.Pages;
using landerist_orels.ES;
using System.Data;

namespace landerist_library.Infrastructure.Sql
{
    public class PageRepository
    {
        private readonly IDatabase _database;

        public PageRepository() : this(new DataBase())
        {
        }

        public PageRepository(IDatabase database)
        {
            ArgumentNullException.ThrowIfNull(database);
            _database = database;
        }

        private IDatabase Database => _database;

        public DataRow? GetDataRow(string uriHash)
        {
            string query =
                "SELECT * " +
                "FROM " + Pages.Pages.PAGES + " " +
                "WHERE [UriHash] = @UriHash";

            var dataTable = Database.QueryTable(query, new Dictionary<string, object?> {
                {"UriHash", uriHash }
            });

            return dataTable.Rows.Count > 0
                ? dataTable.Rows[0]
                : null;
        }

        public bool Insert(IDictionary<string, object?> parameters)
        {
            string query =
                "INSERT INTO " + Pages.Pages.PAGES + " (" +
                "[Host], [Uri], [UriHash], [Inserted], [LastScrape], [LastParseListing], [NextScrape], [HttpStatusCode], [Etag], [LastModified], [PageType], " +
                "[PageTypeCounter], [LockedBy], [WaitingStatus], [ListingParserInputHash], " +
                "[ListingParserInputNotChangedCounter], [TransientErrorCounter], [ResponseBodyZipped], [TokenCount]) " +
                "VALUES(@Host, @Uri, @UriHash, @Inserted, @LastScrape, @LastParseListing, @NextScrape, @HttpStatusCode, @Etag, @LastModified, @PageType, " +
                "@PageTypeCounter, @LockedBy, @WaitingStatus, @ListingParserInputHash, " +
                "@ListingParserInputNotChangedCounter, @TransientErrorCounter, CONVERT(varbinary(max), @ResponseBodyZipped), @TokenCount)";

            return Database.Query(query, parameters);
        }

        public bool Update(IDictionary<string, object?> parameters, out Exception? exception)
        {
            string query =
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
                "[ResponseBodyZipped] = CASE WHEN @ResponseBodyZipped IS NULL THEN NULL ELSE CONVERT(varbinary(max), @ResponseBodyZipped) END," +
                "[TokenCount] = @TokenCount " +
                "WHERE [UriHash] = @UriHash";

            return Database.Query(query, parameters, out exception);
        }

        public bool UpdateNextScrape(string uriHash, DateTime? nextScrape)
        {
            string query =
               "UPDATE " + Pages.Pages.PAGES + " SET " +
               "[NextScrape] = @NextScrape " +
               "WHERE [UriHash] = @UriHash";

            return Database.Query(query, new Dictionary<string, object?> {
                {"UriHash", uriHash },
                {"NextScrape", nextScrape },
            });
        }

        public bool Delete(string uriHash)
        {
            string query =
                "DELETE FROM " + Pages.Pages.PAGES + " " +
                "WHERE [UriHash] = @UriHash";

            return Database.Query(query, new Dictionary<string, object?> {
                {"UriHash", uriHash }
            });
        }
        public bool ListingParserInputExistsOnAnotherListing(
            string host,
            string uriHash,
            string? listingParserInputHash)
        {
            string query =
                "SELECT 1 " +
                "FROM " + Pages.Pages.PAGES + " " +
                "WHERE [Host] = @Host AND " +
                "[UriHash] <> @UriHash AND " +
                "[ListingParserInputHash] = @ListingParserInputHash AND " +
                "EXISTS (SELECT 1 FROM " + ES_Listings.TABLE_ES_LISTINGS + " L " +
                "WHERE L.[guid] = " + Pages.Pages.PAGES + ".[UriHash])";

            return Database.QueryExists(query, new Dictionary<string, object?>
            {
                ["Host"] = host,
                ["UriHash"] = uriHash,
                ["ListingParserInputHash"] = listingParserInputHash
            });
        }

    }
}
