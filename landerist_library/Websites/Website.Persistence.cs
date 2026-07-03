using landerist_library.Infrastructure.Sql;
using landerist_orels.ES;
using System.Data;

namespace landerist_library.Websites
{
    public partial class Website
    {
        private static readonly WebsiteRepository WebsiteRepository = new();

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
            return WebsiteRepository.GetDataRow(Host);
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
            ListingCoordinateRegex = dataRow.Table.Columns.Contains("ListingCoordinateRegex") && dataRow["ListingCoordinateRegex"] is not DBNull
                ? NullIfWhiteSpace(dataRow["ListingCoordinateRegex"].ToString())
                : null;
            ListingHtmlRemoveXPath = dataRow["ListingHtmlRemoveXPath"] is not DBNull
                ? dataRow["ListingHtmlRemoveXPath"].ToString()
                : null;
            ListingUnavailableRegex = dataRow.Table.Columns.Contains("ListingUnavailableRegex") && dataRow["ListingUnavailableRegex"] is not DBNull
                ? NullIfWhiteSpace(dataRow["ListingUnavailableRegex"].ToString())
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
            HtmlIndexingEnabled = dataRow["HtmlIndexingEnabled"] is not DBNull
                ? (bool)dataRow["HtmlIndexingEnabled"]
                : false;
            UseProxy = dataRow["UseProxy"] is not DBNull
                && (bool)dataRow["UseProxy"];
            MinimumRequestIntervalMilliseconds = dataRow["MinimumRequestIntervalMilliseconds"] is not DBNull
                ? (int)dataRow["MinimumRequestIntervalMilliseconds"]
                : null;
        }

        public bool Insert()
        {
            var parameters = GetQueryParameters();
            return WebsiteRepository.Insert(parameters);
        }

        public bool Update()
        {
            var parameters = GetQueryParameters();
            return WebsiteRepository.Update(parameters);
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
                {"ListingCoordinateRegex", NullIfWhiteSpace(ListingCoordinateRegex) },
                {"ListingHtmlRemoveXPath", ListingHtmlRemoveXPath },
                {"ListingUnavailableRegex", NullIfWhiteSpace(ListingUnavailableRegex) },
                {"NavigationWaitSelector", NullIfWhiteSpace(NavigationWaitSelector) },
                {"AllowedResourceTypes", AllowedResourceTypes },
                {"BlockedDomains", NullIfWhiteSpace(BlockedDomains) },
                {"UserAgent", NullIfWhiteSpace(UserAgent) },
                {"HttpRequestHeaders", NullIfWhiteSpace(HttpRequestHeaders) },
                {"HtmlIndexingEnabled", HtmlIndexingEnabled },
                {"UseProxy", UseProxy },
                {"MinimumRequestIntervalMilliseconds", MinimumRequestIntervalMilliseconds },
            };
        }

        private bool DeleteWebsite()
        {
            return WebsiteRepository.Delete(Host);
        }
    }
}
