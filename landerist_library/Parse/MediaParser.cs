using landerist_library.Websites;
using landerist_orels.ES;

namespace landerist_library.Parse
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
            new MediaParserImages(this).GetImages();
            new MediaParserVideos(this).GetVideos();            
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
                "//nav | //footer | //style | //head | " +
                "//form | //code | //canvas | //input | //meta | //option | " +
                "//select | //progress | //svg | //textarea | //del";

            var nodesToRemove = Page.HtmlDocument.DocumentNode.SelectNodes(xPath).ToList();
            foreach (var node in nodesToRemove)
            {
                node.Remove();
            }
        }
    }
}
