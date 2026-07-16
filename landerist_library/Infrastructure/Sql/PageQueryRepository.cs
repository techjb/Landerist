using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Pages;
using System.Data;

namespace landerist_library.Infrastructure.Sql
{
    public class PageQueryRepository
    {
        public DataTable GetPagesByHost(string host)
        {
            string query =
                "SELECT * " +
                "FROM " + Pages.Pages.PAGES + " " +
                "WHERE [Host] = @Host";

            return new DataBase().QueryTable(query, new Dictionary<string, object?> {
                {"Host", host }
            });
        }

        public DataTable GetScrapePages(int topRows)
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
                "   FROM " + Pages.Pages.PAGES + " AS P " +
                "   INNER JOIN " + Websites.Websites.WEBSITES + " AS W ON P.[Host] = W.[Host] " +
                "   WHERE P.[LockedBy] IS NULL AND P.[WaitingStatus] IS NULL " +
                "   AND (P.[PageType] IS NULL OR P.[NextScrape] < GETDATE()) " +
                "   AND NOT EXISTS (" +
                "       SELECT 1 " +
                "       FROM " + WebsitesThrottle.WEBSITES_THROTTLE + " AS WB " +
                "       WHERE WB.[Host] = P.[Host] AND WB.[BlockUntil] > GETDATE()" +
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
                OutputColumns("INSERTED") +
                "FROM " + Pages.Pages.PAGES + " AS P " +
                "INNER JOIN " + Websites.Websites.WEBSITES + " AS W ON P.[Host] = W.[Host] " +
                "INNER JOIN TopPages AS TP ON P.[UriHash] = TP.[UriHash]";

            return new DataBase().QueryTable(query, new Dictionary<string, object?>(){
                { "LockedBy", Config.IsConfigurationLocal()? null: Config.MACHINE_NAME},
                { "MaxPagesPerHost", Config.MAX_PAGES_PER_HOST_PER_SCRAPE}
            });
        }

        public DataTable GetPagesForUpdate(int topRows, string where)
        {
            string query =
                "WITH TopPages AS (" +
                "   SELECT TOP " + topRows + " P.[UriHash] " +
                "   FROM " + Pages.Pages.PAGES + " AS P " +
                "   INNER JOIN " + Websites.Websites.WEBSITES + " AS W ON P.[Host] = W.[Host] " +
                "   WHERE P.[LockedBy] IS NULL AND P.[WaitingStatus] IS NULL " +
                "   AND NOT EXISTS (" +
                "       SELECT 1 " +
                "       FROM " + WebsitesThrottle.WEBSITES_THROTTLE + " AS WB " +
                "       WHERE WB.[Host] = P.[Host] AND WB.[BlockUntil] > GETDATE()" +
                "   ) " +
                (string.IsNullOrEmpty(where) ? string.Empty : " AND " + where) + " " +
                "   ORDER BY P.[NextScrape] ASC" +
                ") " +
                "UPDATE P " +
                "SET LockedBy = @LockedBy " +
                OutputColumns("INSERTED") +
                "FROM " + Pages.Pages.PAGES + " AS P " +
                "INNER JOIN " + Websites.Websites.WEBSITES + " AS W ON P.[Host] = W.[Host] " +
                "INNER JOIN TopPages AS TP ON P.[UriHash] = TP.[UriHash]";

            return new DataBase().QueryTable(query, new Dictionary<string, object?>(){
                { "LockedBy", Config.IsConfigurationLocal()? null: Config.MACHINE_NAME}
            });
        }

        public DataTable GetNonScrapedPages(string host)
        {
            string query =
                "SELECT * " +
                "FROM " + Pages.Pages.PAGES + " " +
                "WHERE [Host] = @Host AND " +
                "[LastScrape] IS NULL";

            return new DataBase().QueryTable(query, new Dictionary<string, object?> {
                {"Host", host }
            });
        }

        public DataTable GetUnknownPageType(string host)
        {
            string query =
                "SELECT * " +
                "FROM " + Pages.Pages.PAGES + " " +
                "WHERE [Host] = @Host AND " +
                "[PageType] IS NULL";

            return new DataBase().QueryTable(query, new Dictionary<string, object?> {
                {"Host", host },
            });
        }

        public List<string> GetUris(bool isListing)
        {
            string query =
                "SELECT [Uri] " +
                "FROM " + Pages.Pages.PAGES + " " +
                "WHERE IsListing = @IsListing";

            return new DataBase().QueryListString(query, new Dictionary<string, object?>()
            {
                { "IsListing", isListing }
            });
        }

        public List<string> GetUris()
        {
            string query =
                "SELECT [Uri] " +
                "FROM " + Pages.Pages.PAGES;

            return new DataBase().QueryListString(query);
        }

        public DataTable GetHostPagesDataTable(string host)
        {
            string query =
                "SELECT " +
                "[Host], " +
                "[Uri], " +
                "[UriHash], " +
                "[Inserted], " +
                "[LastScrape], " +
                "[LastParseListing], " +
                "[NextScrape], " +
                "[HttpStatusCode], " +
                "[Etag], " +
                "[LastModified], " +
                "[PageType], " +
                "[PageTypeCounter], " +
                "[LockedBy], " +
                "[WaitingStatus], " +
                "[ListingParserInputNotChangedCounter], " +
                "[TransientErrorCounter], " +
                "[TokenCount] " +
                "FROM " + Pages.Pages.PAGES + " " +
                "WHERE [Host] = @Host " +
                "ORDER BY [Uri]";

            return new DataBase().QueryTable(query, new Dictionary<string, object?>
            {
                { "Host", host }
            });
        }

        public int CountPages()
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + Pages.Pages.PAGES;

            return new DataBase().QueryInt(query);
        }

        public DataTable GetPagesBatch(string? lastUriHash, int batchSize)
        {
            string where = lastUriHash == null
                ? string.Empty
                : "WHERE " + Pages.Pages.PAGES + ".[UriHash] > @LastUriHash ";

            string query =
                SelectQuery(batchSize) +
                where +
                "ORDER BY " + Pages.Pages.PAGES + ".[UriHash] ASC";

            Dictionary<string, object?> dictionary = [];
            if (lastUriHash != null)
            {
                dictionary.Add("LastUriHash", lastUriHash);
            }

            return QueryPages(query, dictionary);
        }

        public DataTable QueryPages(string query)
        {
            return QueryPages(query, []);
        }

        public DataTable QueryPages(string query, Dictionary<string, object?> parameters)
        {
            return new DataBase().QueryTable(query, parameters);
        }

        public string SelectQuery(int? topRows = null)
        {
            string top = topRows != null ? "TOP " + topRows : "";
            return
                "SELECT " + top + " " +
                SelectColumns() + " " +
                "FROM " + Pages.Pages.PAGES + " " +
                "INNER JOIN " + Websites.Websites.WEBSITES +
                " ON " + Pages.Pages.PAGES + ".[Host] = " + Websites.Websites.WEBSITES + ".[Host] ";
        }

        private static string OutputColumns(string pagesAlias)
        {
            return
                "OUTPUT " +
                    pagesAlias + ".[Host], " +
                    pagesAlias + ".[Uri], " +
                    pagesAlias + ".[UriHash], " +
                    pagesAlias + ".[Inserted], " +
                    pagesAlias + ".[LastScrape], " +
                    pagesAlias + ".[LastParseListing], " +
                    pagesAlias + ".[NextScrape], " +
                    pagesAlias + ".[HttpStatusCode], " +
                    pagesAlias + ".[Etag], " +
                    pagesAlias + ".[LastModified], " +
                    pagesAlias + ".[PageType], " +
                    pagesAlias + ".[PageTypeCounter], " +
                    pagesAlias + ".[LockedBy], " +
                    pagesAlias + ".[WaitingStatus], " +
                    pagesAlias + ".[ListingParserInputHash], " +
                    pagesAlias + ".[ListingParserInputNotChangedCounter], " +
                    pagesAlias + ".[TransientErrorCounter], " +
                    pagesAlias + ".[ResponseBodyZipped], " +
                    pagesAlias + ".[TokenCount], " +
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
                    "W.[ListingCoordinateRegex], " +
                    "W.[ListingHtmlRemoveXPath], " +
                    "W.[ListingUnavailableRegex], " +
                    "W.[NavigationWaitSelector], " +
                    "W.[AllowedResourceTypes], " +
                    "W.[BlockedDomains], " +
                    "W.[UserAgent], " +
                    "W.[HttpRequestHeaders], " +
                    "W.[HtmlIndexingEnabled], " +
                    "W.[UseProxy], " +
                    "W.[MinimumRequestIntervalMilliseconds] ";
        }

        public static string SelectColumns(string pagesTableName = "")
        {
            if (string.IsNullOrEmpty(pagesTableName))
            {
                pagesTableName = Pages.Pages.PAGES;
            }
            return
                pagesTableName + ".[Host], " +
                pagesTableName + ".[Uri], " +
                pagesTableName + ".[UriHash], " +
                pagesTableName + ".[Inserted], " +
                pagesTableName + ".[LastScrape], " +
                pagesTableName + ".[LastParseListing], " +
                pagesTableName + ".[NextScrape], " +
                pagesTableName + ".[HttpStatusCode], " +
                pagesTableName + ".[Etag], " +
                pagesTableName + ".[LastModified], " +
                pagesTableName + ".[PageType], " +
                pagesTableName + ".[PageTypeCounter], " +
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
                Websites.Websites.WEBSITES + ".[ListingCoordinateRegex], " +
                Websites.Websites.WEBSITES + ".[ListingHtmlRemoveXPath], " +
                Websites.Websites.WEBSITES + ".[ListingUnavailableRegex], " +
                Websites.Websites.WEBSITES + ".[NavigationWaitSelector], " +
                Websites.Websites.WEBSITES + ".[AllowedResourceTypes], " +
                Websites.Websites.WEBSITES + ".[BlockedDomains], " +
                Websites.Websites.WEBSITES + ".[UserAgent], " +
                Websites.Websites.WEBSITES + ".[HttpRequestHeaders], " +
                Websites.Websites.WEBSITES + ".[HtmlIndexingEnabled], " +
                Websites.Websites.WEBSITES + ".[UseProxy], " +
                Websites.Websites.WEBSITES + ".[MinimumRequestIntervalMilliseconds] ";
        }
    }
}
