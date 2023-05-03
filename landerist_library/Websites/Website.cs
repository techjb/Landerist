using Com.Bekijkhet.RobotsTxt;
using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Index;
using System.Data;
using System.Net;

namespace landerist_library.Websites
{
    public class Website
    {
        public Uri MainUri { get; set; }

        public string Host { get; set; } = string.Empty;

        public string? RobotsTxt { get; set; }

        public string? IpAddress { get; set; }

        public short? HttpStatusCode { get; set; }


        private Robots? Robots = null;

        
        public string Language = "es";

        public Website()
        {
            MainUri = new Uri("about:blank", UriKind.RelativeOrAbsolute);
        }

        public Website(Uri mainUri) : this()
        {
            MainUri = mainUri;
            SetHost();
            var dataRow = GetDataRow();
            if (dataRow != null)
            {
                Load(dataRow);
            }
        }

        private void SetHost()
        {
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
                {"MainUri", MainUri.Host }
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
            RobotsTxt = dataRow["RobotsTxt"] is DBNull ? null : dataRow["RobotsTxt"].ToString();
            IpAddress = dataRow["IpAddress"] is DBNull ? null : dataRow["IpAddress"].ToString();
            HttpStatusCode = dataRow["HttpStatusCode"] is DBNull ? null : (short)dataRow["HttpStatusCode"];
        }

        public bool Insert()
        {
            string query =
                "INSERT INTO " + Websites.TABLE_WEBSITES + " " +
                "VALUES (@MainUri, @Host, @RobotsTxt, @IpAddress, @HttpStatusCode)";

            var parameters = GetQueryParameters();
            return new DataBase().Query(query, parameters);
        }

        public bool Update()
        {
            string query =
                "UPDATE " + Websites.TABLE_WEBSITES + " SET " +
                "[MainUri] = @MainUri, " +
                "[RobotsTxt] = @RobotsTxt, " +
                "[IpAddress] = @IpAddress, " +
                "[HttpStatusCode] = @HttpStatusCode " +
                "WHERE [Host] = @Host";

            var parameters = GetQueryParameters();
            return new DataBase().Query(query, parameters);
        }

        private Dictionary<string, object?> GetQueryParameters()
        {
            return new Dictionary<string, object?> {
                {"MainUri", MainUri.ToString() },
                {"Host", Host },
                {"RobotsTxt", RobotsTxt },
                {"IpAddress", IpAddress },
                {"HttpStatusCode", HttpStatusCode },
            };
        }

        public bool SetMainUriAndStatusCode(int iteration = 0)
        {
            HttpClientHandler handler = new()
            {
                AllowAutoRedirect = false
            };

            using var client = new HttpClient(handler);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(Config.USER_AGENT);
            try
            {
                HttpRequestMessage request = new(HttpMethod.Head, MainUri);
                var response = client.SendAsync(request).GetAwaiter().GetResult();
                HttpStatusCode = (short)response.StatusCode;
                if (response != null && response.Headers != null && response.Headers.Location != null)
                {
                    var uriLocation = response.Headers.Location;
                    if (!uriLocation.Equals(MainUri))
                    {
                        MainUri = uriLocation;
                        SetHost();
                        iteration++;
                        if (iteration < 10)
                        {
                            SetMainUriAndStatusCode(iteration++);
                        }
                    }
                }
                return true;
            }
            catch { }
            return false;
        }

        public bool SetRobotsTxt()
        {
            try
            {
                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(Config.USER_AGENT);
                var robotsTxtUrl = new Uri(MainUri, "/robots.txt");
                var response = httpClient.GetAsync(robotsTxtUrl).GetAwaiter().GetResult();
                RobotsTxt = null;
                if (response.IsSuccessStatusCode)
                {
                    RobotsTxt = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                }
                return true;
            }
            catch { }
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
            catch { }
            return false;
        }

        public bool IsMainUriAllowed()
        {
            return IsUriAllowed(MainUri);
        }

        public bool IsUriAllowed(Uri uri)
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

        public long CrawlDelay()
        {
            if (RobotsTxt != null)
            {
                Robots ??= Robots.Load(RobotsTxt);
                return Robots.CrawlDelay(Config.USER_AGENT);
            }
            return 0;
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
                    if (ES_Listings.Delete(listing))
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
                {"MainUri", MainUri.ToString() },
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
            var sitemaps = GetSiteMaps();
            if (sitemaps == null || sitemaps.Count.Equals(0))
            {
                var uri = GetDefaultSiteMap();
                if (uri != null)
                {
                    new SitemapIndexer(this).InsertSitemap(uri);
                }
                return;
            }
            new SitemapIndexer(this).InsertSitemaps(sitemaps);
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
    }
}