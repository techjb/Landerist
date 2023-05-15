using landerist_library.Websites;
using landerist_orels.ES;

namespace landerist_library.Parse.Media
{
    public class MediaParser
    {
        public readonly Page Page;

        public readonly SortedSet<landerist_orels.ES.Media> Media = new(new MediaComparer());

        public MediaParser(Page page)
        {
            Page = page;
            InitHtmlDocument();
        }

        public void AddMedia(landerist_orels.ES.Listing listing)
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
                "//nav | //footer | //style | " +
                "//code | //canvas | //input | //option | " +
                "//select | //progress | //svg | //textarea | //del";

            var nodesToRemove = Page.HtmlDocument.DocumentNode.SelectNodes(xPath).ToList();
            foreach (var node in nodesToRemove)
            {
                node.Remove();
            }
        }
    }
}
