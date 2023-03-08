using System.Data;
using System.Net;

namespace landerist_library.Websites
{
    public class Website : Websites
    {
        public Uri Uri { get; set; }

        public string Domain { get; set; } = string.Empty;

        public string? RobotsTxt { get; set; }

        public string? IpAddress { get; set; }

        public short? HttpStatusCode { get; set; }

        public Website(Uri uri)
        {
            Uri = uri;
            Domain = uri.Host;
        }

        public Website(DataRow dataRow)
        {
            string uriString = dataRow["Uri"].ToString()!;
            Uri = new(uriString);
            Domain = dataRow["Domain"].ToString()!;
            RobotsTxt = dataRow["RobotsTxt"].ToString();
            IpAddress = dataRow["IpAddress"].ToString();
            HttpStatusCode = dataRow["HttpStatusCode"] is DBNull ? null : (short)dataRow["HttpStatusCode"];
        }

        public bool Insert()
        {
            string query =
                "INSERT INTO " + WEBSITES + " " +
                "VALUES (@Uri, @Domain, NULL, NULL, NULL)";

            return new Database().Query(query, new Dictionary<string, object> {
                {"Uri", Uri.ToString() },
                {"Domain", Domain }
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
                "WHERE Domain = @Domain";

            return new Database().Query(query, new Dictionary<string, object> {
                {"RobotsTxt", RobotsTxt },
                {"Domain", Domain }
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
                "WHERE Domain = @Domain";

            return new Database().Query(query, new Dictionary<string, object> {
                {"IpAddress", IpAddress },
                {"Domain", Domain }
            });
        }

        public bool UpdateUri(Uri uri)
        {
            Uri = uri;

            string query =
                "UPDATE " + WEBSITES + " " +
                "SET Uri =  @Uri " +
                "WHERE Domain = @Domain";

            return new Database().Query(query, new Dictionary<string, object> {
                {"Uri", Uri.ToString() },
                {"Domain", Domain },
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
                "WHERE Domain = @Domain";

            return new Database().Query(query, new Dictionary<string, object> {
                {"HttpStatusCode", HttpStatusCode },
                {"Domain", Domain }
            });
        }

        public void SetHttpStatusCode()
        {
            HttpClientHandler handler = new()
            {
                AllowAutoRedirect = false
            };
            using var client = new HttpClient(handler);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(Scraper.Scraper.UserAgentChrome);
            HttpRequestMessage request = new(HttpMethod.Head, Uri);

            var response = client.SendAsync(request).GetAwaiter().GetResult();
            var statusCode = response.StatusCode;            
            UpdateHttpStatusCode((short)statusCode);
        }

        public Uri? GetLocation()
        {
            HttpClientHandler handler = new()
            {
                AllowAutoRedirect = false
            };

            using var client = new HttpClient(handler);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(Scraper.Scraper.UserAgentChrome);
            HttpRequestMessage request = new(HttpMethod.Head, Uri);

            var response = client.SendAsync(request).GetAwaiter().GetResult();
            return response.Headers.Location;            
        }

        public bool SetRobotsTxt()
        {
            var httpClient = new HttpClient();
            var robotsTxtUrl = new Uri(Uri, "/robots.txt");
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
            IPAddress[] ipAddresses = Dns.GetHostAddresses(Domain);
            if (ipAddresses.Length.Equals(0))
            {
                return false;
            }
            var ipAdress = ipAddresses[0].ToString();
            return UpdateIpAddress(ipAdress);
        }
    }
}
