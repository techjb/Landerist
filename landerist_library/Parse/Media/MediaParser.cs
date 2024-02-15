using HtmlAgilityPack;
using landerist_library.Parse.Media.Image;
using landerist_library.Parse.Media.Other;
using landerist_library.Parse.Media.Video;
using landerist_library.Websites;
using landerist_orels.ES;

namespace landerist_library.Parse.Media
{
    public class MediaParser
    {
        public readonly Page Page;

        public readonly SortedSet<landerist_orels.ES.Media> Media = new(new MediaComparer());

        public HtmlDocument? HtmlDocument { get; set; }

        public MediaParser(Page page)
        {
            Page = page;
            InitHtmlDocument();
        }

        public void AddMedia(landerist_orels.ES.Listing listing)
        {
            if (HtmlDocument == null)
            {
                return;
            }
            if (!Page.ContainsMetaRobotsNoImageIndex())
            {
                new ImageParser(this).AddImages();
            }
            new VideoParser(this).GetVideos();
            new OtherParser(this).GetOthers();
            listing.SetMedia(Media);
        }

        private void InitHtmlDocument()
        {
            HtmlDocument = Page.GetHtmlDocument();
            if (HtmlDocument == null)
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
                nodesToRemove = [.. HtmlDocument.DocumentNode.SelectNodes(xPath)];                 
            }
            catch
            {

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
            string title = imgNode.GetAttributeValue("alt", null);
            if (string.IsNullOrEmpty(title))
            {
                title = imgNode.GetAttributeValue("title", null);
            }

            return title;
        }
    }
}
