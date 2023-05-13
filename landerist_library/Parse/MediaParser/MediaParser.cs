using landerist_library.Websites;
using landerist_orels.ES;

namespace landerist_library.Parse.MediaParser
{
    public class MediaParser
    {
        public readonly Page Page;

        public readonly SortedSet<Media> Media = new(new MediaComparer());

        public MediaParser(Page page)
        {
            Page = page;
            InitHtmlDocument();
        }

        public void AddMedia(Listing listing)
        {
            if (Page.HtmlDocument == null)
            {
                return;
            }
            new ImageParser(this).AddImages();
            new VideoParser(this).GetVideos();
            listing.SetMedia(Media);
        }

        private void InitHtmlDocument()
        {
            Page.LoadHtmlDocument(true);
            if (Page.HtmlDocument == null)
            {
                return;
            }
            var xPath =
                //"//nav | //footer | //style | //head | " +
                //"//code | //canvas | //input | //meta | //option | " +
                //"//select | //progress | //svg | //textarea | //del";

                "//nav | //footer | //style | " +
                "//code | //canvas | //input | //option | " +
                "//select | //progress | //svg | //textarea | //del";

            var nodesToRemove = Page.HtmlDocument.DocumentNode.SelectNodes(xPath).ToList();
            foreach (var node in nodesToRemove)
            {
                node.Remove();
            }
        }

        public void AddVideo(string url)
        {
            if (!Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out Uri? uri))
            {
                return;
            }
            if (uri == null)
            {
                return;
            }
            var media = new Media()
            {
                mediaType = MediaType.video,
                url = uri
            };
            Media.Add(media);
        }

        public void AddImage(string url, string title)
        {
            if (!Uri.TryCreate(Page.Uri, url, out Uri? uri))
            {
                return;
            }

            var media = new Media()
            {
                mediaType = MediaType.image,
                url = uri,
                title = title,
            };
            Media.Add(media);
        }
    }
}
