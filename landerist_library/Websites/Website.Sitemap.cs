using landerist_library.Configuration;
using landerist_library.Index;

namespace landerist_library.Websites
{
    public partial class Website
    {
        public void ReadSitemap()
        {
            SitemapUpdated = DateTime.Now;

            try
            {
                if (Config.INDEXER_ENABLED)
                {
                    bool indexedFromRobotsTxt = false;
                    var sitemapIndexer = new SitemapIndexer(this);
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
            finally
            {
                Update();
            }
        }

        private Uri? GetDefaultSiteMap()
        {
            Uri.TryCreate(MainUri, "sitemap.xml", out Uri? uri);
            return uri;
        }
    }
}
