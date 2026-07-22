using landerist_library.Database;
using landerist_library.Websites;
using System.Data;

namespace landerist_library.Infrastructure.Sql
{
    public class WebsiteRepository
    {
        private readonly IDatabase? _database;

        public WebsiteRepository()
        {
        }

        public WebsiteRepository(IDatabase database)
        {
            ArgumentNullException.ThrowIfNull(database);
            _database = database;
        }

        private IDatabase Database => _database ?? new DataBase();

        public DataRow? GetDataRow(string host)
        {
            string query =
                "SELECT * " +
                "FROM " + Websites.Websites.WEBSITES + " " +
                "WHERE [Host] = @Host";

            var dataTable = Database.QueryTable(query, new Dictionary<string, object?> {
                {"Host", host }
            });

            return dataTable.Rows.Count > 0
                ? dataTable.Rows[0]
                : null;
        }

        public bool Insert(IDictionary<string, object?> parameters)
        {
            string query =
                "INSERT INTO " + Websites.Websites.WEBSITES + " (" +
                "[MainUri], [Host], [LanguageCode], [CountryCode], [RobotsTxt], [RobotsTxtUpdated], " +
                "[SitemapUpdated], [IpAddress], [IpAddressUpdated], [IndexUrlRegex], [SitemapUrlRegex], [ListingUrlRegex], [ListingCoordinateRegex], [ListingHtmlRemoveXPath], [ListingUnavailableRegex], [NavigationWaitSelector], [AllowedResourceTypes], [BlockedDomains], [UserAgent], [HttpRequestHeaders], [HtmlIndexingEnabled], [UseProxy], [MinimumRequestIntervalMilliseconds]) VALUES (" +
                "@MainUri, @Host, @LanguageCode, @CountryCode, @RobotsTxt, @RobotsTxtUpdated, " +
                "@SitemapUpdated, @IpAddress, @IpAddressUpdated, @IndexUrlRegex, @SitemapUrlRegex, @ListingUrlRegex, @ListingCoordinateRegex, @ListingHtmlRemoveXPath, @ListingUnavailableRegex, @NavigationWaitSelector, @AllowedResourceTypes, @BlockedDomains, @UserAgent, @HttpRequestHeaders, @HtmlIndexingEnabled, @UseProxy, @MinimumRequestIntervalMilliseconds)";

            return Database.Query(query, parameters);
        }

        public bool Update(IDictionary<string, object?> parameters)
        {
            string query =
                "UPDATE " + Websites.Websites.WEBSITES + " SET " +
                "[MainUri] = @MainUri, " +
                "[LanguageCode] = @LanguageCode, " +
                "[CountryCode] = @CountryCode, " +
                "[RobotsTxt] = @RobotsTxt, " +
                "[RobotsTxtUpdated] = @RobotsTxtUpdated, " +
                "[SitemapUpdated] = @SitemapUpdated, " +
                "[IpAddress] = @IpAddress, " +
                "[IpAddressUpdated] = @IpAddressUpdated, " +
                "[IndexUrlRegex] = @IndexUrlRegex, " +
                "[SitemapUrlRegex] = @SitemapUrlRegex, " +
                "[ListingUrlRegex] = @ListingUrlRegex, " +
                "[ListingCoordinateRegex] = @ListingCoordinateRegex, " +
                "[ListingHtmlRemoveXPath] = @ListingHtmlRemoveXPath, " +
                "[ListingUnavailableRegex] = @ListingUnavailableRegex, " +
                "[NavigationWaitSelector] = @NavigationWaitSelector, " +
                "[AllowedResourceTypes] = @AllowedResourceTypes, " +
                "[BlockedDomains] = @BlockedDomains, " +
                "[UserAgent] = @UserAgent, " +
                "[HttpRequestHeaders] = @HttpRequestHeaders, " +
                "[HtmlIndexingEnabled] = @HtmlIndexingEnabled, " +
                "[UseProxy] = @UseProxy, " +
                "[MinimumRequestIntervalMilliseconds] = @MinimumRequestIntervalMilliseconds " +
                "WHERE [Host] = @Host";

            return Database.Query(query, parameters);
        }

        public bool Delete(string host)
        {
            string query =
               "DELETE FROM " + Websites.Websites.WEBSITES + " " +
               "WHERE [Host] = @Host";

            return Database.Query(query, new Dictionary<string, object?> {
                {"Host", host }
            });
        }
    }
}
