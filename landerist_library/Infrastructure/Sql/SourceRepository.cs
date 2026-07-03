using landerist_library.Database;
using landerist_orels.ES;
using System.Data;

namespace landerist_library.Infrastructure.Sql
{
    public class SourceRepository
    {
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

                new DataBase().Query(query, new Dictionary<string, object?> {
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

            return new DataBase().Query(query, new Dictionary<string, object?>()
            {
                { "listingGuid", guid }
            });
        }

        public bool Delete()
        {
            return new DataBase().Query("DELETE FROM " + TableEsSources);
        }

        public DataTable GetSources(Listing listing)
        {
            string query =
                "SELECT * " +
                "FROM " + TableEsSources + " " +
                "WHERE [listingGuid] = @listingGuid";

            return new DataBase().QueryTable(query, new Dictionary<string, object?>()
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

            return new DataBase().QueryTable(query);
        }
    }
}
