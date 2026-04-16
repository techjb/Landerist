using landerist_library.Configuration;
using landerist_library.Pages;
using landerist_library.Tools;
using landerist_library.Websites;

namespace landerist_library.Index
{
    public class Indexer(Page page)
    {
        protected Page Page { get; } = page;

        private readonly HashSet<Uri> Inserted = [];

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

        public Indexer(Website website) : this(new Page(website))
        {
        }

        public void IndexPages()
        {
            if (!Config.INDEXER_ENABLED)
            {
                return;
            }

            if (Page.Website.AchievedMaxNumberOfPages())
            {
                return;
            }

            if (!string.IsNullOrEmpty(Page.RedirectUrl))
            {
                Insert(Page.RedirectUrl);
                return;
            }

            if (Page.ContainsMetaRobotsNoFollow())
            {
                return;
            }

            if (Page.PageType.Equals(PageType.IncorrectLanguage))
            {
                new LinkAlternateIndexer(Page).Insert();
                return;
            }

            if (Page.PageType.Equals(PageType.NotCanonical))
            {
                new CanonicalIndexer(Page).Insert();
                return;
            }

            new HyperlinksIndexer(Page).Insert();
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

        public Uri? GetUri(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            if (!Uri.TryCreate(Page.Uri, url, out Uri? uri))
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

        protected void InsertUri(Uri uri)
        {
            var website = Page.Website;
            if (website == null)
            {
                return;
            }

            if (website.AchievedMaxNumberOfPages())
            {
                return;
            }

            uri = Uris.CleanUri(uri);

            if (Inserted.Contains(uri))
            {
                return;
            }

            if (ProhibitedUrls.IsProhibited(uri, website.LanguageCode))
            {
                return;
            }

            if (!IsWebPage(uri))
            {
                return;
            }

            if (LanguageValidator.ContainsNotAllowed(uri, website.LanguageCode))
            {
                return;
            }

            if (!uri.Host.Equals(Page.Host, StringComparison.OrdinalIgnoreCase) || uri.Equals(Page.Uri))
            {
                return;
            }

            if (!website.IsAllowedByRobotsTxt(uri))
            {
                return;
            }

            if (website.MainUri.Equals(uri))
            {
                return;
            }

            if (!website.CanAddNewPages())
            {
                return;
            }

            Pages.Pages.Insert(website, uri);
            Inserted.Add(uri);
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
