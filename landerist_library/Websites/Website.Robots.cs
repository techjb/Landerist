using Com.Bekijkhet.RobotsTxt;
using landerist_library.Configuration;
using System.Net;
using System.Text;

namespace landerist_library.Websites
{
    public partial class Website
    {
        public bool SetRobotsTxt()
        {
            RobotsTxtUpdated = DateTime.Now;

            var robotsTxtUrl = new Uri(MainUri, "/robots.txt");

            try
            {
                using var httpClient = GetRobotsTxtHttpClient();
                using var request = CreateHttpRequestMessage(HttpMethod.Get, robotsTxtUrl);

                var response = httpClient.SendAsync(request).GetAwaiter().GetResult();
                RobotsTxt = null;
                Robots = null;

                if (response.StatusCode == HttpStatusCode.OK)
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
                Robots ??= Robots.Load(RobotsTxt);
                return Robots.IsPathAllowed(BrowserUserAgent, uri.PathAndQuery);
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
                return (int)Robots.CrawlDelay(BrowserUserAgent) / 1000;
            }

            return 0;
        }

        public bool CrawlDelayTooBig()
        {
            var crawlDelay = CrawlDelay();
            return crawlDelay > Config.MAX_CRAW_DELAY_SECONDS;
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

        private HttpClient GetRobotsTxtHttpClient()
        {
            if (!UseProxy)
            {
                return new HttpClient();
            }

            HttpClientHandler handler = new()
            {
                UseProxy = true,
                Proxy = new WebProxy(PrivateConfig.PROXY_HOST, GetProxyPort())
                {
                    Credentials = new NetworkCredential(
                        PrivateConfig.PROXY_USERNAME,
                        PrivateConfig.PROXY_PASSWORD)
                }
            };

            return new HttpClient(handler);
        }

        private static int GetProxyPort()
        {
            if (!PrivateConfig.PROXY_RANDOMIZE_STICKY_PORTS ||
                PrivateConfig.PROXY_STICKY_PORT_MIN > PrivateConfig.PROXY_STICKY_PORT_MAX)
            {
                return int.Parse(PrivateConfig.PROXY_PORT);
            }

            return Random.Shared.Next(
                PrivateConfig.PROXY_STICKY_PORT_MIN,
                PrivateConfig.PROXY_STICKY_PORT_MAX + 1);
        }
    }
}
