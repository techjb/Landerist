using landerist_library.Websites;
using Louw.SitemapParser;

namespace landerist_library.Index
{
    public class SitemapIndexer : Indexer
    {
        private static readonly ISitemapFetcher SitemapFetcher = new GzipAwareSitemapFetcher();
        private static readonly ISitemapParser SitemapParser = new SitemapParser();
        private readonly HashSet<string> SitemapsIndexes = new(StringComparer.OrdinalIgnoreCase);

        public SitemapIndexer(Website website) : base(website)
        {
        }

        public bool IndexNewPages(List<Com.Bekijkhet.RobotsTxt.Sitemap> sitemaps)
        {
            if (sitemaps == null || sitemaps.Count == 0)
            {
                return false;
            }

            bool insertedAnyPage = false;
            foreach (var sitemap in sitemaps)
            {
                if (!CanAddMoreSitemaps())
                {
                    break;
                }

                if (Page.Website.IsDiscardedBySitemapUrlRegex(sitemap.Url))
                {
                    continue;
                }

                insertedAnyPage |= IndexNewPages(sitemap.Url);
            }

            return insertedAnyPage;
        }

        private int GetMaxSiteMapsPerWebsite()
        {
            if (Page.Website.ApplySpecialRules)
            {
                return 1000;
            }
            return 10;
        }

        public bool IndexNewPages(Uri uri)
        {
            if (uri == null)
            {
                return false;
            }

            return IndexNewPages(new Sitemap(uri));
        }

        private bool IndexNewPages(Sitemap? sitemap)
        {
            if (sitemap == null)
            {
                return false;
            }
            if (!IsValidSitemap(sitemap))
            {
                return false;
            }

            if (!RegisterSitemap(sitemap.SitemapLocation))
            {
                return false;
            }

            if (!sitemap.IsLoaded)
            {
                sitemap = DownloadSitemap(sitemap);
            }

            if (sitemap == null || !sitemap.IsLoaded)
            {
                return false;
            }

            bool insertedAnyPage = false;
            if (sitemap.SitemapType == SitemapType.Index)
            {
                foreach (var sitemapIndex in sitemap.Sitemaps)
                {
                    if (!CanAddMoreSitemaps())
                    {
                        break;
                    }
                    if (Page.Website.IsDiscardedBySitemapUrlRegex(sitemapIndex.SitemapLocation))
                    {
                        continue;
                    }

                    insertedAnyPage |= IndexNewPages(sitemapIndex);
                }
            }
            else if (sitemap.SitemapType == SitemapType.Items)
            {
                foreach (var item in sitemap.Items)
                {
                    insertedAnyPage |= InsertUri(item.Location);
                }
            }

            return insertedAnyPage;
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
