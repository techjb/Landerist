using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Index;
using landerist_library.Tools;
using landerist_library.Websites;
using landerist_orels.ES;
using System.Collections.Concurrent;
using System.Data;

namespace landerist_library.Pages
{
    public partial class Pages
    {
        public static bool Delete(Website website)
        {
            string query =
               "DELETE FROM " + PAGES + " " +
               "WHERE [Host] = @Host";

            bool sucess = new DataBase().Query(query, new Dictionary<string, object?> {
                {"Host", website.Host }
            });
            return sucess;
        }

        public static bool DeleteAll()
        {
            string query =
               "DELETE FROM " + PAGES;

            return new DataBase().Query(query);
        }

       
        public static void Delete(PageType pageType)
        {
            var pages = GetPages(pageType);
            Delete(pages);
        }

        public static void DeleteDuplicateUriQuery()
        {
            string query =
                "SELECT [Uri] " +
                "FROM " + PAGES;

            DataTable dataTable = new DataBase().QueryTable(query);
            int counter = 0;
            int total = dataTable.Rows.Count;
            var pages = new ConcurrentBag<Page>();

            Parallel.ForEach(dataTable.AsEnumerable(), dataRow =>
            {
                string uriString = dataRow["Uri"].ToString()!;
                var uri = new Uri(uriString);
                var newUri = Uris.CleanUri(uri);
                int current = Interlocked.Increment(ref counter);

                if (newUri != uri)
                {
                    Page page = new(uri);
                    new Indexer(page).Insert(page.Uri);
                    pages.Add(page);
                }

                Console.WriteLine(current + "/" + total);
            });

            Delete([.. pages]);
        }

        public static void DeleteListingsHttpStatusCodeError()
        {
            string query =
               SelectQuery() +
               "WHERE [PageType] = 'Listing' and [HttpStatusCode] <> 200";

            var pages = GetPages(query);
            Delete(pages);
        }

        public static void DeleteListingsResponseBodyRepeated()
        {
            string query =
               SelectQuery() +
               "WHERE [PageType] = 'Listing' AND [ListingParserInputHash] IS NOT NULL";

            var pages = GetPages(query);
            HashSet<string> hashSet = [];
            List<Page> repeated = [];
            foreach (var page in pages)
            {
                if (page.ListingParserInputHash == null)
                {
                    continue;
                }
                if (!hashSet.Add(page.ListingParserInputHash))
                {
                    repeated.Add(page);
                }
            }
            Delete(repeated);
        }

        public static void DeleteUrisLikePrint()
        {
            string query =
               SelectQuery() +
               "WHERE " +
               "    Uri like '%print%' OR " +
               "    Uri like '%imprimi%' ";

            var pages = GetPages(query);
            Delete(pages);
        }

        // Also removes url with prohibited host
        public static void DeleteProhibitedUris()
        {
            string where = string.Join(" OR ", ProhibitedUrls.Prohibited_ES.Select(uri => $"Uri LIKE '%{uri}%'"));
            string query =
               SelectQuery() +
               "WHERE " + where;

            var pages = GetPages(query);
            Delete(pages);
        }

        public static void Delete(List<Page> pages)
        {
            Console.WriteLine("Deleting " + pages.Count + " pages..");
            int counter = 0;
            int errors = 0;
            int total = pages.Count;
            Parallel.ForEach(pages,
                //new ParallelOptions(){MaxDegreeOfParallelism = 1}, 
                page =>
            {
                Console.WriteLine(page.Uri);
                if (page.Delete())
                {
                    Interlocked.Increment(ref counter);
                }
                else
                {
                    Interlocked.Increment(ref errors);
                }
                Console.WriteLine($"Deleted {counter}/{total} Errors: {errors}");
            });
        }

        public static void DeleteUnpublishedListings()
        {
            DateTime unlistingDate = DateTime.Now.AddDays(-Config.DAYS_TO_REMOVE_UMPUBLISHED_LISTINGS);
            var listings = ES_Listings.GetUnpublishedListings(unlistingDate);
            DeleteListings(listings);
        }

        private static void DeleteListings(SortedSet<Listing> listings)
        {
            int counter = 0;
            int deleted = 0;
            int errors = 0;
            Parallel.ForEach(listings,
                new ParallelOptions()
                {
                    //MaxDegreeOfParallelism = Config.MAX_DEGREE_OF_PARALLELISM,
                },
                listing =>
                {
                    Interlocked.Increment(ref counter);
                    foreach (var source in listing.sources)
                    {
                        var page = new Page(source.sourceUrl);
                        if (page.DeleteListing())
                        {
                            Interlocked.Increment(ref deleted);
                        }
                        else
                        {
                            Interlocked.Increment(ref errors);
                        }
                    }

                    Console.WriteLine(counter + "/" + listings.Count + " Deleted: " + deleted);
                });
        }
    }
}
