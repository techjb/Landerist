using landerist_library.Websites;
using Louw.SitemapParser;

namespace landerist_library.Index
{
    public class SitemapIndexer : Indexer
    {
        public SitemapIndexer(Website website) : base(website) { }

        public void InsertSitemaps(List<Com.Bekijkhet.RobotsTxt.Sitemap> sitemaps)
        {
            foreach (var sitemap in sitemaps)
            {
                InsertSitemap(sitemap.Url);
            }
        }

        public void InsertSitemap(Uri uri)
        {
            var sitemap = new Sitemap(uri);
            InsertSitemap(sitemap);
        }

        private void InsertSitemap(Sitemap sitemap)
        {
            if (!IsValidSitemap(sitemap))
            {
                return;
            }
            if (!sitemap.IsLoaded)
            {
                sitemap = DownloadSitemap(sitemap);
            }
            if (!sitemap.IsLoaded)
            {
                return;
            }
            if (sitemap.SitemapType == SitemapType.Index)
            {
                foreach (var indexSiteMap in sitemap.Sitemaps)
                {
                    InsertSitemap(indexSiteMap);
                }
            }
            else if (sitemap.SitemapType == SitemapType.Items)
            {
                foreach (var item in sitemap.Items)
                {
                    InsertUri(item.Location);
                }
            }
        }

        private static Sitemap DownloadSitemap(Sitemap siteMap)
        {
            try
            {
                siteMap = Task.Run(async () => await siteMap.LoadAsync()).Result;
            }
            catch { }
            return siteMap;
        }

        private static bool IsValidSitemap(Sitemap sitemap)
        {
            return !LanguageValidator.ContainsNotAllowed(sitemap.SitemapLocation, Page.Website.LanguageCode);
        }
    }
}
