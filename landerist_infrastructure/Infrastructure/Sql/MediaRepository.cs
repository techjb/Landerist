using landerist_library.Application.Listings;
using landerist_library.Database;
using landerist_orels;
using landerist_orels.ES;
using System.Data;

namespace landerist_library.Infrastructure.Sql
{
    public sealed class MediaRepository : IListingMediaRepository
    {
        private readonly IDatabase _database;
        public MediaRepository(IDatabase database)
        {
            ArgumentNullException.ThrowIfNull(database);
            _database = database;
        }

        private IDatabase Database => _database;

        public void Insert(Listing listing)
        {
            if (listing.media == null)
            {
                return;
            }

            foreach (var media in listing.media)
            {
                string query =
                    "INSERT INTO " + SqlTableNames.Media + " " +
                    "VALUES(@ListingGuid ,@MediaType ,@Title ,@Url)";

                Database.Query(query, new Dictionary<string, object?> {
                    {"listingGuid", listing.guid },
                    {"mediaType", media.mediaType?.ToString() },
                    {"title", media.title },
                    {"url", media.url.ToString()},
                });
            }
        }

        public bool Delete(string guid)
        {
            string query =
                "DELETE FROM " + SqlTableNames.Media + " " +
                "WHERE [listingGuid] = @listingGuid";

            return Database.Query(query, new Dictionary<string, object?>()
            {
                { "listingGuid", guid }
            });
        }

        public bool DeleteAll()
        {
            return Database.Query("DELETE FROM " + SqlTableNames.Media);
        }

        public DataTable GetMedia(Listing listing)
        {
            string query =
                "SELECT * " +
                "FROM " + SqlTableNames.Media + " " +
                "WHERE [listingGuid] = @listingGuid";

            return Database.QueryTable(query, new Dictionary<string, object?>()
            {
                { "listingGuid", listing.guid }
            });
        }
    }
}
