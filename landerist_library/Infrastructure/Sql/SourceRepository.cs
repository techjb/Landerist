using landerist_library.Database;
using landerist_orels.ES;
using System.Data;

namespace landerist_library.Infrastructure.Sql
{
    public class SourceRepository
    {
        private readonly IDatabase _database;

        public SourceRepository() : this(new DataBase())
        {
        }

        public SourceRepository(IDatabase database)
        {
            ArgumentNullException.ThrowIfNull(database);
            _database = database;
        }

        private IDatabase Database => _database;

        private const string TableEsSources = "[ES_SOURCES]";

        public void Insert(Listing listing)
        {
            if (listing.sources == null)
            {
                return;
            }

            foreach (var source in listing.sources)
            {
                string query =
                    "INSERT INTO " + TableEsSources + " " +
                    "VALUES(@ListingGuid ,@SourceName ,@SourceUrl ,@SourceGuid)";

                Database.Query(query, new Dictionary<string, object?> {
                    {"ListingGuid", listing.guid },
                    {"SourceName", source.sourceName?.ToString() },
                    {"SourceUrl", source.sourceUrl.ToString()},
                    {"SourceGuid", source.sourceGuid?.ToString()},
                });
            }
        }

        public bool Delete(string guid)
        {
            string query =
                "DELETE FROM " + TableEsSources + " " +
                "WHERE [listingGuid] = @listingGuid";

            return Database.Query(query, new Dictionary<string, object?>()
            {
                { "listingGuid", guid }
            });
        }

        public bool Delete()
        {
            return Database.Query("DELETE FROM " + TableEsSources);
        }

        public DataTable GetSources(Listing listing)
        {
            string query =
                "SELECT * " +
                "FROM " + TableEsSources + " " +
                "WHERE [listingGuid] = @listingGuid";

            return Database.QueryTable(query, new Dictionary<string, object?>()
            {
                { "listingGuid", listing.guid }
            });
        }

        public DataTable GetListingsWithoutSourcePages()
        {
            string query =
                "SELECT * FROM PAGES " +
                "WHERE UriHash in (  " +
                "   SELECT guid FROM [Landerist].[dbo].[ES_LISTINGS]  " +
                "   WHERE guid NOT IN (SELECT listingGuid FROM ES_SOURCES)  " +
                ")";

            return Database.QueryTable(query);
        }
    }
}
