using landerist_library.Websites;

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

        public List<Uri> GetUris()
        {
            if (Page.HtmlDocument == null)
            {
                return Uris;
            }
            try
            {
                var links = Page.HtmlDocument.DocumentNode.Descendants("a")
                   .Where(a => !a.Attributes["rel"]?.Value.Contains("nofollow") ?? true)
                   .Select(a => a.Attributes["href"]?.Value)
                   .Where(href => !string.IsNullOrWhiteSpace(href))
                   .ToList();

                if (links != null)
                {
                    GetUris(links);
                }
            }
            catch (Exception ex)
            {

            }
            return Uris;
        }

        private void GetUris(List<string?> links)
        {
            links = links.Distinct().ToList();
            foreach (var link in links)
            {
                if (link != null)
                {
                    AddUri(link);
                }
            }
        }

        private void AddUri(string link)
        {
            try
            {
                var uri = GetUri(link);
                if (uri != null && !Uris.Contains(uri))
                {
                    Uris.Add(uri);
                }
            }
            catch (Exception ex)
            {

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
            //UriBuilder uriBuilder = new(uri) // para construirlo con parámetros ?param1=values..
            //{
            //    Fragment = ""
            //};
            UriBuilder uriBuilder = new()
            {
                Fragment = "",
                Scheme = uri.Scheme,
                Host = uri.Host,
                Port = uri.Port,
                Path = uri.AbsolutePath
            };
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

        private bool IsMultimediaPage(Uri uri)
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
