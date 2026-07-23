using landerist_library.Infrastructure.Sql;
using landerist_orels;
using landerist_orels.ES;
using System.Data;

namespace landerist_library.Database
{
    public class ES_Sources
    {
        private static readonly SourceRepository Repository = new(global::landerist_library.Database.LegacyDatabase.Create());

        public static void Insert(Listing listing)
        {
            Repository.Insert(listing);
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
            return Repository.Delete(guid);
        }

        public static bool Delete()
        {
            return Repository.Delete();
        }

        public static SortedSet<Source> GetSources(Listing listing)
        {
            DataTable dataTable = Repository.GetSources(listing);

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

        internal static Source? GetSource(DataRow dataRow)
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
            DataTable dataTable = Repository.GetListingsWithoutSourcePages();
            int total = dataTable.Rows.Count;
            int counter = 0;
            int errors = 0;
            Parallel.ForEach(dataTable.AsEnumerable(), dataRow =>
            {
                Interlocked.Increment(ref counter);
                Console.WriteLine(counter + "/" + total + " Errors: " + errors);
                string guid = dataRow["Uri"].ToString() ?? string.Empty;
                var page = new Pages.Page(guid);
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
