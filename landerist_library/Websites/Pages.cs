using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Index;
using landerist_library.Tools;
using landerist_orels.ES;
using System.Data;

namespace landerist_library.Websites
{
    public class Pages
    {
        public const string PAGES = "[PAGES]";

        public static Page? GetPage(string uriHash)
        {
            string query =
                SelectQuery() +
                "WHERE [UriHash] = @UriHash";


            var pages = GetPages(query, new Dictionary<string, object?> {
                {"UriHash", uriHash }
            });
            if (pages.Count.Equals(1))
            {
                return pages[0];
            }
            return null;
        }
        public static List<Page> GetPages()
        {
            Console.WriteLine("Reading all pages");
            string query = SelectQuery();
            return GetPages(query);
        }

        public static List<Page> GetPages(Website website)
        {
            string query =
                "SELECT * " +
                "FROM " + PAGES + " " +
                "WHERE [Host] = @Host";

            DataTable dataTable = new DataBase().QueryTable(query, new Dictionary<string, object?> {
                {"Host", website.Host }
            });

            return GetPages(website, dataTable);
        }

        public static List<Page> GetPages(PageType pageType)
        {
            string query =
                SelectQuery() +
                "WHERE [PageType] = @PageType";

            return GetPages(query, new Dictionary<string, object?> {
                {"PageType", pageType.ToString() }
            });
        }

        public static List<Page> GetUnknownPageType()
        {
            string query =
                SelectQuery() +
                "WHERE [PageType] IS NULL AND [WaitingStatus] IS NULL ";

            return GetPages(query);
        }

        public static List<Page> GetUnknownPageType(int topRows)
        {
            string where = "P.[PageType] IS NULL";
            return GetPages(topRows, where);
        }

        public static List<Page> GetNextUpdate(int topRows, bool extendToFillTopRows)
        {
            string where = extendToFillTopRows ? string.Empty : "P.[NextUpdate] < GETDATE()";
            return GetPages(topRows, where);
        }

        public static List<Page> GetNextUpdateFuture(int topRows)
        {
            string where = "P.[NextUpdate] >= GETDATE()";
            return GetPages(topRows, where);
        }

        public static List<Page> GetUnpublishedPages(int topRows)
        {
            string where =
                "P.[UriHash] IN (" +
                "   SELECT [Guid] FROM " + ES_Listings.TABLE_ES_LISTINGS + " " +
                "   WHERE [ListingStatus] = 'unpublished' AND [UnlistingDate]>DATEADD(day, -2, getdate())" +
                ")";
            return GetPages(topRows, where);
        }

        private static List<Page> GetPages(int topRows, string where)
        {
            string query =
                "WITH TopPages AS (" +
                "   SELECT TOP " + topRows + " P.[UriHash] " +
                "   FROM " + PAGES + " AS P " +
                "   INNER JOIN " + Websites.WEBSITES + " AS W ON P.[Host] = W.[Host] " +
                "   WHERE P.[LockedBy] IS NULL AND P.[WaitingStatus] IS NULL " + (string.IsNullOrEmpty(where) ? string.Empty : " AND " + where) + " " +
                "   ORDER BY P.[NextUpdate] ASC" +
                ") " +
                "UPDATE P " +
                "SET LockedBy = @LockedBy " +
                "OUTPUT " +
                    "INSERTED.[Host], " +
                    "INSERTED.[Uri], " +
                    "INSERTED.[UriHash], " +
                    "INSERTED.[Inserted], " +
                    "INSERTED.[Updated], " +
                    "INSERTED.[NextUpdate], " +
                    "INSERTED.[HttpStatusCode], " +
                    "INSERTED.[PageType], " +
                    "INSERTED.[PageTypeCounter], " +
                    "INSERTED.[ListingStatus], " +
                    "INSERTED.[LockedBy], " +
                    "INSERTED.[WaitingStatus], " +
                    "INSERTED.[ResponseBodyTextHash], " +
                    "INSERTED.[ResponseBodyZipped], " +
                    "W.[MainUri], " +
                    "W.[LanguageCode], " +
                    "W.[CountryCode], " +
                    "W.[RobotsTxt], " +
                    "W.[RobotsTxtUpdated], " +
                    "W.[SitemapUpdated], " +
                    "W.[IpAddress], " +
                    "W.[IpAddressUpdated], " +
                    "W.[NumPages], " +
                    "W.[NumListings], " +
                    "W.[ListingExampleUri], " +
                    "W.[ListingExampleNodeSet], " +
                    "W.[ListingExampleNodeSetUpdated] " +
                "FROM " + PAGES + " AS P " +
                "INNER JOIN " + Websites.WEBSITES + " AS W ON P.[Host] = W.[Host] " +
                "INNER JOIN TopPages AS TP ON P.[UriHash] = TP.[UriHash]";

            DataTable dataTable = new DataBase().QueryTable(query, new Dictionary<string, object?>(){
                { "LockedBy", Config.IsConfigurationLocal()? null: Config.MACHINE_NAME}
            });
            return GetPages(dataTable);
        }

        public static List<Page> GetNonScrapedPages(Website website)
        {
            string query =
                "SELECT * " +
                "FROM " + PAGES + " " +
                "WHERE [Host] = @Host AND " +
                "[Updated] IS NULL";

            DataTable dataTable = new DataBase().QueryTable(query, new Dictionary<string, object?> {
                {"Host", website.Host }
            });

            return GetPages(website, dataTable);
        }

        public static List<Page> GetUnknowPageType(Website website)
        {
            string query =
                "SELECT * " +
                "FROM " + PAGES + " " +
                "WHERE [Host] = @Host AND " +
                "[PageType] IS NULL";

            DataTable dataTable = new DataBase().QueryTable(query, new Dictionary<string, object?> {
                {"Host", website.Host },
            });

            return GetPages(website, dataTable);
        }

        public static List<Page> GetUnknowHttpStatusCode()
        {
            string query =
                SelectQuery() +
                "WHERE [HttpStatusCode] IS NULL";

            DataTable dataTable = new DataBase().QueryTable(query);
            return GetPages(dataTable);
        }

        public static Dictionary<string, object?> GroupByPageType(ListingStatus? listingStatus = null)
        {
            string where = listingStatus != null
                ? "WHERE [listingStatus] = @listingStatus "
                : string.Empty;
            string query =
                "SELECT [PageType], COUNT(*) " +
                "FROM " + PAGES + " " +
                where + " " +
                "GROUP BY [PageType] " +
                "ORDER BY COUNT(*) DESC";

            return new DataBase().QueryDictionary(query, new Dictionary<string, object?>
            {
                { "listingStatus", listingStatus.ToString() }
            });
        }

        public static Dictionary<string, object?> GroupByHttpStatusCode(ListingStatus? listingStatus = null)
        {
            string where = listingStatus != null
                ? "WHERE [listingStatus] = @listingStatus "
                : string.Empty;
            string query =
                "SELECT CONVERT(VARCHAR,  [HttpStatusCode], 23), COUNT(*) " +
                "FROM " + PAGES + " " +
                where + " " +
                "GROUP BY CONVERT(VARCHAR,  [HttpStatusCode], 23) " +
                "ORDER BY COUNT(*) DESC";

            return new DataBase().QueryDictionary(query, new Dictionary<string, object?>
            {
                { "listingStatus", listingStatus.ToString() }
            });
        }

        public static Dictionary<string, object?> GroupByNextUpdate()
        {
            string query =
                "SELECT  CONVERT(VARCHAR, [NextUpdate], 23) AS [DateWhithoutTime], COUNT(*) AS [Total] " +
                "FROM " + PAGES + " " +
                "GROUP BY CONVERT(VARCHAR, [NextUpdate], 23) " +
                "ORDER BY [DateWhithoutTime] ASC";

            return new DataBase().QueryDictionary(query);
        }

        public static Dictionary<string, object?> CountByHttpStatusCode()
        {
            string query =
                "SELECT CAST([HttpStatusCode] AS VARCHAR), COUNT(*) " +
                "FROM " + PAGES + " " +
                "GROUP BY [HttpStatusCode] " +
                "ORDER BY COUNT(*) DESC";

            return new DataBase().QueryDictionary(query);
        }

        private static string SelectQuery(int? topRows = null)
        {
            string top = topRows != null ? "TOP " + topRows : "";
            return
                "SELECT " + top + " " +
                SelectColumns() + " " +
                "FROM " + PAGES + " " +
                "INNER JOIN " + Websites.WEBSITES +
                " ON " + PAGES + ".[Host] = " + Websites.WEBSITES + ".[Host] ";
        }

        private static string SelectColumns()
        {
            return
                PAGES + ".[Host], " +
                PAGES + ".[Uri], " +
                PAGES + ".[UriHash], " +
                PAGES + ".[Inserted], " +
                PAGES + ".[Updated], " +
                PAGES + ".[NextUpdate], " +
                PAGES + ".[HttpStatusCode], " +
                PAGES + ".[PageType], " +
                PAGES + ".[PageTypeCounter], " +
                PAGES + ".[ListingStatus], " +
                PAGES + ".[LockedBy], " +
                PAGES + ".[WaitingStatus], " +
                PAGES + ".[ResponseBodyTextHash], " +
                PAGES + ".[ResponseBodyZipped], " +
                Websites.WEBSITES + ".[MainUri], " +
                Websites.WEBSITES + ".[LanguageCode], " +
                Websites.WEBSITES + ".[CountryCode], " +
                Websites.WEBSITES + ".[RobotsTxt], " +
                Websites.WEBSITES + ".[RobotsTxtUpdated], " +
                Websites.WEBSITES + ".[SitemapUpdated], " +
                Websites.WEBSITES + ".[IpAddress], " +
                Websites.WEBSITES + ".[IpAddressUpdated], " +
                Websites.WEBSITES + ".[NumPages], " +
                Websites.WEBSITES + ".[NumListings], " +
                Websites.WEBSITES + ".[ListingExampleUri], " +
                Websites.WEBSITES + ".[ListingExampleNodeSet], " +
                Websites.WEBSITES + ".[ListingExampleNodeSetUpdated] ";
        }

        public static List<Page> GetPages(string query)
        {
            return GetPages(query, []);
        }

        public static List<Page> GetPages(string query, Dictionary<string, object?> dictionary)
        {
            DataTable dataTable = new DataBase().QueryTable(query, dictionary);
            return GetPages(dataTable);
        }

        private static List<Page> GetPages(DataTable dataTable)
        {
            List<Page> pages = [];
            foreach (DataRow dataRow in dataTable.Rows)
            {
                Website website = new(dataRow);
                Page page = new(website, dataRow);
                pages.Add(page);
            }
            return pages;
        }

        private static List<Page> GetPages(Website website, DataTable dataTable)
        {
            List<Page> pages = [];
            foreach (DataRow dataRow in dataTable.Rows)
            {
                Page page = new(website, dataRow);
                pages.Add(page);
            }
            return pages;
        }

        public static bool Delete(Website website)
        {
            string query =
               "DELETE FROM " + PAGES + " " +
               "WHERE [Host] = @Host";

            bool sucess = new DataBase().Query(query, new Dictionary<string, object?> {
                {"Host", website.Host }
            });
            if (sucess)
            {
                website.SetNumPagesToZero();
            }
            return sucess;
        }

        public static bool DeleteAll()
        {
            string query =
               "DELETE FROM " + PAGES;

            return new DataBase().Query(query);
        }

        public static bool Insert(Website website, Uri uri)
        {
            var page = new Page(website, uri);
            return page.Insert();
        }

        public static List<string> GetUris(bool isListing)
        {
            string query =
                "SELECT [Uri] " +
                "FROM " + PAGES + " " +
                "WHERE IsListing = @IsListing";
            return new DataBase().QueryListString(query, new Dictionary<string, object?>()
            {
                { "IsListing", isListing }
            });
        }

        public static List<string> GetUris()
        {
            string query =
                "SELECT [Uri] " +
                "FROM " + PAGES;
            return new DataBase().QueryListString(query);
        }

        public static void DeleteNumPagesExceded()
        {
            string query =
                "SELECT [Host] " +
                "FROM " + PAGES + " " +
                "GROUP BY [Host] " +
                "HAVING COUNT(*) > " + Config.MAX_PAGES_PER_WEBSITE;

            DataTable dataTable = new DataBase().QueryTable(query);
            int total = dataTable.Rows.Count;
            int counter = 0;
            foreach (DataRow dataRow in dataTable.Rows)
            {
                Console.WriteLine(counter++ + "/" + total);
                string host = (string)dataRow[0];
                Website website = new(host);
                var pages = website.GetPages();
                int pageCounter = 0;

                Parallel.ForEach(pages, page =>
                {
                    Interlocked.Increment(ref pageCounter);
                    if (page.IsMainPage())
                    {
                        return;
                    }
                    if (pageCounter > Config.MAX_PAGES_PER_WEBSITE)
                    {
                        page.Delete();
                    }
                });
            }
            ;
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
            List<Page> pages = [];
            Parallel.ForEach(dataTable.AsEnumerable(), dataRow =>
            {
                string uriString = dataRow["Uri"].ToString()!;
                var uri = new Uri(uriString);
                var newUri = Uris.CleanUri(uri);
                if (newUri != uri)
                {
                    Page page = new(uri);
                    var newPage = new Page(newUri);
                    new Indexer(page).Insert(page.Uri);
                    pages.Add(page);
                    Console.WriteLine(counter + "/" + total);
                    Interlocked.Increment(ref counter);
                }
            });
            Delete(pages);
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
               "WHERE [PageType] = 'Listing' AND [ResponseBodyTextHash] IS NOT NULL";

            var pages = GetPages(query);
            HashSet<string> hashSet = [];
            List<Page> repeated = [];
            foreach (var page in pages)
            {
                if (page.ResponseBodyTextHash == null)
                {
                    continue;
                }
                if (!hashSet.Add(page.ResponseBodyTextHash))
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

        public static void UpdateInvalidCadastastralReferences()
        {
            var pages = GetPages();
            int total = pages.Count;
            int updated = 0;
            int counter = 0;

            foreach (var page in pages)
            {
                Console.WriteLine(counter++ + "/" + total);
                var listing = page.GetListing(false, false);
                if (listing != null && listing.cadastralReference != null)
                {
                    if (!Validate.CadastralReference(listing.cadastralReference))
                    {
                        listing.cadastralReference = null;
                        updated++;
                        if (ES_Listings.Update(listing))
                        {
                            Console.WriteLine("UPDATED: " + updated++);
                        }
                    }
                }
            }
            Console.WriteLine(updated + "/" + total);
        }

        public static void UpdateNextUpdate()
        {
            var pages = GetPages();
            int total = pages.Count;
            int updated = 0;
            int counter = 0;
            int errors = 0;

            Parallel.ForEach(pages, page =>
            {
                Interlocked.Increment(ref counter);
                page.SetNextUpdate();
                if (page.UpdateNextUpdate())
                {
                    Interlocked.Increment(ref updated);
                }
                else
                {
                    Interlocked.Increment(ref errors);
                }
                Console.WriteLine(counter + "/" + total + " updated: " + updated + " errors: " + errors);
            });
            Console.WriteLine(counter + "/" + total + " updated: " + updated + " errors: " + errors);
        }

        public static bool RemoveResponseBodyTextHash(PageType pageType)
        {
            string query =
                "UPDATE" + PAGES + " " +
                "SET [ResponseBodyTextHash] = NULL " +
                "WHERE [PageType] = @PageType";

            return new DataBase().Query(query, new Dictionary<string, object?> {
                {"PageType", pageType.ToString() }
            });
        }

        public static bool RemoveResponseBodyTextHashToAll()
        {
            string query =
                "UPDATE" + PAGES + " " +
                "SET [ResponseBodyTextHash] = NULL";

            return new DataBase().Query(query);
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

        public static List<Page> SelectWaitingStatusAiRequest(int topRows)
        {
            return SelectWaitingStatus(topRows, WaitingStatus.waiting_ai_request);
        }

        private static List<Page> SelectWaitingStatus(int topRows, WaitingStatus waitingStatus)
        {
            string query =
                SelectQuery(topRows) +
                "WHERE [WaitingStatus] = @WaitingStatus";

            DataTable dataTable = new DataBase().QueryTable(query, new Dictionary<string, object?> {
                {"WaitingStatus", waitingStatus.ToString() },
            });
            return GetPages(dataTable);
        }

        public static bool UpdateWaitingStatusAiResponse(string uriHash)
        {
            return UpdateWaitingStatus(uriHash, WaitingStatus.waiting_ai_response);
        }

        public static bool UpdateWaitingStatus(string uriHash, WaitingStatus waitingStatus)
        {
            string query =
                "UPDATE " + PAGES + " " +
                "SET [WaitingStatus] = @WaitingStatus " +
                "WHERE [UriHash] = @UriHash";

            return new DataBase().Query(query, new Dictionary<string, object?> {
                {"WaitingStatus", waitingStatus.ToString() },
                {"UriHash", uriHash }
            });
        }

        public static void CleanLockedBy()
        {
            string query =
                "UPDATE " + PAGES + " " +
                "SET [LockedBy] = NULL " +
                "WHERE [LockedBy] = @LockedBy";

            new DataBase().Query(query, new Dictionary<string, object?>()
            {
                { "LockedBy", Config.MACHINE_NAME }
            });
        }
    }
}
