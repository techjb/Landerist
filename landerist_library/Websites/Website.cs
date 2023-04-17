using Com.Bekijkhet.RobotsTxt;
using landerist_library.Configuration;
using landerist_library.Database;
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
            MainUri = new Uri("about:blank", UriKind.RelativeOrAbsolute);
        }

        public Website(Uri mainUri):this()
        {
            MainUri = mainUri;
            Host = mainUri.Host;
            var dataRow = GetDataRow();
            if (dataRow != null)
            {
                Load(dataRow);
            }
        }

        public Website(DataRow dataRow): this()
        {
            Load(dataRow);
        }

        private DataRow? GetDataRow()
        {
            string query = 
                "SELECT * " +
                "FROM " + TABLE_WEBSITES + " " +
                "WHERE [MainUri] = @MainUri";

            var dataTable = new DataBase().QueryTable(query, new Dictionary<string, object?> {
                {"MainUri", MainUri.ToString() }                
            });

            if(dataTable.Rows.Count > 0)
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
            RobotsTxt = dataRow["RobotsTxt"].ToString();
            IpAddress = dataRow["IpAddress"].ToString();
            HttpStatusCode = dataRow["HttpStatusCode"] is DBNull ? null : (short)dataRow["HttpStatusCode"];
        }

        public bool Insert()
        {
            string query =
                "INSERT INTO " + TABLE_WEBSITES + " " +
                "VALUES (@MainUri, @Host, NULL, NULL, NULL)";

            return new DataBase().Query(query, new Dictionary<string, object?> {
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
                "UPDATE " + TABLE_WEBSITES + " " +
                "SET RobotsTxt =  @RobotsTxt " +
                "WHERE Host = @Host";

            return new DataBase().Query(query, new Dictionary<string, object?> {
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
                "UPDATE " + TABLE_WEBSITES + " " +
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
                "UPDATE " + TABLE_WEBSITES + " " +
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
            if (HttpStatusCode == null)
            {
                return true;
            }
            string query =
                "UPDATE " + TABLE_WEBSITES + " " +
                "SET HttpStatusCode =  @HttpStatusCode " +
                "WHERE Host = @Host";

            return new DataBase().Query(query, new Dictionary<string, object?> {
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
            Pages.Remove(this);

            string query =
               "DELETE FROM " + TABLE_WEBSITES + " " +
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
    }
}