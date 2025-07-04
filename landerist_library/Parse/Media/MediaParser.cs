﻿using HtmlAgilityPack;
using landerist_library.Parse.Media.Image;
using landerist_library.Parse.Media.Other;
using landerist_library.Parse.Media.Video;
using landerist_library.Websites;
using landerist_orels;
using landerist_orels.ES;

namespace landerist_library.Parse.Media
{
    public class MediaParser
    {
        public readonly Page Page;

        private readonly SortedSet<landerist_orels.Media> Media = new(new MediaComparer());

        public HtmlDocument? HtmlDocument { get; set; }

        public MediaParser(Page page)
        {
            Page = page;
            InitHtmlDocument();
        }

        public void Add(landerist_orels.Media media)
        {
            if (media.url == null)
            {
                return;
            }

            if (media.url.Scheme != Uri.UriSchemeHttp && media.url.Scheme != Uri.UriSchemeHttps)
            {
                return;
            }
            Media.Add(media);
        }

        public void AddMedia(Listing listing)
        {
            if (HtmlDocument == null)
            {
                return;
            }
            if (!Page.ContainsMetaRobotsNoImageIndex())
            {
                new ImageParser(this).AddImages();
            }
            //new VideoParser(this).GetVideos();
            //new OtherParser(this).GetOthers();
            listing.SetMedia(Media);
        }

        public void AddMediaImages(Listing listing, string[]? list)
        {
            if (list == null || list.Length.Equals(0))
            {
                return;
            }
            if (!Page.ContainsMetaRobotsNoImageIndex())
            {
                new ImageParserUrls(this).AddImagesFromUrls(list);
            }
            listing.SetMedia(Media);
        }

        public void AddMediaImages(Listing listing, List<(string url, string? title)>? list)
        {
            if (list == null || list.Count.Equals(0))
            {
                return;
            }
            if (!Page.ContainsMetaRobotsNoImageIndex())
            {
                new ImageParserUrls(this).AddImagesFromUrls(list);
            }
            listing.SetMedia(Media);
        }

        private void InitHtmlDocument()
        {
            HtmlDocument = Page.GetHtmlDocument();
            if (HtmlDocument == null || HtmlDocument.DocumentNode == null)
            {
                return;
            }
            var xPath =
                "//nav | //footer | //style | " +
                "//code | //canvas | //input | //option | " +
                "//select | //progress | //svg | //textarea | //del";

            List<HtmlNode>? nodesToRemove = null;

            try
            {
                var selectedNodes = HtmlDocument.DocumentNode.SelectNodes(xPath);
                if (selectedNodes != null)
                {
                    nodesToRemove = [.. selectedNodes];
                }
            }
            catch
            {
                // Handle exceptions if necessary
            }

            if (nodesToRemove is not null)
            {
                foreach (var node in nodesToRemove)
                {
                    node.Remove();
                }
            }
        }

        public static string GetTitle(HtmlNode imgNode)
        {
            string title = imgNode.GetAttributeValue("alt", "");
            if (string.IsNullOrEmpty(title))
            {
                title = imgNode.GetAttributeValue("title", "");                
            }

            return title;
        }
    }
}
