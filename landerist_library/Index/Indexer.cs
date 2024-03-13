using landerist_library.Parse.PageTypeParser;
using landerist_library.Tools;
using landerist_library.Websites;
using System;

namespace landerist_library.Index
{
    public class Indexer(Page page)
    {
        protected Page Page = page;

        private readonly HashSet<Uri> Inserted = [];

        //private static readonly string[] MultimediaExtensions = [".jpg", ".jpeg", ".png", ".gif", ".bmp", ".mp3", ".mp4", ".avi", ".mov", ".mkv", ".flv", ".ogg", ".webm"];

        private static readonly string[] WebPageExtensions = [".htm", ".html", ".xhtml", ".asp", ".aspx", ".php", ".jsp", ".cshtml", ".vbhtml", "razor"];

        public Indexer(Website website) : this(new Page(website))
        {

        }

        public void Insert(List<string?> urls)
        {
            urls = urls.Distinct().ToList();
            foreach (var url in urls)
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
            if (uri != null)
            {
                InsertUri(uri);
            }

        }

        public Uri? GetUri(string? url)
        {
            if (string.IsNullOrEmpty(url))
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
            //
            return uriBuilder.Uri;
        }

        public void Insert(Uri uri)
        {
            InsertUri(uri);
        }


        protected void InsertUri(Uri uri)
        {
            if (Page.Website.AchievedMaxNumberOfPages())
            {
                return;
            }

            uri = Uris.CleanUri(uri);

            if (Inserted.Contains(uri))
            {
                return;
            }
            if (ProhibitedUrls.IsProhibited(uri, Page.Website.LanguageCode))
            {
                return;
            }
            if (!IsWebPage(uri))
            {
                return;
            }
            if (LanguageValidator.ContainsNotAllowed(uri, Page.Website.LanguageCode))
            {
                return;
            }
            if (!uri.Host.Equals(Page.Host) || uri.Equals(Page.Uri))
            {
                return;
            }
            if (Page.Website == null)
            {
                return;
            }
            if (!Page.Website.IsAllowedByRobotsTxt(uri))
            {
                return;
            }
            if (Page.Website.MainUri.Equals(uri))
            {
                return;
            }
            if (!Page.Website.CanAddNewPages())
            {
                return;
            }

            Pages.Insert(Page.Website, uri);
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
            string extension = Path.GetExtension(uri.AbsolutePath).Trim().ToLower();
            return extension.Equals(string.Empty) || WebPageExtensions.Contains(extension);
        }
    }
}
