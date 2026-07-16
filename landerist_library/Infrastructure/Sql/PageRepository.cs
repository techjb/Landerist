using landerist_library.Database;
using landerist_library.Pages;
using System.Data;

namespace landerist_library.Infrastructure.Sql
{
    public class PageRepository
    {
        public DataRow? GetDataRow(string uriHash)
        {
            string query =
                "SELECT * " +
                "FROM " + Pages.Pages.PAGES + " " +
                "WHERE [UriHash] = @UriHash";

            var dataTable = new DataBase().QueryTable(query, new Dictionary<string, object?> {
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
                "[PageTypeCounter], [ListingStatus], [LockedBy], [WaitingStatus], [ListingParserInputHash], " +
                "[ListingParserInputNotChangedCounter], [TransientErrorCounter], [ResponseBodyZipped], [TokenCount]) " +
                "VALUES(@Host, @Uri, @UriHash, @Inserted, @LastScrape, @LastParseListing, @NextScrape, @HttpStatusCode, @Etag, @LastModified, @PageType, " +
                "@PageTypeCounter, @ListingStatus, @LockedBy, @WaitingStatus, @ListingParserInputHash, " +
                "@ListingParserInputNotChangedCounter, @TransientErrorCounter, CONVERT(varbinary(max), @ResponseBodyZipped), @TokenCount)";

            return new DataBase().Query(query, parameters);
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

            return new DataBase().Query(query, parameters, out exception);
        }

        public bool UpdateNextScrape(string uriHash, DateTime? nextScrape)
        {
            string query =
               "UPDATE " + Pages.Pages.PAGES + " SET " +
               "[NextScrape] = @NextScrape " +
               "WHERE [UriHash] = @UriHash";

            return new DataBase().Query(query, new Dictionary<string, object?> {
                {"UriHash", uriHash },
                {"NextScrape", nextScrape },
            });
        }

        public bool Delete(string uriHash)
        {
            string query =
                "DELETE FROM " + Pages.Pages.PAGES + " " +
                "WHERE [UriHash] = @UriHash";

            return new DataBase().Query(query, new Dictionary<string, object?> {
                {"UriHash", uriHash }
            });
        }
    }
}
