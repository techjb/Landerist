using HtmlAgilityPack;
using landerist_library.Pages;
using landerist_library.Parse.Media.Image;
using landerist_orels;
using landerist_orels.ES;

namespace landerist_library.Parse.Media
{
    public class MediaParser
    {
        public readonly Page Page;

        private readonly SortedSet<landerist_orels.Media> _media = new(new MediaComparer());

        public HtmlDocument? HtmlDocument { get; private set; }

        public MediaParser(Page page)
        {
            ArgumentNullException.ThrowIfNull(page);

            Page = page;
            InitHtmlDocument();
        }

        public void Add(landerist_orels.Media media)
        {
            ArgumentNullException.ThrowIfNull(media);

            if (media.url == null)
            {
                return;
            }

            if (media.url.Scheme != Uri.UriSchemeHttp && media.url.Scheme != Uri.UriSchemeHttps)
            {
                return;
            }

            _media.Add(media);
        }

        public void AddMedia(Listing listing)
        {
            ArgumentNullException.ThrowIfNull(listing);

            if (HtmlDocument != null && !Page.ContainsMetaRobotsNoImageIndex())
            {
                new ImageParser(this).AddImages();
            }

            //new VideoParser(this).GetVideos();
            //new OtherParser(this).GetOthers();
            listing.SetMedia(_media);
        }

        public void AddMediaImages(Listing listing, string[]? list)
        {
            ArgumentNullException.ThrowIfNull(listing);

            if (list != null && list.Length > 0 && !Page.ContainsMetaRobotsNoImageIndex())
            {
                new ImageParserUrls(this).AddImagesFromUrls(list);
            }

            listing.SetMedia(_media);
        }

        public void AddMediaImages(Listing listing, List<(string url, string? title)>? list)
        {
            ArgumentNullException.ThrowIfNull(listing);

            if (list != null && list.Count > 0 && !Page.ContainsMetaRobotsNoImageIndex())
            {
                new ImageParserUrls(this).AddImagesFromUrls(list);
            }

            listing.SetMedia(_media);
        }

        private void InitHtmlDocument()
        {
            HtmlDocument = Page.GetHtmlDocument();
            if (HtmlDocument?.DocumentNode == null)
            {
                return;
            }

            const string xPath =
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

            if (nodesToRemove == null)
            {
                return;
            }

            foreach (var node in nodesToRemove)
            {
                node.Remove();
            }
        }

        public static string GetTitle(HtmlNode imgNode)
        {
            ArgumentNullException.ThrowIfNull(imgNode);

            var title = imgNode.GetAttributeValue("alt", "");
            if (string.IsNullOrWhiteSpace(title))
            {
                title = imgNode.GetAttributeValue("title", "");
            }

            return title;
        }
    }
}
