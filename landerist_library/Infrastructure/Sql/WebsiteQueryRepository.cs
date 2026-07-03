using landerist_library.Database;
using System.Data;

namespace landerist_library.Infrastructure.Sql
{
    public class WebsiteQueryRepository
    {
        public HashSet<string> GetHosts()
        {
            string query =
                "SELECT [Host] " +
                "FROM " + Websites.Websites.WEBSITES;

            return new DataBase().QueryHashSet(query);
        }

        public DataTable GetAll()
        {
            string query =
                "SELECT * " +
                "FROM " + Websites.Websites.WEBSITES;

            return new DataBase().QueryTable(query);
        }

        public DataTable GetHostMainUri()
        {
            string query =
                "SELECT [Host], [MainUri] " +
                "FROM " + Websites.Websites.WEBSITES;

            return new DataBase().QueryTable(query);
        }

        public DataTable GetHttpStatusCodeOk()
        {
            string query =
                "SELECT * " +
                "FROM " + Websites.Websites.WEBSITES + " " +
                "WHERE [HttpStatusCode] = 200";

            return new DataBase().QueryTable(query);
        }

        public DataTable GetHttpStatusCodeNotOk()
        {
            string query =
                "SELECT * " +
                "FROM " + Websites.Websites.WEBSITES + " " +
                "WHERE [HttpStatusCode] <> 200 AND [HttpStatusCode] IS NOT NULL";

            return new DataBase().QueryTable(query);
        }

        public DataTable GetHttpStatusCodeNull()
        {
            string query =
                "SELECT * " +
                "FROM " + Websites.Websites.WEBSITES + " " +
                "WHERE [HttpStatusCode] IS NULL";

            return new DataBase().QueryTable(query);
        }

        public DataTable GetWebsite(string host)
        {
            string query =
                "SELECT TOP 1 * " +
                "FROM " + Websites.Websites.WEBSITES + " " +
                "WHERE [Host] = @Host";

            return new DataBase().QueryTable(query, new Dictionary<string, object?> {
                {"Host", host }
            });
        }

        public bool Exists(string host)
        {
            string query =
                "SELECT 1 " +
                "FROM " + Websites.Websites.WEBSITES + " " +
                "WHERE Host = @Host";

            return new DataBase().QueryExists(query, new Dictionary<string, object?> {
                {"Host", host }
            });
        }

        public HashSet<string> GetUrls()
        {
            string query =
                "SELECT [Uri] " +
                "FROM " + Websites.Websites.WEBSITES;

            return new DataBase().QueryHashSet(query);
        }

        public bool DeleteAll()
        {
            string query =
             "DELETE FROM " + Websites.Websites.WEBSITES;

            return new DataBase().Query(query);
        }

        public DataTable GetNeedToUpdateRobotsTxt(DateTime robotsTxtUpdatedSpecialRules)
        {
            string query =
                "SELECT * " +
                "FROM " + Websites.Websites.WEBSITES + " " +
                "WHERE ([RobotsTxtUpdated] IS NULL OR [RobotsTxtUpdated] < @RobotsTxtUpdatedSpecialRules)";

            return new DataBase().QueryTable(query, new Dictionary<string, object?> {
                {"RobotsTxtUpdatedSpecialRules", robotsTxtUpdatedSpecialRules },
            });
        }

        public DataTable GetNeedToUpdateSitemaps(DateTime sitemapUpdatedSpecialRules)
        {
            string query =
                "SELECT * " +
                "FROM " + Websites.Websites.WEBSITES + " " +
                "WHERE ([SitemapUpdated] IS NULL OR [SitemapUpdated] < @SitemapUpdatedSpecialRules)";

            return new DataBase().QueryTable(query, new Dictionary<string, object?> {
                {"SitemapUpdatedSpecialRules", sitemapUpdatedSpecialRules },
            });
        }

        public DataTable GetNeedToUpdateIpAddress(DateTime ipAddressUpdated)
        {
            string query =
                "SELECT * " +
                "FROM " + Websites.Websites.WEBSITES + " " +
                "WHERE ([IpAddressUpdated] < @IpAddressUpdated OR [IpAddressUpdated] IS NULL)";

            return new DataBase().QueryTable(query, new Dictionary<string, object?> {
                {"IpAddressUpdated", ipAddressUpdated },
            });
        }
    }
}
