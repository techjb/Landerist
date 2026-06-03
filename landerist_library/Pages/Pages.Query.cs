using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Websites;
using landerist_orels.ES;
using System.Data;

namespace landerist_library.Pages
{
    public partial class Pages
    {
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
                "   SELECT [Guid] FROM " + ES_Listings.TABLE_ES_LISTINGS + " " +
                "   WHERE [ListingStatus] = 'unpublished' AND [UnlistingDate] > DATEADD(day, -2, getdate())" +
                ")";
            return GetPages(topRows, where);
        }

        public static List<Page> GetScrapePages(int topRows)
        {
            string query =
                "WITH CandidatePages AS (" +
                "   SELECT " +
                "       P.[UriHash], " +
                "       P.[NextScrape], " +
                "       CASE WHEN P.[PageType] IS NULL THEN 0 ELSE 1 END AS SelectionPriority, " +
                "       ROW_NUMBER() OVER (" +
                "           PARTITION BY P.[Host] " +
                "           ORDER BY CASE WHEN P.[PageType] IS NULL THEN 0 ELSE 1 END ASC, P.[NextScrape] ASC, P.[UriHash] ASC" +
                "       ) AS HostPageRank " +
                "   FROM " + PAGES + " AS P " +
                "   INNER JOIN " + Websites.Websites.WEBSITES + " AS W ON P.[Host] = W.[Host] " +
                "   WHERE P.[LockedBy] IS NULL AND P.[WaitingStatus] IS NULL " +
                "   AND (P.[PageType] IS NULL OR P.[NextScrape] < GETDATE()) " +
                "   AND NOT EXISTS (" +
                "       SELECT 1 " +
                "       FROM " + WebsitesThrottle.WEBSITES_THROTTLE + " AS WB " +
                "       WHERE WB.[IpOrHost] = P.[Host] AND WB.[BlockUntil] > GETDATE()" +
                "   ) " +
                "), " +
                "TopPages AS (" +
                "   SELECT TOP " + topRows + " [UriHash] " +
                "   FROM CandidatePages " +
                "   WHERE HostPageRank <= @MaxPagesPerHost " +
                "   ORDER BY SelectionPriority ASC, [NextScrape] ASC, [UriHash] ASC" +
                ") " +
                "UPDATE P " +
                "SET LockedBy = @LockedBy " +
                "OUTPUT " +
                    "INSERTED.[Host], " +
                    "INSERTED.[Uri], " +
                    "INSERTED.[UriHash], " +
                    "INSERTED.[Inserted], " +
                    "INSERTED.[LastScrape], " +
                    "INSERTED.[NextScrape], " +
                    "INSERTED.[HttpStatusCode], " +
                    "INSERTED.[Etag], " +
                    "INSERTED.[LastModified], " +
                    "INSERTED.[PageType], " +
                    "INSERTED.[PageTypeCounter], " +
                    "INSERTED.[ListingStatus], " +
                    "INSERTED.[LockedBy], " +
                    "INSERTED.[WaitingStatus], " +
                    "INSERTED.[ListingParserInputHash], " +
                    "INSERTED.[ListingParserInputNotChangedCounter], " +
                    "INSERTED.[TransientErrorCounter], " +
                    "INSERTED.[ResponseBodyZipped], " +
                    "INSERTED.[TokenCount], " +
                    "W.[MainUri], " +
                    "W.[LanguageCode], " +
                    "W.[CountryCode], " +
                    "W.[RobotsTxt], " +
                    "W.[RobotsTxtUpdated], " +
                    "W.[SitemapUpdated], " +
                    "W.[IpAddress], " +
                    "W.[IpAddressUpdated], " +
                    "W.[IndexUrlRegex], " +
                    "W.[SitemapUrlRegex], " +
                    "W.[ListingUrlRegex]," +
                    "W.[ListingHtmlRemoveXPath], " +
                    "W.[NavigationWaitSelector], " +
                    "W.[AllowedResourceTypes], " +
                    "W.[BlockedDomains], " +
                    "W.[UserAgent], " +
                    "W.[HttpRequestHeaders], " +
                    "W.[ApplySpecialRules], " +
                    "W.[HtmlIndexingEnabled], " +
                    "W.[UseProxy], " +
                    "W.[MinimumRequestIntervalMilliseconds] " +
                "FROM " + PAGES + " AS P " +
                "INNER JOIN " + Websites.Websites.WEBSITES + " AS W ON P.[Host] = W.[Host] " +
                "INNER JOIN TopPages AS TP ON P.[UriHash] = TP.[UriHash]";

            DataTable dataTable = new DataBase().QueryTable(query, new Dictionary<string, object?>(){
                { "LockedBy", Config.IsConfigurationLocal()? null: Config.MACHINE_NAME},
                { "MaxPagesPerHost", Config.MAX_PAGES_PER_HOST_PER_SCRAPE}
            });
            return GetPages(dataTable);
        }

        public static List<Page> GetNonScrapedPages(Website website)
        {
            string query =
                "SELECT * " +
                "FROM " + PAGES + " " +
                "WHERE [Host] = @Host AND " +
                "[LastScrape] IS NULL";

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

        public static DataTable GetHostPagesDataTable(Website website)
        {
            string query =
                "SELECT " +
                "[Host], " +
                "[Uri], " +
                "[UriHash], " +
                "[Inserted], " +
                "[LastScrape], " +
                "[NextScrape], " +
                "[HttpStatusCode], " +
                "[Etag], " +
                "[LastModified], " +
                "[PageType], " +
                "[PageTypeCounter], " +
                "[ListingStatus], " +
                "[LockedBy], " +
                "[WaitingStatus], " +
                "[ListingParserInputNotChangedCounter], " +
                "[TransientErrorCounter], " +
                "[TokenCount] " +
                "FROM " + PAGES + " " +
                "WHERE [Host] = @Host " +
                "ORDER BY [Uri]";

            return new DataBase().QueryTable(query, new Dictionary<string, object?>
            {
                { "Host", website.Host }
            });
        }

        private static List<Page> GetPages(int topRows, string where)
        {
            string query =
                "WITH TopPages AS (" +
                "   SELECT TOP " + topRows + " P.[UriHash] " +
                "   FROM " + PAGES + " AS P " +
                "   INNER JOIN " + Websites.Websites.WEBSITES + " AS W ON P.[Host] = W.[Host] " +
                "   WHERE P.[LockedBy] IS NULL AND P.[WaitingStatus] IS NULL " +
                "   AND NOT EXISTS (" +
                "       SELECT 1 " +
                "       FROM " + WebsitesThrottle.WEBSITES_THROTTLE + " AS WB " +
                "       WHERE WB.[IpOrHost] = P.[Host] AND WB.[BlockUntil] > GETDATE()" +
                "   ) " +
                (string.IsNullOrEmpty(where) ? string.Empty : " AND " + where) + " " +
                "   ORDER BY P.[NextScrape] ASC" +
                ") " +
                "UPDATE P " +
                "SET LockedBy = @LockedBy " +
                "OUTPUT " +
                    "INSERTED.[Host], " +
                    "INSERTED.[Uri], " +
                    "INSERTED.[UriHash], " +
                    "INSERTED.[Inserted], " +
                    "INSERTED.[LastScrape], " +
                    "INSERTED.[NextScrape], " +
                    "INSERTED.[HttpStatusCode], " +
                    "INSERTED.[Etag], " +
                    "INSERTED.[LastModified], " +
                    "INSERTED.[PageType], " +
                    "INSERTED.[PageTypeCounter], " +
                    "INSERTED.[ListingStatus], " +
                    "INSERTED.[LockedBy], " +
                    "INSERTED.[WaitingStatus], " +
                    "INSERTED.[ListingParserInputHash], " +
                    "INSERTED.[ListingParserInputNotChangedCounter], " +
                    "INSERTED.[TransientErrorCounter], " +
                    "INSERTED.[ResponseBodyZipped], " +
                    "INSERTED.[TokenCount], " +
                    "W.[MainUri], " +
                    "W.[LanguageCode], " +
                    "W.[CountryCode], " +
                    "W.[RobotsTxt], " +
                    "W.[RobotsTxtUpdated], " +
                    "W.[SitemapUpdated], " +
                    "W.[IpAddress], " +
                    "W.[IpAddressUpdated], " +
                    "W.[IndexUrlRegex], " +
                    "W.[SitemapUrlRegex], " +
                    "W.[ListingUrlRegex]," +
                    "W.[ListingHtmlRemoveXPath], " +
                    "W.[NavigationWaitSelector], " +
                    "W.[AllowedResourceTypes], " +
                    "W.[BlockedDomains], " +
                    "W.[UserAgent], " +
                    "W.[HttpRequestHeaders], " +
                    "W.[ApplySpecialRules], " +
                    "W.[HtmlIndexingEnabled], " +
                    "W.[UseProxy], " +
                    "W.[MinimumRequestIntervalMilliseconds] " +
                "FROM " + PAGES + " AS P " +
                "INNER JOIN " + Websites.Websites.WEBSITES + " AS W ON P.[Host] = W.[Host] " +
                "INNER JOIN TopPages AS TP ON P.[UriHash] = TP.[UriHash]";

            DataTable dataTable = new DataBase().QueryTable(query, new Dictionary<string, object?>(){
                { "LockedBy", Config.IsConfigurationLocal()? null: Config.MACHINE_NAME}
            });
            return GetPages(dataTable);
        }

        private static string SelectQuery(int? topRows = null)
        {
            string top = topRows != null ? "TOP " + topRows : "";
            return
                "SELECT " + top + " " +
                SelectColumns() + " " +
                "FROM " + PAGES + " " +
                "INNER JOIN " + Websites.Websites.WEBSITES +
                " ON " + PAGES + ".[Host] = " + Websites.Websites.WEBSITES + ".[Host] ";
        }

        private static int CountPages()
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + PAGES;

            return new DataBase().QueryInt(query);
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
            string where = lastUriHash == null
                ? string.Empty
                : "WHERE " + PAGES + ".[UriHash] > @LastUriHash ";

            string query =
                SelectQuery(batchSize) +
                where +
                "ORDER BY " + PAGES + ".[UriHash] ASC";

            Dictionary<string, object?> dictionary = [];
            if (lastUriHash != null)
            {
                dictionary.Add("LastUriHash", lastUriHash);
            }

            return GetPages(query, dictionary);
        }

        private static string SelectColumns(string pagesTableName = "")
        {
            if (string.IsNullOrEmpty(pagesTableName))
            {
                pagesTableName = PAGES;
            }
            return
                pagesTableName + ".[Host], " +
                pagesTableName + ".[Uri], " +
                pagesTableName + ".[UriHash], " +
                pagesTableName + ".[Inserted], " +
                pagesTableName + ".[LastScrape], " +
                pagesTableName + ".[NextScrape], " +
                pagesTableName + ".[HttpStatusCode], " +
                pagesTableName + ".[Etag], " +
                pagesTableName + ".[LastModified], " +
                pagesTableName + ".[PageType], " +
                pagesTableName + ".[PageTypeCounter], " +
                pagesTableName + ".[ListingStatus], " +
                pagesTableName + ".[LockedBy], " +
                pagesTableName + ".[WaitingStatus], " +
                pagesTableName + ".[ListingParserInputHash], " +
                pagesTableName + ".[ListingParserInputNotChangedCounter], " +
                pagesTableName + ".[TransientErrorCounter], " +
                pagesTableName + ".[ResponseBodyZipped], " +
                pagesTableName + ".[TokenCount], " +
                Websites.Websites.WEBSITES + ".[MainUri], " +
                Websites.Websites.WEBSITES + ".[LanguageCode], " +
                Websites.Websites.WEBSITES + ".[CountryCode], " +
                Websites.Websites.WEBSITES + ".[RobotsTxt], " +
                Websites.Websites.WEBSITES + ".[RobotsTxtUpdated], " +
                Websites.Websites.WEBSITES + ".[SitemapUpdated], " +
                Websites.Websites.WEBSITES + ".[IpAddress], " +
                Websites.Websites.WEBSITES + ".[IpAddressUpdated], " +
                Websites.Websites.WEBSITES + ".[IndexUrlRegex], " +
                Websites.Websites.WEBSITES + ".[SitemapUrlRegex], " +
                Websites.Websites.WEBSITES + ".[ListingUrlRegex], " +
                Websites.Websites.WEBSITES + ".[ListingHtmlRemoveXPath], " +
                Websites.Websites.WEBSITES + ".[NavigationWaitSelector], " +
                Websites.Websites.WEBSITES + ".[AllowedResourceTypes], " +
                Websites.Websites.WEBSITES + ".[BlockedDomains], " +
                Websites.Websites.WEBSITES + ".[UserAgent], " +
                Websites.Websites.WEBSITES + ".[HttpRequestHeaders], " +
                Websites.Websites.WEBSITES + ".[ApplySpecialRules], " +
                Websites.Websites.WEBSITES + ".[HtmlIndexingEnabled], " +
                Websites.Websites.WEBSITES + ".[UseProxy], " +
                Websites.Websites.WEBSITES + ".[MinimumRequestIntervalMilliseconds] ";
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
    }
}
