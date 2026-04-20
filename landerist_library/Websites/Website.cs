using Com.Bekijkhet.RobotsTxt;
using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Index;
using landerist_library.Pages;
using System.Data;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace landerist_library.Websites
{

    // https://www.w3schools.com/tags/ref_country_codes.asp
    public enum CountryCode
    {
        ES
    }

    // https://www.w3schools.com/tags/ref_language_codes.asp
    public enum LanguageCode
    {
        es
    }

    public class Website : IDisposable
    {
        public Uri MainUri { get; set; }

        public string Host { get; set; } = string.Empty;

        public string? RobotsTxt { get; set; }

        public DateTime? RobotsTxtUpdated { get; set; }

        public DateTime? SitemapUpdated { get; set; }

        public string? IpAddress { get; set; }

        public DateTime? IpAddressUpdated { get; set; }

        public string? IndexUrlRegex { get; set; }

        public string? SitemapUrlRegex { get; set; }

        public string? ListingUrlRegex { get; set; }

        public bool ApplySpecialRules { get; set; }

        public Robots? Robots = null;

        public LanguageCode LanguageCode = LanguageCode.es;

        public CountryCode CountryCode = CountryCode.ES;

        private bool Disposed;

        public Website()
        {
            MainUri = new Uri("about:blank", UriKind.RelativeOrAbsolute);
        }

        public Website(string host) : this()
        {
            Host = host;
            LoadDataRow();
        }

        public Website(Uri mainUri) : this()
        {
            SetMainUri(mainUri);
            LoadDataRow();
        }

        private void SetMainUri(Uri mainUri)
        {
            MainUri = mainUri;
            Host = MainUri.Host;
        }

        public Website(DataRow dataRow) : this()
        {
            Load(dataRow);
        }

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
            SitemapUrlRegex = dataRow.Table.Columns.Contains("SitemapUrlRegex") && dataRow["SitemapUrlRegex"] is not DBNull
                ? dataRow["SitemapUrlRegex"].ToString()
                : null;
            ListingUrlRegex = dataRow.Table.Columns.Contains("ListingUrlRegex") && dataRow["ListingUrlRegex"] is not DBNull
                ? dataRow["ListingUrlRegex"].ToString()
                : null;
            ApplySpecialRules = dataRow.Table.Columns.Contains("ApplySpecialRules")
                && dataRow["ApplySpecialRules"] is not DBNull
                && (bool)dataRow["ApplySpecialRules"];
        }

        public bool Insert()
        {
            string query =
                "INSERT INTO " + Websites.WEBSITES + " (" +
                "[MainUri], [Host], [LanguageCode], [CountryCode], [RobotsTxt], [RobotsTxtUpdated], " +
                "[SitemapUpdated], [IpAddress], [IpAddressUpdated], [IndexUrlRegex], [SitemapUrlRegex], [ListingUrlRegex], [ApplySpecialRules]) VALUES (" +
                "@MainUri, @Host, @LanguageCode, @CountryCode, @RobotsTxt, @RobotsTxtUpdated, " +
                "@SitemapUpdated, @IpAddress, @IpAddressUpdated, @IndexUrlRegex, @SitemapUrlRegex, @ListingUrlRegex, @ApplySpecialRules)";

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
                "[ApplySpecialRules] = @ApplySpecialRules " +
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
                {"ApplySpecialRules", ApplySpecialRules },
            };
        }

        public bool IsDiscardedByIndexUrlRegex(Uri uri)
        {
            return IsDiscardedByRegex(uri, IndexUrlRegex, "IndexUrlRegex");
        }

        public bool IsDiscardedByListingUrlRegex(Uri uri)
        {
            return IsDiscardedByRegex(uri, ListingUrlRegex, "ListingUrlRegex");
        }

        public bool IsDiscardedBySitemapUrlRegex(Uri uri)
        {
            return IsDiscardedByRegex(uri, SitemapUrlRegex, "SitemapUrlRegex");
        }

        private bool IsDiscardedByRegex(Uri uri, string? regexPattern, string regexFieldName)
        {
            ArgumentNullException.ThrowIfNull(uri);

            if (string.IsNullOrWhiteSpace(regexPattern))
            {
                return false;
            }

            try
            {
                return !Regex.IsMatch(
                    uri.AbsoluteUri,
                    regexPattern,
                    RegexOptions.IgnoreCase,
                    TimeSpan.FromSeconds(1));
            }
            catch (ArgumentException exception)
            {
                Logs.Log.WriteError(
                    "Website IsDiscardedByRegex",
                    $"{Host} {regexFieldName} {regexPattern}",
                    exception);

                return false;
            }
        }

        public bool SetMainUri(int iteration = 0)
        {
            if (iteration >= 10)
            {
                return false;
            }

            HttpClientHandler handler = new()
            {
                AllowAutoRedirect = false
            };

            using var httpClient = new HttpClient(handler);
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(Config.USER_AGENT);
            httpClient.Timeout = TimeSpan.FromSeconds(Config.HTTPCLIENT_SECONDS_TIMEOUT);

            try
            {
                HttpRequestMessage request = new(HttpMethod.Head, MainUri);
                var response = httpClient.SendAsync(request).GetAwaiter().GetResult();

                if (response?.Headers?.Location != null)
                {
                    var uriLocation = response.Headers.Location;

                    if (uriLocation.ToString().StartsWith('/'))
                    {
                        Uri.TryCreate(MainUri, uriLocation, out uriLocation);
                    }

                    if (uriLocation != null && !uriLocation.Equals(MainUri))
                    {
                        SetMainUri(uriLocation);
                        return SetMainUri(iteration + 1);
                    }
                }

                return true;
            }
            catch //(Exception exception)
            {
                //Logs.Log.WriteLogErrors("Website SetMainUriAndStatusCode", MainUri, exception);
            }

            return false;
        }

        public bool SetRobotsTxt()
        {
            RobotsTxtUpdated = DateTime.Now;

            var robotsTxtUrl = new Uri(MainUri, "/robots.txt");

            try
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(Config.USER_AGENT);

                var response = httpClient.GetAsync(robotsTxtUrl).GetAwaiter().GetResult();
                RobotsTxt = null;
                Robots = null;

                if (response.IsSuccessStatusCode)
                {
                    using var streamReader = new StreamReader(response.Content.ReadAsStreamAsync().GetAwaiter().GetResult(), Encoding.Default);
                    RobotsTxt = streamReader.ReadToEnd();
                }

                return true;
            }
            catch //(Exception exception)
            {
                //Logs.Log.WriteLogErrors("Website SetRobotsTxt", robotsTxtUrl, exception);
            }

            return false;
        }

        public bool SetIpAddress()
        {
            IpAddressUpdated = DateTime.Now;

            try
            {
                IPAddress[] ipAddresses = Dns.GetHostAddresses(Host);
                IpAddress = null;

                if (ipAddresses.Length > 0)
                {
                    IpAddress = ipAddresses[0].ToString();
                }

                return true;
            }
            catch// (Exception exception)
            {
                //Logs.Log.WriteLogErrors("Website SetIpAddress", Host, exception);
            }

            return false;
        }

        public bool IsMainUriAllowedByRobotsTxt()
        {
            return IsAllowedByRobotsTxt(MainUri);
        }

        public bool IsAllowedByRobotsTxt(Uri uri)
        {
            if (RobotsTxt != null)
            {
                Robots ??= Com.Bekijkhet.RobotsTxt.Robots.Load(RobotsTxt);
                return Robots.IsPathAllowed(Config.USER_AGENT, uri.PathAndQuery);
            }

            return true;
        }

        public int CountRobotsSiteMaps()
        {
            if (RobotsTxt != null)
            {
                Robots ??= Robots.Load(RobotsTxt);

                if (Robots.Sitemaps != null)
                {
                    return Robots.Sitemaps.Count;
                }
            }

            return 0;
        }

        public int CrawlDelay()
        {
            if (RobotsTxt != null)
            {
                Robots ??= Robots.Load(RobotsTxt);
                return (int)Robots.CrawlDelay(Config.USER_AGENT) / 1000;
            }

            return 0;
        }

        public bool CrawlDelayTooBig()
        {
            var crawlDelay = CrawlDelay();
            return crawlDelay > Config.MAX_CRAW_DELAY_SECONDS;
        }

        public bool Delete()
        {
            DeleteListings();
            Pages.Pages.Delete(this);
            return DeleteWebsite();
        }

        public void DeleteListings()
        {
            int counter = 0;
            var pages = GetPages();

            foreach (var page in pages)
            {
                if (page.DeleteListing())
                {
                    counter++;
                }
            }
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

        public bool InsertMainPage()
        {
            var page = new Page(this);
            return page.Insert();
        }

        public void SetSitemap()
        {
            SitemapUpdated = DateTime.Now;

            if (!Config.INDEXER_ENABLED)
            {
                return;
            }

            try
            {
                var sitemaps = GetSiteMapsFromRobotsTxt();
                if (sitemaps != null && sitemaps.Count > 0)
                {
                    new SitemapIndexer(this).InsertSitemaps(sitemaps);
                    return;
                }

                var uri = GetDefaultSiteMap();
                if (uri != null)
                {
                    new SitemapIndexer(this).InsertSitemap(uri);
                }
            }
            catch (Exception exception)
            {
                Logs.Log.WriteError("Website InsertPagesFromSiteMap", Host, exception);
            }
        }

        private Uri? GetDefaultSiteMap()
        {
            Uri.TryCreate(MainUri, "sitemap.xml", out Uri? uri);
            return uri;
        }

        public List<Sitemap>? GetSiteMapsFromRobotsTxt()
        {
            if (RobotsTxt != null)
            {
                Robots ??= Robots.Load(RobotsTxt);
                return Robots.Sitemaps;
            }

            return null;
        }

        public List<Page> GetPages()
        {
            return Pages.Pages.GetPages(this);
        }

        public List<Page> GetPagesUnknowPageType()
        {
            return Pages.Pages.GetUnknowPageType(this);
        }

        public List<Page> GetNonScrapedPages()
        {
            return Pages.Pages.GetNonScrapedPages(this);
        }

        public int GetNumPages()
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + Pages.Pages.PAGES + " " +
                "WHERE [Host] = @Host";

            return new DataBase().QueryInt(query, new Dictionary<string, object?> {
                {"Host", Host }
            });
        }       

        public bool AchievedMaxNumberOfPages()
        {
            return 
                //Config.IsConfigurationProduction() &&
                GetNumPages() >= GetMaxPagesPerWebsite();
        }

        private int GetMaxPagesPerWebsite()
        {
            return ApplySpecialRules
                ? Config.MAX_PAGES_PER_WEBSITE_SPECIAL_RULES
                : Config.MAX_PAGES_PER_WEBSITE;
        }

        public int GetNumListings()
        {
            return ES_Listings.CountByHost(Host);
        }

        public void Dispose()
        {
            if (!Disposed)
            {
                Host = string.Empty;
                IpAddress = null;
                Robots = null;
                RobotsTxt = null;
                IndexUrlRegex = null;
                SitemapUrlRegex = null;
                ApplySpecialRules = false;
            }

            Disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
