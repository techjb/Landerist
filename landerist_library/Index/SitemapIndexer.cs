using landerist_library.Websites;
using Louw.SitemapParser;

namespace landerist_library.Index
{
    public class SitemapIndexer : Indexer
    {
        private static readonly ISitemapParser SitemapParser = new SitemapParser();
        private readonly ISitemapFetcher WebsiteSitemapFetcher;
        private readonly HashSet<string> SitemapsIndexes = new(StringComparer.OrdinalIgnoreCase);

        private sealed record LoadedSitemap(Sitemap Sitemap, IReadOnlyList<Uri> AlternateUrls);

        public SitemapIndexer(Website website) : base(website)
        {
            WebsiteSitemapFetcher = new GzipAwareSitemapFetcher(website.BrowserUserAgent, website.UseProxy);
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

            IReadOnlyList<Uri> alternateUrls = [];
            if (!sitemap.IsLoaded)
            {
                var loadedSitemap = DownloadSitemap(sitemap);
                sitemap = loadedSitemap?.Sitemap;
                alternateUrls = loadedSitemap?.AlternateUrls ?? [];
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

                foreach (var alternateUrl in alternateUrls)
                {
                    insertedAnyPage |= InsertUri(alternateUrl);
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

        private LoadedSitemap? DownloadSitemap(Sitemap siteMap)
        {
            if (siteMap == null)
            {
                return null;
            }

            try
            {
                var content = WebsiteSitemapFetcher.Fetch(siteMap.SitemapLocation).ConfigureAwait(false).GetAwaiter().GetResult();
                var sitemap = SitemapParser.Parse(content, siteMap.SitemapLocation);
                var alternateUrls = GetAlternateUrls(content, siteMap.SitemapLocation);

                return new LoadedSitemap(sitemap, alternateUrls);
            }
            catch
            {
                return null;
            }
        }

        private static IReadOnlyList<Uri> GetAlternateUrls(string content, Uri sitemapLocation)
        {
            if (string.IsNullOrWhiteSpace(content) || sitemapLocation == null)
            {
                return [];
            }

            try
            {
                var document = System.Xml.Linq.XDocument.Parse(content);
                HashSet<string> urls = new(StringComparer.OrdinalIgnoreCase);
                List<Uri> alternateUrls = [];

                foreach (var element in document.Descendants())
                {
                    if (!string.Equals(element.Name.LocalName, "link", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var rel = element.Attribute("rel")?.Value;
                    if (string.IsNullOrWhiteSpace(rel) ||
                        !rel.Contains("alternate", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var href = element.Attribute("href")?.Value;
                    if (string.IsNullOrWhiteSpace(href) ||
                        !Uri.TryCreate(sitemapLocation, href, out Uri? alternateUrl))
                    {
                        continue;
                    }

                    if (urls.Add(alternateUrl.ToString()))
                    {
                        alternateUrls.Add(alternateUrl);
                    }
                }

                return alternateUrls;
            }
            catch
            {
                return [];
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
