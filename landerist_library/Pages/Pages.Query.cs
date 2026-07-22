using landerist_library.Infrastructure.Sql;
using landerist_library.Websites;
using System.Data;

namespace landerist_library.Pages
{
    public partial class Pages
    {
        private static readonly PageQueryRepository QueryRepository = new();

        public static Page? GetPage(string uriHash)
        {
            var pages = GetPages(QueryRepository.GetPageByUriHash(uriHash));
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
            return GetPages(QueryRepository.GetPagesByPageType(pageType));
        }

        public static List<Page> GetUnknownPageType()
        {
            return GetPages(QueryRepository.GetUnknownPageType());
        }

        public static List<Page> GetUnknownPageType(int topRows)
        {
            return GetPages(QueryRepository.GetUnknownPageTypeForUpdate(topRows));
        }

        public static List<Page> GetNextScrape(int topRows, bool extendToFillTopRows)
        {
            return GetPages(QueryRepository.GetNextScrapeForUpdate(topRows, extendToFillTopRows));
        }

        public static List<Page> GetNextScrapeFuture(int topRows)
        {
            return GetPages(QueryRepository.GetNextScrapeFutureForUpdate(topRows));
        }

        public static List<Page> GetRecentlyUnpublishedListingsPages(int topRows)
        {
            return GetPages(QueryRepository.GetRecentlyUnpublishedListingsPages(topRows));
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
            return GetPages(QueryRepository.GetUnknownHttpStatusCode());
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
