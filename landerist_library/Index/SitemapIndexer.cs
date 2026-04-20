using landerist_library.Websites;
using Louw.SitemapParser;

namespace landerist_library.Index
{
    public class SitemapIndexer(Website website) : Indexer(website)
    {
        private static readonly ISitemapFetcher SitemapFetcher = new GzipAwareSitemapFetcher();
        private static readonly ISitemapParser SitemapParser = new SitemapParser();
        private readonly HashSet<string> SitemapsIndexes = new(StringComparer.OrdinalIgnoreCase);

        public void InsertSitemaps(List<Com.Bekijkhet.RobotsTxt.Sitemap> sitemaps)
        {
            if (sitemaps == null || sitemaps.Count == 0)
            {
                return;
            }

            sitemaps = [.. sitemaps.Take(GetMaxSiteMapsPerWebsite())];
            foreach (var sitemap in sitemaps)
            {
                InsertSitemap(sitemap.Url);
            }
        }

        private int GetMaxSiteMapsPerWebsite()
        {
            if (website.ApplySpecialRules)
            {
                return 1000;
            }
            return 10;
        }

        public void InsertSitemap(Uri uri)
        {
            if (uri == null)
            {
                return;
            }

            InsertSitemap(new Sitemap(uri));
        }

        private void InsertSitemap(Sitemap? sitemap)
        {
            if (sitemap == null)
            {
                return;
            }
            if (!IsValidSitemap(sitemap))
            {
                return;
            }

            if (!RegisterSitemap(sitemap.SitemapLocation))
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
                    if (!CanAddMoreSitemaps())
                    {
                        break;
                    }

                    InsertSitemap(sitemapIndex);
                }
            }
            else if (sitemap.SitemapType == SitemapType.Items)
            {
                foreach (var item in sitemap.Items)
                {
                    if (website.IsDiscardedBySitemapUrlRegex(item.Location))
                    {
                        continue;
                    }

                    InsertUri(item.Location);
                }
            }
        }

        private bool CanAddMoreSitemaps()
        {
            return SitemapsIndexes.Count < GetMaxSiteMapsPerWebsite();
        }

        private bool RegisterSitemap(Uri location)
        {
            if (location == null || !CanAddMoreSitemaps())
            {
                return false;
            }

            return SitemapsIndexes.Add(location.ToString());
        }

        private static Sitemap? DownloadSitemap(Sitemap siteMap)
        {
            if (siteMap == null)
            {
                return null;
            }

            try
            {
                // can return null.
                return siteMap.LoadAsync(SitemapFetcher, SitemapParser).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch
            {
                return null;
            }
        }

        private bool IsValidSitemap(Sitemap sitemap)
        {
            if (sitemap?.SitemapLocation == null)
            {
                return false;
            }

            return !LanguageValidator.ContainsNotAllowed(sitemap.SitemapLocation, Page.Website.LanguageCode);
        }
    }
}
