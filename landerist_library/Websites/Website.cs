using Com.Bekijkhet.RobotsTxt;
using System.Data;
using System.Net;

namespace landerist_library.Websites
{
    public class Website : Websites
    {
        public Uri MainUri { get; set; }

        public string Host { get; set; } = string.Empty;

        public string? RobotsTxt { get; set; }

        public string? IpAddress { get; set; }

        public short? HttpStatusCode { get; set; }

        
        private Robots? Robots = null;

        public Website()
        {
            MainUri = new Uri(string.Empty);
        }

        public Website(DataRow dataRow)
        {
            Load(dataRow);
        }

        private void Load(DataRow dataRow)
        {
            string mainUriString = dataRow["MainUri"].ToString()!;
            MainUri = new(mainUriString);
            Host = dataRow["Host"].ToString()!;
            RobotsTxt = dataRow["RobotsTxt"].ToString();
            IpAddress = dataRow["IpAddress"].ToString();
            HttpStatusCode = dataRow["HttpStatusCode"] is DBNull ? null : (short)dataRow["HttpStatusCode"];
        }

        public bool Insert()
        {
            string query =
                "INSERT INTO " + WEBSITES + " " +
                "VALUES (@MainUri, @Host, NULL, NULL, NULL)";

            return new Database().Query(query, new Dictionary<string, object> {
                {"MainUri", MainUri.ToString() },
                {"Host", Host }
            });
        }

        public bool UpdateRobotsTxt(string robotsTxt)
        {
            RobotsTxt = robotsTxt;
            if (RobotsTxt == null)
            {
                return true;
            }

            string query =
                "UPDATE " + WEBSITES + " " +
                "SET RobotsTxt =  @RobotsTxt " +
                "WHERE Host = @Host";

            return new Database().Query(query, new Dictionary<string, object> {
                {"RobotsTxt", RobotsTxt },
                {"Host", Host }
            });
        }

        public bool UpdateIpAddress(string ipAddress)
        {
            IpAddress = ipAddress;
            if (IpAddress == null)
            {
                return true;
            }
            string query =
                "UPDATE " + WEBSITES + " " +
                "SET IpAddress =  @IpAddress " +
                "WHERE Host = @Host";

            return new Database().Query(query, new Dictionary<string, object> {
                {"IpAddress", IpAddress },
                {"Host", Host }
            });
        }

        public bool UpdateMainUri(Uri mainUri)
        {
            MainUri = mainUri;

            string query =
                "UPDATE " + WEBSITES + " " +
                "SET MainUri =  @MainUri " +
                "WHERE Host = @Host";

            return new Database().Query(query, new Dictionary<string, object> {
                {"MainUri", MainUri.ToString() },
                {"Host", Host },
            });
        }

        public bool UpdateHttpStatusCode(short httpStatusCode)
        {
            HttpStatusCode = httpStatusCode;
            if (HttpStatusCode == null)
            {
                return true;
            }
            string query =
                "UPDATE " + WEBSITES + " " +
                "SET HttpStatusCode =  @HttpStatusCode " +
                "WHERE Host = @Host";

            return new Database().Query(query, new Dictionary<string, object> {
                {"HttpStatusCode", HttpStatusCode },
                {"Host", Host }
            });
        }

        public void SetHttpStatusCode()
        {
            HttpClientHandler handler = new()
            {
                AllowAutoRedirect = false
            };
            using var client = new HttpClient(handler);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(Scraper.ScraperBase.UserAgentChrome);
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
            client.DefaultRequestHeaders.UserAgent.ParseAdd(Scraper.ScraperBase.UserAgentChrome);
            HttpRequestMessage request = new(HttpMethod.Head, MainUri);

            var response = client.SendAsync(request).GetAwaiter().GetResult();
            return response.Headers.Location;
        }

        public bool SetRobotsTxt()
        {
            var httpClient = new HttpClient();
            var robotsTxtUrl = new Uri(MainUri, "/robots.txt");
            var response = httpClient.GetAsync(robotsTxtUrl).GetAwaiter().GetResult();
            if (response.IsSuccessStatusCode)
            {
                string robotsTxt = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                return UpdateRobotsTxt(robotsTxt);
            }
            return false;
        }

        public bool SetIpAddress()
        {
            IPAddress[] ipAddresses = Dns.GetHostAddresses(Host);
            if (ipAddresses.Length.Equals(0))
            {
                return false;
            }
            var ipAdress = ipAddresses[0].ToString();
            return UpdateIpAddress(ipAdress);
        }


        public bool CanAccessMainUri()
        {
            return CanAccess(MainUri);
        }


        public bool CanAccess(Uri uri)
        {
            if (RobotsTxt != null)
            {
                Robots ??= Robots.Load(RobotsTxt);
                return Robots.IsPathAllowed(Scraper.ScraperBase.UserAgentChrome, uri.PathAndQuery);
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
                return Robots.CrawlDelay(Scraper.ScraperBase.UserAgentChrome);
            }
            return 0;
        }
    }
}