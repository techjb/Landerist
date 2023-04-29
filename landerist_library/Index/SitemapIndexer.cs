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
                var lowSitemap = new Sitemap(sitemap.Url);
                InsertSitemap(lowSitemap);
            }
        }

        public void InsertSitemap(Sitemap siteMap)
        {
            if (!siteMap.IsLoaded)
            {
                siteMap = Task.Run(async () => await siteMap.LoadAsync()).Result;                
            }
            if (!siteMap.IsLoaded)
            {
                return;
            }
            if (siteMap.SitemapType == SitemapType.Index)
            {
                foreach (var indexSiteMap in siteMap.Sitemaps)
                {
                    if (Languages.ContainsNotAllowedES(indexSiteMap.SitemapLocation))
                    {
                        continue;
                    }
                    InsertSitemap(indexSiteMap);
                }                
            }
            else if (siteMap.SitemapType == SitemapType.Items)
            {
                foreach (var item in siteMap.Items)
                {
                    InsertUri(item.Location);
                }
            }
        }
    }
}
