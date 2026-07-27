using Com.Bekijkhet.RobotsTxt;

namespace landerist_library.Websites
{
    public partial class Website
    {
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
            return crawlDelay > Rules.MaxCrawlDelaySeconds;
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

        internal void ResetParsedRobots() => Robots = null;
    }
}
