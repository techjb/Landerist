using landerist_library.Websites;
using Louw.SitemapParser;

namespace landerist_library.Index
{

    public class SitemapIndexer(Website website) : Indexer(website)
    {
        readonly HashSet<string> SitemapsIndexes = [];

        public void InsertSitemaps(List<Com.Bekijkhet.RobotsTxt.Sitemap> sitemaps)
        {
            sitemaps = [.. sitemaps.Take(Configuration.Config.MAX_SITEMAPS_PER_WEBSITE)];
            foreach (var sitemap in sitemaps)
            {
                InsertSitemap(sitemap.Url);
            }
        }

        public void InsertSitemap(Uri uri)
        {
            if (uri == null)
            {
                return;
            }

            var sitemap = new Sitemap(uri);
            InsertSitemap(sitemap);
        }

        private void InsertSitemap(Sitemap sitemap)
        {
            if (!IsValidSitemap(sitemap))
            {
                return;
            }

            if (!CannAddMoreSitemaps())
            {
                return;
            }

            if (!sitemap.IsLoaded)
            {
                sitemap = DownloadSitemap(sitemap);
            }
            if (sitemap == null || !sitemap.IsLoaded)
            {
                return;
            }
            if (sitemap.SitemapType == SitemapType.Index)
            {
                foreach (var sitemapIndex in sitemap.Sitemaps)
                {
                    if (!CannAddMoreSitemaps())
                    {
                        break;
                    }
                    if (SitemapsIndexes.Add(sitemapIndex.SitemapLocation.ToString()))
                    {
                        InsertSitemap(sitemapIndex);
                    }
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

        private bool CannAddMoreSitemaps()
        {
            return SitemapsIndexes.Count < Configuration.Config.MAX_SITEMAPS_PER_WEBSITE;
        }

        private static Sitemap DownloadSitemap(Sitemap siteMap)
        {
            try
            {
                // can return null.
                var task = Task.Run(() => siteMap.LoadAsync());
                if (task != null)
                {
                    try
                    {
                        siteMap = task.Result;
                    }
                    catch //(AggregateException ae)
                    {

                    }
                }
            }
            catch { }
            return siteMap;
        }

        private bool IsValidSitemap(Sitemap sitemap)
        {
            if (sitemap == null)
            {
                return false;
            }

            return !LanguageValidator.ContainsNotAllowed(sitemap.SitemapLocation, Page.Website.LanguageCode);
        }
    }
}
