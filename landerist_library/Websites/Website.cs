using Com.Bekijkhet.RobotsTxt;
using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Index;
using System.Data;
using System.Net;
using System.Text;

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

    public class Website
    {
        public Uri MainUri { get; set; }

        public string Host { get; set; } = string.Empty;

        public string? RobotsTxt { get; set; }

        public string? IpAddress { get; set; }

        public short? HttpStatusCode { get; set; }

        private int NumPages { get; set; } = 0;

        private int NumListings { get; set; } = 0;


        private Robots? Robots = null;


        public LanguageCode LanguageCode = LanguageCode.es;


        public CountryCode CountryCode = CountryCode.ES;

        public Website()
        {
            MainUri = new Uri("about:blank", UriKind.RelativeOrAbsolute);
        }

        public Website(Uri mainUri) : this()
        {
            SetMainUri(mainUri);
            var dataRow = GetDataRow();
            if (dataRow != null)
            {
                Load(dataRow);
            }
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

        private DataRow? GetDataRow()
        {
            string query =
                "SELECT * " +
                "FROM " + Websites.TABLE_WEBSITES + " " +
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
            IpAddress = dataRow["IpAddress"] is DBNull ? null : dataRow["IpAddress"].ToString();
            HttpStatusCode = dataRow["HttpStatusCode"] is DBNull ? null : (short)dataRow["HttpStatusCode"];
            NumPages = (int)dataRow["NumPages"];
            NumListings = (int)dataRow["NumListings"];
        }

        public bool Insert()
        {
            string query =
                "INSERT INTO " + Websites.TABLE_WEBSITES + " " +
                "VALUES (@MainUri, @Host, @LanguageCode, @CountryCode, @RobotsTxt, " +
                "@IpAddress, @HttpStatusCode, @NumPages, @NumListings)";

            var parameters = GetQueryParameters();
            return new DataBase().Query(query, parameters);
        }

        public bool Update()
        {
            string query =
                "UPDATE " + Websites.TABLE_WEBSITES + " SET " +
                "[MainUri] = @MainUri, " +
                "[LanguageCode] = @LanguageCode, " +
                "[CountryCode] = @CountryCode, " +
                "[RobotsTxt] = @RobotsTxt, " +
                "[IpAddress] = @IpAddress, " +
                "[HttpStatusCode] = @HttpStatusCode, " +
                "[NumPages] = @NumPages, " +
                "[NumListings] = @NumListings " +
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
                {"IpAddress", IpAddress },
                {"HttpStatusCode", HttpStatusCode },
                {"NumPages", NumPages },
                {"NumListings", NumListings },
            };
        }

        public bool SetMainUriAndStatusCode(int iteration = 0)
        {
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
                HttpStatusCode = (short)response.StatusCode;
                if (response != null && response.Headers != null && response.Headers.Location != null)
                {
                    var uriLocation = response.Headers.Location;
                    if (uriLocation.ToString().StartsWith("/"))
                    {
                        Uri.TryCreate(MainUri, uriLocation, out uriLocation);
                    }
                    if (uriLocation != null && !uriLocation.Equals(MainUri))
                    {
                        SetMainUri(uriLocation);
                        iteration++;
                        if (iteration < 10)
                        {
                            return SetMainUriAndStatusCode(iteration++);
                        }
                    }
                }
                return true;
            }
            catch (Exception exception)
            {
                Logs.Log.WriteLogErrors(MainUri, exception);
            }
            return false;
        }

        public bool SetRobotsTxt()
        {
            var robotsTxtUrl = new Uri(MainUri, "/robots.txt");
            try
            {
                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(Config.USER_AGENT);
                var response = httpClient.GetAsync(robotsTxtUrl).GetAwaiter().GetResult();
                RobotsTxt = null;
                if (response.IsSuccessStatusCode)
                {
                    using var streamReader = new StreamReader(response.Content.ReadAsStreamAsync().GetAwaiter().GetResult(), Encoding.Default);
                    RobotsTxt = streamReader.ReadToEnd();
                }
                return true;
            }
            catch (Exception exception)
            {
                Logs.Log.WriteLogErrors(robotsTxtUrl, exception);
            }
            return false;
        }

        public bool SetIpAddress()
        {
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
            catch (Exception exception)
            {
                Logs.Log.WriteLogErrors(Host, exception);
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
                Robots ??= Robots.Load(RobotsTxt);
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
            Pages.Delete(this);
            return DeleteWebsite();
        }

        public void DeleteListings()
        {
            int counter = 0;
            var pages = GetPages();
            foreach (var page in pages)
            {
                var listing = ES_Listings.GetListing(page, false);
                if (listing != null)
                {
                    if (ES_Listings.Delete(page.Website, listing) &&
                        ES_Media.Delete(listing))
                    {
                        counter++;
                    }
                }
            }
            Console.WriteLine("Deleted " + counter + " listings");
        }

        private bool DeleteWebsite()
        {
            string query =
               "DELETE FROM " + Websites.TABLE_WEBSITES + " " +
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

        public void InsertPagesFromSiteMap()
        {
            if (!Config.INDEXER_ENABLED)
            {
                return;
            }
            var sitemaps = GetSiteMaps();
            if (sitemaps != null && sitemaps.Count > 0)
            {
                new SitemapIndexer(this).InsertSitemaps(sitemaps);
            }
            else
            {
                var uri = GetDefaultSiteMap();
                if (uri != null)
                {
                    new SitemapIndexer(this).InsertSitemap(uri);
                }
            }
        }

        private Uri? GetDefaultSiteMap()
        {
            Uri.TryCreate(MainUri, "sitemap.xml", out Uri? uri);
            return uri;
        }

        public List<Sitemap>? GetSiteMaps()
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
            return Pages.GetPages(this);
        }

        public List<Page> GetPagesUnknowPageType()
        {
            return Pages.GetPagesUnknowPageType(this);
        }

        public List<Page> GetNonScrapedPages()
        {
            return Pages.GetNonScrapedPages(this);
        }

        public List<Page> GetUnknowIsListingPages()
        {
            return Pages.GetUnknowIsListingPages(this);
        }

        public List<Page> GetIsNotListingPages()
        {
            return Pages.GetIsNotListingPages(this);
        }

        public int GetNumPages()
        {
            return NumPages;
        }

        public bool SetNumPagesToZero()
        {
            NumPages = 0;
            return UpdateNumPages();
        }

        public bool IncreaseNumPages()
        {
            NumPages++;
            return UpdateNumPages();
        }

        public bool DecreaseNumPages()
        {
            NumPages--;
            if (NumPages < 0)
            {
                NumPages = 0;
            }
            return UpdateNumPages();
        }

        public bool UpdateNumPages()
        {
            string query =
                "UPDATE " + Websites.TABLE_WEBSITES + " " +
                "SET [NumPages] = @NumPages " +
                "WHERE [Host] = @Host";

            return new DataBase().Query(query, new Dictionary<string, object?> {
                {"Host", Host },
                {"NumPages", NumPages }
            });
        }

        public int GetNumListings()
        {
            return NumListings;
        }

        public bool SetNumListingsToZero()
        {
            NumListings = 0;
            return UpdateNumListings();
        }

        public bool IncreaseNumListings()
        {
            NumListings++;
            return UpdateNumListings();
        }

        public bool DecreaseNumListings()
        {
            NumListings--;
            if (NumListings < 0)
            {
                NumListings = 0;
            }
            return UpdateNumListings();
        }

        public bool UpdateNumListings()
        {
            string query =
                "UPDATE " + Websites.TABLE_WEBSITES + " " +
                "SET [NumListings] = @NumListings " +
                "WHERE [Host] = @Host";

            return new DataBase().Query(query, new Dictionary<string, object?> {
                {"Host", Host },
                {"NumListings", NumListings }
            });
        }
    }
}