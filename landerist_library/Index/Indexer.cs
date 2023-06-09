﻿using landerist_library.Websites;

namespace landerist_library.Index
{
    public class Indexer
    {
        protected static Page Page = new();

        private readonly HashSet<Uri> Inserted = new();

        private static readonly string[] MultimediaExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".mp3", ".mp4", ".avi", ".mov", ".mkv", ".flv", ".ogg", ".webm" };

        private static readonly string[] WebPageExtensions = { ".htm", ".html", ".xhtml", ".asp", ".aspx", ".php", ".jsp", ".cshtml", ".vbhtml", "razor" };

        public Indexer(Website website) : this(new Page(website))
        {

        }

        public Indexer(Page page)
        {
            Page = page;
        }

        public void InsertUrls(List<string?> urls)
        {
            urls = urls.Distinct().ToList();
            foreach (var url in urls)
            {
                if (!CanInsertNewUrls())
                {
                    break;
                }
                if (url != null)
                {
                    InsertUrl(url);
                }
            }
        }

        public void InsertUrl(string? link)
        {
            if (string.IsNullOrEmpty(link))
            {
                return;
            }
            if (!Uri.TryCreate(Page.Uri, link, out Uri? uri))
            {
                return;
            }
            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            {
                return;
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
            InsertUri(uriBuilder.Uri);
        }

        private static bool CanInsertNewUrls()
        {
            return Page.Website.GetNumPages() < Configuration.Config.MAX_PAGES_PER_WEBSITE;
        }

        protected void InsertUri(Uri uri)
        {
            if (!CanInsertNewUrls())
            {
                return;
            }
            
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

            if (!Page.Website.IsUriAllowed(uri))
            {
                return;
            }
            if (Page.Website.MainUri.Equals(uri))
            {
                return;
            }

            Pages.Insert(Page.Website, uri);
            Inserted.Add(uri);
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
