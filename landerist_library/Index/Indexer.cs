using landerist_library.Websites;
using Newtonsoft.Json.Linq;

namespace landerist_library.Index
{
    public class Indexer
    {
        private readonly Page Page;

        private readonly List<Uri> Uris = new();

        private static readonly string[] MultimediaExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".mp3", ".mp4", ".avi", ".mov", ".mkv", ".flv", ".ogg", ".webm" };

        private static readonly string[] WebPageExtensions = { ".htm", ".html", ".xhtml", ".asp", ".aspx", ".php", ".jsp", ".cshtml", ".vbhtml", "razor" };

        public Indexer(Page page)
        {
            Page = page;
        }

        public void InsertPageUrls()
        {
            if (Page.HtmlDocument == null)
            {
                return;
            }
            try
            {
                var urls = Page.HtmlDocument.DocumentNode.Descendants("a")
                   .Where(a => !a.Attributes["rel"]?.Value.Contains("nofollow") ?? true)
                   .Select(a => a.Attributes["href"]?.Value)
                   .Where(href => !string.IsNullOrWhiteSpace(href))
                   .ToList();

                if (urls != null)
                {
                    AddUrls(urls);
                }
            }
            catch (Exception ecception)
            {
                Logs.Log.WriteLogErrors(Page.Uri, ecception);
            }
            Pages.Insert(Page.Website, Uris);
        }

        public void InsertUrl(string url)
        {
            AddUri(url);
            Pages.Insert(Page.Website, Uris);
        }

        private void AddUrls(List<string?> urls)
        {
            urls = urls.Distinct().ToList();
            foreach (var url in urls)
            {
                if (url != null)
                {
                    AddUri(url);
                }
            }
        }

        private void AddUri(string url)
        {
            try
            {
                var uri = GetUri(url);
                if (uri != null && !Uris.Contains(uri))
                {
                    Uris.Add(uri);
                }
            }
            catch (Exception ecception)
            {
                Logs.Log.WriteLogErrors(Page.Uri, ecception);
            }
        }

        private Uri? GetUri(string? link)
        {
            if (link == null)
            {
                return null;
            }
            if (!Uri.TryCreate(Page.Uri, link, out Uri? uri))
            {
                return null;
            }
            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            {
                return null;
            }
            // build without fragments #fragment1..
            UriBuilder uriBuilder = new(uri) // 
            {
                Fragment = ""
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
            uri = uriBuilder.Uri;
            if (ProhibitedWords.Contains(uri))
            {
                return null;
            }
            if (!IsWebPage(uri))
            {
                return null;
            }
            if (Languages.ContainsNotAllowed(uri, "es"))
            {
                return null;
            }
            if (!uri.Host.Equals(Page.Host) || uri.Equals(Page.Uri))
            {
                return null;
            }
            if (Page.Website != null)
            {
                if (!Page.Website.IsUriAllowed(uri))
                {
                    return null;
                }
                if (Page.Website.MainUri.Equals(uri))
                {
                    return null;
                }
            }
            return uri;
        }

        private static bool IsMultimediaPage(Uri uri)
        {
            var path = uri.AbsolutePath.ToLower();
            var extension = Path.GetExtension(path);

            return MultimediaExtensions.Contains(extension);
        }

        public static bool IsWebPage(Uri uri)
        {
            string extension = Path.GetExtension(uri.AbsolutePath).Trim().ToLower();
            return extension.Equals(string.Empty) || WebPageExtensions.Contains(extension);
        }
    }
}
