using landerist_library.Infrastructure.Sql;
using landerist_library.Websites;
using landerist_orels.ES;
using System.Data;

namespace landerist_library.Pages
{
    public partial class Pages
    {
        private static readonly PageQueryRepository QueryRepository = new();

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
            List<Page> pages = [];
            int batchNumber = 0;

            foreach (var batch in GetPageBatches())
            {
                batchNumber++;
                pages.AddRange(batch);
                Console.WriteLine("Read batch " + batchNumber + ": " + batch.Count + " pages. Total: " + pages.Count);
            }

            return pages;
        }

        public static List<Page> GetPages(Website website)
        {
            DataTable dataTable = QueryRepository.GetPagesByHost(website.Host);
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

        public static List<Page> GetNextScrape(int topRows, bool extendToFillTopRows)
        {
            string where = extendToFillTopRows ? string.Empty : "P.[NextScrape] < GETDATE()";
            return GetPages(topRows, where);
        }

        public static List<Page> GetNextScrapeFuture(int topRows)
        {
            string where = "P.[NextScrape] >= GETDATE()";
            return GetPages(topRows, where);
        }

        public static List<Page> GetRecentlyUnpublishedListingsPages(int topRows)
        {
            string where =
                "P.[UriHash] IN (" +
                "   SELECT [Guid] FROM " + Database.ES_Listings.TABLE_ES_LISTINGS + " " +
                "   WHERE [ListingStatus] = 'unpublished' AND [UnlistingDate] > DATEADD(day, -2, getdate())" +
                ")";
            return GetPages(topRows, where);
        }

        public static List<Page> GetScrapePages(int topRows)
        {
            DataTable dataTable = QueryRepository.GetScrapePages(topRows);
            return GetPages(dataTable);
        }

        public static List<Page> GetNonScrapedPages(Website website)
        {
            DataTable dataTable = QueryRepository.GetNonScrapedPages(website.Host);
            return GetPages(website, dataTable);
        }

        public static List<Page> GetUnknowPageType(Website website)
        {
            DataTable dataTable = QueryRepository.GetUnknownPageType(website.Host);
            return GetPages(website, dataTable);
        }

        public static List<Page> GetUnknowHttpStatusCode()
        {
            string query =
                SelectQuery() +
                "WHERE [HttpStatusCode] IS NULL";

            DataTable dataTable = QueryRepository.QueryPages(query);
            return GetPages(dataTable);
        }

        public static List<string> GetUris(bool isListing)
        {
            return QueryRepository.GetUris(isListing);
        }

        public static List<string> GetUris()
        {
            return QueryRepository.GetUris();
        }

        public static DataTable GetHostPagesDataTable(Website website)
        {
            return QueryRepository.GetHostPagesDataTable(website.Host);
        }

        private static List<Page> GetPages(int topRows, string where)
        {
            DataTable dataTable = QueryRepository.GetPagesForUpdate(topRows, where);
            return GetPages(dataTable);
        }

        private static string SelectQuery(int? topRows = null)
        {
            return QueryRepository.SelectQuery(topRows);
        }

        private static int CountPages()
        {
            return QueryRepository.CountPages();
        }

        private static IEnumerable<List<Page>> GetPageBatches(int batchSize = GET_ALL_PAGES_BATCH_SIZE)
        {
            string? lastUriHash = null;

            while (true)
            {
                var batch = GetPagesBatch(lastUriHash, batchSize);
                if (batch.Count == 0)
                {
                    yield break;
                }

                yield return batch;

                if (batch.Count < batchSize)
                {
                    yield break;
                }

                lastUriHash = batch[^1].UriHash;
            }
        }

        private static List<Page> GetPagesBatch(string? lastUriHash, int batchSize)
        {
            DataTable dataTable = QueryRepository.GetPagesBatch(lastUriHash, batchSize);
            return GetPages(dataTable);
        }

        private static string SelectColumns(string pagesTableName = "")
        {
            return PageQueryRepository.SelectColumns(pagesTableName);
        }

        public static List<Page> GetPages(string query)
        {
            return GetPages(query, []);
        }

        public static List<Page> GetPages(string query, Dictionary<string, object?> dictionary)
        {
            DataTable dataTable = QueryRepository.QueryPages(query, dictionary);
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
    }
}
