using landerist_library.Database;
using landerist_orels.ES;
using System.Data;

namespace landerist_library.Websites
{
    public partial class Website
    {
        private void LoadDataRow()
        {
            var dataRow = GetDataRow();
            if (dataRow != null)
            {
                Load(dataRow);
            }
        }

        private DataRow? GetDataRow()
        {
            string query =
                "SELECT * " +
                "FROM " + Websites.WEBSITES + " " +
                "WHERE [Host] = @Host";

            var dataTable = new DataBase().QueryTable(query, new Dictionary<string, object?> {
                {"Host", Host }
            });

            if (dataTable.Rows.Count > 0)
            {
                return dataTable.Rows[0];
            }

            return null;
        }

        private void Load(DataRow dataRow)
        {
            string mainUriString = dataRow["MainUri"].ToString()!;
            MainUri = new(mainUriString);
            Host = dataRow["Host"].ToString()!;
            LanguageCode = (LanguageCode)Enum.Parse(typeof(LanguageCode), dataRow["LanguageCode"].ToString()!);
            CountryCode = (CountryCode)Enum.Parse(typeof(CountryCode), dataRow["CountryCode"].ToString()!);
            RobotsTxt = dataRow["RobotsTxt"] is DBNull ? null : dataRow["RobotsTxt"].ToString();
            RobotsTxtUpdated = dataRow["RobotsTxtUpdated"] is DBNull ? null : (DateTime)dataRow["RobotsTxtUpdated"];
            SitemapUpdated = dataRow["SitemapUpdated"] is DBNull ? null : (DateTime)dataRow["SitemapUpdated"];
            IpAddress = dataRow["IpAddress"] is DBNull ? null : dataRow["IpAddress"].ToString();
            IpAddressUpdated = dataRow["IpAddressUpdated"] is DBNull ? null : (DateTime)dataRow["IpAddressUpdated"];
            IndexUrlRegex = dataRow["IndexUrlRegex"] is DBNull ? null : dataRow["IndexUrlRegex"].ToString();
            SitemapUrlRegex = dataRow["SitemapUrlRegex"] is not DBNull
                ? dataRow["SitemapUrlRegex"].ToString()
                : null;
            ListingUrlRegex = dataRow["ListingUrlRegex"] is not DBNull
                ? dataRow["ListingUrlRegex"].ToString()
                : null;
            ListingHtmlRemoveXPath = dataRow["ListingHtmlRemoveXPath"] is not DBNull
                ? dataRow["ListingHtmlRemoveXPath"].ToString()
                : null;
            NavigationWaitSelector = dataRow.Table.Columns.Contains("NavigationWaitSelector") && dataRow["NavigationWaitSelector"] is not DBNull
                ? NullIfWhiteSpace(dataRow["NavigationWaitSelector"].ToString())
                : null;
            AllowedResourceTypes = dataRow["AllowedResourceTypes"] is not DBNull
                ? dataRow["AllowedResourceTypes"].ToString()
                : null;
            BlockedDomains = dataRow.Table.Columns.Contains("BlockedDomains") && dataRow["BlockedDomains"] is not DBNull
                ? dataRow["BlockedDomains"].ToString()
                : null;
            UserAgent = dataRow["UserAgent"] is not DBNull
                ? NullIfWhiteSpace(dataRow["UserAgent"].ToString())
                : null;
            HttpRequestHeaders = dataRow["HttpRequestHeaders"] is not DBNull
                ? NullIfWhiteSpace(dataRow["HttpRequestHeaders"].ToString())
                : null;
            ApplySpecialRules = dataRow["ApplySpecialRules"] is not DBNull
                && (bool)dataRow["ApplySpecialRules"];
            HtmlIndexingEnabled = dataRow["HtmlIndexingEnabled"] is not DBNull
                ? (bool)dataRow["HtmlIndexingEnabled"]
                : !ApplySpecialRules;
            UseProxy = dataRow["UseProxy"] is not DBNull
                && (bool)dataRow["UseProxy"];
            MinimumRequestIntervalMilliseconds = dataRow["MinimumRequestIntervalMilliseconds"] is not DBNull
                ? (int)dataRow["MinimumRequestIntervalMilliseconds"]
                : null;
        }

        public bool Insert()
        {
            string query =
                "INSERT INTO " + Websites.WEBSITES + " (" +
                "[MainUri], [Host], [LanguageCode], [CountryCode], [RobotsTxt], [RobotsTxtUpdated], " +
                "[SitemapUpdated], [IpAddress], [IpAddressUpdated], [IndexUrlRegex], [SitemapUrlRegex], [ListingUrlRegex], [ListingHtmlRemoveXPath], [NavigationWaitSelector], [AllowedResourceTypes], [BlockedDomains], [UserAgent], [HttpRequestHeaders], [ApplySpecialRules], [HtmlIndexingEnabled], [UseProxy], [MinimumRequestIntervalMilliseconds]) VALUES (" +
                "@MainUri, @Host, @LanguageCode, @CountryCode, @RobotsTxt, @RobotsTxtUpdated, " +
                "@SitemapUpdated, @IpAddress, @IpAddressUpdated, @IndexUrlRegex, @SitemapUrlRegex, @ListingUrlRegex, @ListingHtmlRemoveXPath, @NavigationWaitSelector, @AllowedResourceTypes, @BlockedDomains, @UserAgent, @HttpRequestHeaders, @ApplySpecialRules, @HtmlIndexingEnabled, @UseProxy, @MinimumRequestIntervalMilliseconds)";

            var parameters = GetQueryParameters();
            return new DataBase().Query(query, parameters);
        }

        public bool Update()
        {
            string query =
                "UPDATE " + Websites.WEBSITES + " SET " +
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
                "[ListingHtmlRemoveXPath] = @ListingHtmlRemoveXPath, " +
                "[NavigationWaitSelector] = @NavigationWaitSelector, " +
                "[AllowedResourceTypes] = @AllowedResourceTypes, " +
                "[BlockedDomains] = @BlockedDomains, " +
                "[UserAgent] = @UserAgent, " +
                "[HttpRequestHeaders] = @HttpRequestHeaders, " +
                "[ApplySpecialRules] = @ApplySpecialRules, " +
                "[HtmlIndexingEnabled] = @HtmlIndexingEnabled, " +
                "[UseProxy] = @UseProxy, " +
                "[MinimumRequestIntervalMilliseconds] = @MinimumRequestIntervalMilliseconds " +
                "WHERE [Host] = @Host";

            var parameters = GetQueryParameters();
            return new DataBase().Query(query, parameters);
        }

        private Dictionary<string, object?> GetQueryParameters()
        {
            return new Dictionary<string, object?> {
                {"MainUri", MainUri.ToString() },
                {"Host", Host },
                {"LanguageCode", LanguageCode.ToString() },
                {"CountryCode", CountryCode.ToString() },
                {"RobotsTxt", RobotsTxt },
                {"RobotsTxtUpdated", RobotsTxtUpdated},
                {"SitemapUpdated", SitemapUpdated},
                {"IpAddress", IpAddress },
                {"IpAddressUpdated", IpAddressUpdated},
                {"IndexUrlRegex", IndexUrlRegex },
                {"SitemapUrlRegex", SitemapUrlRegex },
                {"ListingUrlRegex", ListingUrlRegex },
                {"ListingHtmlRemoveXPath", ListingHtmlRemoveXPath },
                {"NavigationWaitSelector", NullIfWhiteSpace(NavigationWaitSelector) },
                {"AllowedResourceTypes", AllowedResourceTypes },
                {"BlockedDomains", NullIfWhiteSpace(BlockedDomains) },
                {"UserAgent", NullIfWhiteSpace(UserAgent) },
                {"HttpRequestHeaders", NullIfWhiteSpace(HttpRequestHeaders) },
                {"ApplySpecialRules", ApplySpecialRules },
                {"HtmlIndexingEnabled", HtmlIndexingEnabled },
                {"UseProxy", UseProxy },
                {"MinimumRequestIntervalMilliseconds", MinimumRequestIntervalMilliseconds },
            };
        }

        private bool DeleteWebsite()
        {
            string query =
               "DELETE FROM " + Websites.WEBSITES + " " +
               "WHERE [Host] = @Host";

            return new DataBase().Query(query, new Dictionary<string, object?> {
                {"Host", Host }
            });
        }
    }
}
