using landerist_library.Configuration;
using landerist_library.Application.Websites;
using landerist_library.Pages;
using landerist_library.Tools;
using landerist_library.Websites;

namespace landerist_library.Infrastructure.Indexing
{
    public class Indexer(Page page, IWebsiteRobotsPolicy robots, Func<Page, bool>? insertPage = null, Func<Website, bool>? achievedMaxNumberOfPages = null)
    {
        protected Page Page { get; } = page;
        protected IWebsiteRobotsPolicy RobotsPolicy { get; } = robots;
        private readonly Func<Page, bool>? _insertPage = insertPage;
        private readonly Func<Website, bool> _achievedMaxNumberOfPages = achievedMaxNumberOfPages ?? (_ => false);

        private readonly HashSet<Uri> Processed = [];

        private static readonly HashSet<string> WebPageExtensions =
        [
            ".htm",
            ".html",
            ".xhtml",
            ".asp",
            ".aspx",
            ".php",
            ".jsp",
            ".cshtml",
            ".vbhtml",
            ".razor"
        ];

        public Indexer(Website website, IWebsiteRobotsPolicy robots, Func<Page, bool>? insertPage = null, Func<Website, bool>? achievedMaxNumberOfPages = null) : this(new Page(website), robots, insertPage, achievedMaxNumberOfPages)
        {
        }

        public void IndexPages()
        {
            if (!Config.INDEXER_ENABLED)
            {
                return;
            }


            if (_achievedMaxNumberOfPages(Page.Website))
            {
                return;
            }

            if (!string.IsNullOrEmpty(Page.RedirectUrl))
            {
                Insert(Page.RedirectUrl);
                return;
            }


            if (Page.PageType.Equals(PageType.IncorrectLanguage))
            {
                new LinkAlternateIndexer(Page, RobotsPolicy).Insert();
                return;
            }

            if (Page.PageType.Equals(PageType.NotCanonical))
            {
                new CanonicalIndexer(Page, RobotsPolicy).Insert();
                return;
            }

            if (Page.ContainsMetaRobotsNoFollow())
            {
                return;
            }

            if (Page.Website.HtmlIndexingEnabled)
            {
                new HyperlinksIndexer(Page, RobotsPolicy).Insert();
            }
        }

        public void Insert(List<string?> urls)
        {
            foreach (var url in new HashSet<string?>(urls))
            {
                if (url != null)
                {
                    Insert(url);
                }
            }
        }

        public void Insert(string? url)
        {
            var uri = GetUri(url);
            if (uri is not null)
            {
                InsertUri(uri);
            }
        }

        public Uri? GetUri(string? url) => GetUri(Page, url);

        public static Uri? GetUri(Page page, string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            if (!Uri.TryCreate(page.Uri, url, out Uri? uri))
            {
                return null;
            }

            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            {
                return null;
            }

            // build without fragments #fragment1..
            UriBuilder uriBuilder = new(uri)
            {
                Fragment = string.Empty
            };

            // build without parameters
            //UriBuilder uriBuilder = new()
            //{
            //    Fragment = "",
            //    Scheme = uri.Scheme,
            //    Host = uri.Host,
            //    Port = uri.Port,
            //    Path = uri.AbsolutePath
            //};
            //
            return uriBuilder.Uri;
        }

        public void Insert(Uri uri)
        {
            InsertUri(uri);
        }

        protected bool InsertUri(Uri uri)
        {
            var website = Page.Website;
            if (website == null)
            {
                return false;
            }

            if (_achievedMaxNumberOfPages(website))
            {
                return false;
            }

            if (website.IsDiscardedByIndexUrlRegex(uri))
            {
                return false;
            }

            uri = Uris.CleanUri(uri);

            if (Processed.Contains(uri))
            {
                return false;
            }

            if (ProhibitedUrls.IsProhibited(uri, website.LanguageCode))
            {
                return false;
            }

            if (!IsWebPage(uri))
            {
                return false;
            }

            //if (LanguageValidator.ContainsNotAllowed(uri, website.LanguageCode))
            //{
            //    return false;
            //}

            if (!uri.Host.Equals(Page.Host, StringComparison.OrdinalIgnoreCase) || uri.Equals(Page.Uri))
            {
                return false;
            }

            if (!RobotsPolicy.IsAllowed(website, uri))
            {
                return false;
            }

            if (website.MainUri.Equals(uri))
            {
                return false;
            }

            bool inserted = _insertPage?.Invoke(new Page(website, uri)) ?? false;
            Processed.Add(uri);
            return inserted;
        }

        //private static bool IsMultimediaPage(Uri uri)
        //{
        //    var path = uri.AbsolutePath.ToLower();
        //    var extension = Path.GetExtension(path);

        //    return MultimediaExtensions.Contains(extension);
        //}

        public static bool IsWebPage(Uri uri)
        {
            string extension = Path.GetExtension(uri.AbsolutePath);
            return string.IsNullOrEmpty(extension) || WebPageExtensions.Contains(extension);
        }
    }
}
