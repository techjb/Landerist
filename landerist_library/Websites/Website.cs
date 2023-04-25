using Com.Bekijkhet.RobotsTxt;
using landerist_library.Configuration;
using landerist_library.Database;
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

        public Website()
        {
            MainUri = new Uri("about:blank", UriKind.RelativeOrAbsolute);
        }

        public Website(Uri mainUri) : this()
        {
            MainUri = mainUri;
            Host = mainUri.Host;
            var dataRow = GetDataRow();
            if (dataRow != null)
            {
                Load(dataRow);
            }
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
                "WHERE [MainUri] = @MainUri";

            var dataTable = new DataBase().QueryTable(query, new Dictionary<string, object?> {
                {"MainUri", MainUri.ToString() }
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

            return new DataBase().Query(query, new Dictionary<string, object?> {
                {"MainUri", MainUri.ToString() },
                {"Host", Host },
                {"RobotsTxt", RobotsTxt },
                {"IpAddress", IpAddress },
                {"HttpStatusCode", HttpStatusCode },
            });
        }

        public bool UpdateRobotsTxt(string? robotsTxt)
        {
            RobotsTxt = robotsTxt;

            string query =
                "UPDATE " + Websites.TABLE_WEBSITES + " " +
                "SET RobotsTxt =  @RobotsTxt " +
                "WHERE Host = @Host";

            return new DataBase().Query(query, new Dictionary<string, object?> {
                {"RobotsTxt", RobotsTxt },
                {"Host", Host }
            });
        }

        public bool UpdateIpAddress(string? ipAddress)
        {
            IpAddress = ipAddress;

            string query =
                "UPDATE " + Websites.TABLE_WEBSITES + " " +
                "SET IpAddress =  @IpAddress " +
                "WHERE Host = @Host";

            return new DataBase().Query(query, new Dictionary<string, object?> {
                {"IpAddress", IpAddress },
                {"Host", Host }
            });
        }

        public bool UpdateMainUri(Uri mainUri)
        {
            MainUri = mainUri;

            string query =
                "UPDATE " + Websites.TABLE_WEBSITES + " " +
                "SET MainUri =  @MainUri " +
                "WHERE Host = @Host";

            return new DataBase().Query(query, new Dictionary<string, object?> {
                {"MainUri", MainUri.ToString() },
                {"Host", Host },
            });
        }

        public bool UpdateHttpStatusCode(short httpStatusCode)
        {
            HttpStatusCode = httpStatusCode;

            string query =
                "UPDATE " + Websites.TABLE_WEBSITES + " " +
                "SET HttpStatusCode =  @HttpStatusCode " +
                "WHERE Host = @Host";

            return new DataBase().Query(query, new Dictionary<string, object?> {
                {"HttpStatusCode", HttpStatusCode },
                {"Host", Host }
            });
        }

        public void UpdateHttpStatusCode()
        {
            HttpClientHandler handler = new()
            {
                AllowAutoRedirect = false
            };
            using var client = new HttpClient(handler);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(Config.USER_AGENT);
            HttpRequestMessage request = new(HttpMethod.Head, MainUri);

            var response = client.SendAsync(request).GetAwaiter().GetResult();
            var statusCode = response.StatusCode;
            UpdateHttpStatusCode((short)statusCode);
        }

        public Uri? GetResponseLocation()
        {
            HttpClientHandler handler = new()
            {
                AllowAutoRedirect = false
            };

            using var client = new HttpClient(handler);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(Config.USER_AGENT);
            HttpRequestMessage request = new(HttpMethod.Head, MainUri);

            var response = client.SendAsync(request).GetAwaiter().GetResult();
            return response.Headers.Location;
        }

        public bool UpdateRobotsTxt()
        {
            var httpClient = new HttpClient();
            var robotsTxtUrl = new Uri(MainUri, "/robots.txt");
            var response = httpClient.GetAsync(robotsTxtUrl).GetAwaiter().GetResult();
            string? robotsTxt = null;
            if (response.IsSuccessStatusCode)
            {
                robotsTxt = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }
            return UpdateRobotsTxt(robotsTxt);
        }

        public bool UpdateIpAddress()
        {
            IPAddress[] ipAddresses = Dns.GetHostAddresses(Host);
            string? ipAdress = null;
            if (ipAddresses.Length > 0)
            {
                ipAdress = ipAddresses[0].ToString();
            }
            return UpdateIpAddress(ipAdress);
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

        public bool Remove()
        {
            RemoveListings();
            Pages.Remove(this);
            return RemoveWebsite();
        }

        public void RemoveListings()
        {
            int counter = 0;
            var pages = GetPages();
            foreach (var page in pages)
            {
                var listing = ES_Listings.GetListing(page, false);
                if (listing != null)
                {
                    if (ES_Listings.Remove(listing))
                    {
                        counter++;
                    }
                }
            }
            Console.WriteLine("Removed " + counter + " listings");
        }

        private bool RemoveWebsite()
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

        public List<Page> GetPages()
        {
            return new Pages().GetPages(this);
        }

        public List<Page> GetNonScrapedPages()
        {
            return new Pages().GetNonScrapedPages(this);
        }

        public List<Page> GetUnknowIsListingPages()
        {
            return new Pages().GetUnknowIsListingPages(this);
        }

        public List<Page> GetIsNotListingPages()
        {
            return new Pages().GetIsNotListingPages(this);
        }
    }
}