using landerist_library.Configuration;
using landerist_library.Pages;
using landerist_library.Index;

namespace landerist_library.Websites
{
    public partial class Website
    {
        public void ReadSitemap(Func<Page, bool>? insertPage = null, Func<Website, bool>? achievedMaxNumberOfPages = null)
        {
            SitemapUpdated = DateTime.Now;

            try
            {
                if (Config.INDEXER_ENABLED)
                {
                    bool indexedFromRobotsTxt = false;
                    var sitemapIndexer = new SitemapIndexer(this, insertPage, achievedMaxNumberOfPages);
                    var sitemaps = GetSiteMapsFromRobotsTxt();
                    if (sitemaps != null && sitemaps.Count > 0)
                    {
                        indexedFromRobotsTxt = sitemapIndexer.IndexNewPages(sitemaps);
                    }

                    if (!indexedFromRobotsTxt)
                    {
                        var uri = GetDefaultSiteMap();
                        if (uri != null)
                        {
                            sitemapIndexer.IndexNewPages(uri);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Logs.Log.WriteError("Website InsertPagesFromSiteMap", Host, exception);
            }
        }

        private Uri? GetDefaultSiteMap()
        {
            Uri.TryCreate(MainUri, "sitemap.xml", out Uri? uri);
            return uri;
        }
    }
}
