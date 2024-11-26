using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Index;
using landerist_library.Tools;
using System.Data;

namespace landerist_library.Websites
{
    public class Pages
    {
        public const string TABLE_PAGES = "[PAGES]";

        public static Page? GetPage(string uriHash)
        {
            string query =
                SelectQuery() +
                "WHERE [UriHash] = @UriHash";

            DataTable dataTable = new DataBase().QueryTable(query, new Dictionary<string, object?> {
                {"UriHash", uriHash }
            });

            var pages = GetPages(dataTable);
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

            var dataTable = new DataBase().QueryTable(query);
            return GetPages(dataTable);
        }

        public static List<Page> GetPages(Website website)
        {
            string query =
                "SELECT * " +
                "FROM " + TABLE_PAGES + " " +
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

            DataTable dataTable = new DataBase().QueryTable(query, new Dictionary<string, object?> {
                {"PageType", pageType.ToString() }
            });

            return GetPages(dataTable);
        }

        public static List<Page> GetPagesNextUpdatePast()
        {
            string query =
                SelectQuery(true) +
                "WHERE [NextUpdate] < @Now AND [WaitingAIParsing] IS NULL";
            
            query = AddRanquedPages(query);

            DataTable dataTable = new DataBase().QueryTable(query, new Dictionary<string, object?> {
                {"Now", DateTime.Now },
            });

            return GetPages(dataTable);
        }

        public static List<Page> GetPagesNextUpdateFuture()
        {
            string query =
                SelectQuery(true) +
                "WHERE [NextUpdate] >= @Now AND [WaitingAIParsing] IS NULL " +
                "ORDER BY [NextUpdate] ASC";

            query = AddRanquedPages(query);

            DataTable dataTable = new DataBase().QueryTable(query, new Dictionary<string, object?> {
                {"Now", DateTime.Now },
            });

            return GetPages(dataTable);
        }

        public static List<Page> GetNonScrapedPages(Website website)
        {
            string query =
                "SELECT * " +
                "FROM " + TABLE_PAGES + " " +
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
                "FROM " + TABLE_PAGES + " " +
                "WHERE [Host] = @Host AND " +
                "[PageType] IS NULL";

            DataTable dataTable = new DataBase().QueryTable(query, new Dictionary<string, object?> {
                {"Host", website.Host },
            });

            return GetPages(website, dataTable);
        }

        public static List<Page> GetUnknownPageType()
        {
            string query =
                SelectQuery(true) +
                "WHERE [PageType] IS NULL AND [WaitingAIParsing] IS NULL";

            query = AddRanquedPages(query);
            DataTable dataTable = new DataBase().QueryTable(query);
            return GetPages(dataTable);
        }


        public static List<Page> GetUnknowHttpStatusCode()
        {
            string query =
                SelectQuery() +
                "WHERE [HttpStatusCode] IS NULL";

            DataTable dataTable = new DataBase().QueryTable(query);
            return GetPages(dataTable);
        }

        private static string SelectQuery(bool addRowNumber = false)
        {
            string rowNumber = addRowNumber ? 
                ", ROW_NUMBER() OVER (PARTITION BY [PAGES].[Host] ORDER BY [PAGES].[Host]) AS RowNum " : 
                string.Empty;

            return
                "SELECT " +
                TABLE_PAGES + ".[Host], " +
                TABLE_PAGES + ".[Uri], " +
                TABLE_PAGES + ".[UriHash], " +
                TABLE_PAGES + ".[Inserted], " +
                TABLE_PAGES + ".[Updated], " +
                TABLE_PAGES + ".[NextUpdate], " +
                TABLE_PAGES + ".[HttpStatusCode], " +
                TABLE_PAGES + ".[PageType], " +
                TABLE_PAGES + ".[PageTypeCounter], " +
                TABLE_PAGES + ".[WaitingAIParsing], " +
                TABLE_PAGES + ".[ResponseBodyTextHash], " +
                TABLE_PAGES + ".[ResponseBodyZipped], " +                
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
                Websites.WEBSITES + ".[ListingExampleNodeSetUpdated] " +
                rowNumber + 
                "FROM " + TABLE_PAGES + " " +
                "INNER JOIN " + Websites.WEBSITES + 
                " ON " + TABLE_PAGES + ".[Host] = " + Websites.WEBSITES + ".[Host] ";
        }

        private static string AddRanquedPages(string query)
        {
            return
                "WITH RankedPages AS (" + query + ") " +
                "SELECT TOP "+ Config.MAX_PAGES_PER_SCRAPE + " * " +
                "FROM RankedPages AS T " +
                "WHERE RowNum <= " + Config.MAX_PAGES_PER_HOSTS_PER_SCRAPE;
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
               "DELETE FROM " + TABLE_PAGES + " " +
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
               "DELETE FROM " + TABLE_PAGES;

            return new DataBase().Query(query);
        }

        public static void Insert(Website website, Uri uri)
        {
            var page = new Page(website, uri);
            page.Insert();
        }

        public static List<string> GetUris(bool isListing)
        {
            string query =
                "SELECT [Uri] " +
                "FROM " + TABLE_PAGES + " " +
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
                "FROM " + TABLE_PAGES;
            return new DataBase().QueryListString(query);
        }

        public static void DeleteNumPagesExceded()
        {
            string query =
                "SELECT [Host] " +
                "FROM " + TABLE_PAGES + " " +
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
                    if (page.MainPage())
                    {
                        return;
                    }
                    if (pageCounter > Config.MAX_PAGES_PER_WEBSITE)
                    {
                        page.Delete();
                    }
                });
            };
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
                "FROM " + TABLE_PAGES;

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

            DataTable dataTable = new DataBase().QueryTable(query);
            var pages = GetPages(dataTable);
            Delete(pages);
        }

        public static void DeleteListingsResponseBodyRepeated()
        {
            string query =
               SelectQuery() +
               "WHERE [PageType] = 'Listing' AND [ResponseBodyTextHash] IS NOT NULL";

            DataTable dataTable = new DataBase().QueryTable(query);
            var pages = GetPages(dataTable);
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

        public static void Delete(List<Page> pages)
        {
            Console.WriteLine("Deleting " + pages.Count + " pages..");
            int counter = 0;
            Parallel.ForEach(pages, page =>
            {
                if (page.Delete())
                {
                    Interlocked.Increment(ref counter);
                }
            });
            Console.WriteLine("Deleted " + pages.Count + " pages");
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
                var listing = page.GetListing(false);
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
                "UPDATE" + TABLE_PAGES + " " +
                "SET [ResponseBodyTextHash] = NULL " +
                "WHERE [PageType] = @PageType";

            return new DataBase().Query(query, new Dictionary<string, object?> {
                {"PageType", pageType.ToString() }
            });
        }

        public static bool RemoveResponseBodyTextHashToAll()
        {
            string query =
                "UPDATE" + TABLE_PAGES + " " +
                "SET [ResponseBodyTextHash] = NULL";

            return new DataBase().Query(query);
        }

        public static void DeleteUnpublishedListings()
        {
            DateTime unlistingDate = DateTime.Now.AddDays(-Config.DAYS_TO_REMOVE_UMPUBLISHED_LISTINGS);
            var listings = ES_Listings.GetUnpublishedListings(unlistingDate);
            if (listings.Equals(0))
            {
                return;
            }
            int total = listings.Count;
            int processed = 0;
            int deleted = 0;
            int errors = 0;
            Parallel.ForEach(listings,
                new ParallelOptions()
                {
                    //MaxDegreeOfParallelism = Config.MAX_DEGREE_OF_PARALLELISM,
                },
                listing =>
            {
                Interlocked.Increment(ref processed);
                var page = new Page(listing);
                if (page.DeleteListing())
                {
                    Interlocked.Increment(ref deleted);
                }
                else
                {
                    Interlocked.Increment(ref errors);
                }
                Console.WriteLine(processed + "/" + total + " Deleted: " + deleted);

            });
            Logs.Log.WriteInfo("DeleteUnpublishedListings", "Deleted: " + deleted + "/" + total + " Errors: " + errors);
        }

        public static List<Page> SelectWaitingAIParsing()
        {
            string query =
                SelectQuery() +
                "WHERE [WaitingAIParsing] = 1";

            DataTable dataTable = new DataBase().QueryTable(query);
            return GetPages(dataTable);
        }
    }
}
