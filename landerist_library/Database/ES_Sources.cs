using landerist_orels;
using landerist_orels.ES;
using System.Data;

namespace landerist_library.Database
{
    public class ES_Sources
    {
        private const string TABLE_ES_SOURCES = "[ES_SOURCES]";

        public static void Insert(Listing listing)
        {
            if (listing.sources == null)
            {
                return;
            }

            foreach (var source in listing.sources)
            {
                string query =
                    "INSERT INTO " + TABLE_ES_SOURCES + " " +
                    "VALUES(@ListingGuid ,@SourceName ,@SourceUrl ,@SourceGuid)";

                new DataBase().Query(query, new Dictionary<string, object?> {
                    {"ListingGuid", listing.guid },
                    {"SourceName", source.sourceName?.ToString() },
                    {"SourceUrl", source.sourceUrl.ToString()},
                    {"SourceGuid", source.sourceGuid?.ToString()},
                });
            }
        }

        public static void Update(Listing listing)
        {
            if (Delete(listing))
            {
                Insert(listing);
            }            
        }

        public static bool Delete(Listing listing)
        {
            return Delete(listing.guid);
        }

        public static bool Delete(string guid)
        {
            string query =
                "DELETE FROM " + TABLE_ES_SOURCES + " " +
                "WHERE [listingGuid] = @listingGuid";

            return new DataBase().Query(query, new Dictionary<string, object?>()
            {
                { "listingGuid", guid }
            });
        }

        public static bool Delete()
        {
            string query =
                "DELETE FROM " + TABLE_ES_SOURCES;

            return new DataBase().Query(query);
        }

        public static SortedSet<Source> GetSources(Listing listing)
        {
            string query =
                "SELECT * " +
                "FROM " + TABLE_ES_SOURCES + " " +
                "WHERE [listingGuid] = @listingGuid";

            DataTable dataTable = new DataBase().QueryTable(query, new Dictionary<string, object?>()
            {
                { "listingGuid", listing.guid }
            });

            SortedSet<Source> sources = new(new SourceComparer());
            foreach (DataRow dataRow in dataTable.Rows)
            {
                var source = GetSource(dataRow);
                if (source != null)
                {
                    sources.Add(source);
                }
            }
            return sources;
        }

        private static Source? GetSource(DataRow dataRow)
        {

            var sourceName = dataRow["sourceName"] is DBNull ? null : (string)dataRow["sourceName"];
            if (!Uri.TryCreate((string)dataRow["sourceUrl"], UriKind.Absolute, out Uri? uri))
            {
                return null;
            }
            var sourceGuid = dataRow["sourceGuid"] is DBNull ? null : (string)dataRow["sourceGuid"];

            return new Source()
            {
                sourceName = sourceName,
                sourceUrl = uri,
                sourceGuid = sourceGuid,
            };
        }

        public static void FixListingsWhitoutSource()
        {
            string query =
                "SELECT * FROM PAGES " +
                "WHERE UriHash in (  " +
                "   SELECT guid FROM [Landerist].[dbo].[ES_LISTINGS]  " +
                "   WHERE guid NOT IN (SELECT listingGuid FROM ES_SOURCES)  " +
                ")";
            DataTable dataTable = new DataBase().QueryTable(query);
            int total = dataTable.Rows.Count;
            int counter = 0;
            int errors = 0;
            Parallel.ForEach(dataTable.AsEnumerable(), dataRow =>
            {
                Interlocked.Increment(ref counter);
                Console.WriteLine(counter + "/" + total + " Errors: " + errors);
                string guid = dataRow["Uri"].ToString() ?? string.Empty;
                var page = new Websites.Page(guid);
                Listing? listing = ES_Listings.GetListing(page, true, true);
                if (listing == null)
                {
                    Interlocked.Increment(ref errors);
                    return;
                }
                var source = new Source
                {
                    sourceGuid = "",
                    sourceUrl = page.Uri,
                    sourceName = page.Website.Host,
                };
                listing.sources.Add(source);
                ES_Listings.InsertUpdate(page.Website, listing);
            });
        }
    }
}
