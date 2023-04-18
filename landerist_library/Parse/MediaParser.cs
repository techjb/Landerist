using landerist_library.Websites;
using landerist_orels.ES;
using HtmlAgilityPack;

namespace landerist_library.Parse
{
    public class MediaParser
    {
        private readonly Page Page;

        private readonly SortedSet<Media> Medias = new(new MediaComparer());

        public MediaParser(Page page)
        {
            Page = page;            
            RemoveResponseBodyNodes();
        }

        public void AddMedia(Listing listing)
        {
           
            if (Page.HtmlDocument == null)
            {
                return;
            }
            GetImages(Page.HtmlDocument);
            listing.SetMedia(Medias);
        }

        private void RemoveResponseBodyNodes()
        {
            Page.LoadHtmlDocument(true);
            if (Page.HtmlDocument == null)
            {
                return;
            }
            var xPath =
                "//script | //nav | //footer | //style | //head | " +
                "//form | //code | //canvas | //input | //meta | //option | " +
                "//select | //progress | //svg | //textarea | //del";

            var nodesToRemove = Page.HtmlDocument.DocumentNode.SelectNodes(xPath).ToList();
            foreach (var node in nodesToRemove)
            {
                node.Remove();
            }
        }

        private void GetImages(HtmlDocument htmlDocument)
        {
            GetImagesSrc(htmlDocument);
            GetImagesA(htmlDocument);
        }

        private void GetImagesSrc(HtmlDocument htmlDocument)
        {
            var imageNodes = htmlDocument.DocumentNode.SelectNodes("//img");
            ParseImages(imageNodes, "src");
        }

        private void GetImagesA(HtmlDocument htmlDocument)
        {
            var imageNodes = htmlDocument.DocumentNode.SelectNodes("//a");
            ParseImages(imageNodes, "href");
        }

        private void ParseImages(HtmlNodeCollection? nodeCollection, string attributeValue)
        {
            if (nodeCollection == null)
            {
                return;
            }
            foreach (var node in nodeCollection)
            {
                var media = ParseImage(node, attributeValue);
                if (media != null)
                {
                    Medias.Add(media);
                }
            }
        }

        private Media? ParseImage(HtmlNode imgNode, string name)
        {
            string attributeValue = imgNode.GetAttributeValue(name, null);
            if (string.IsNullOrEmpty(attributeValue))
            {
                return null;
            }
            if (!Uri.TryCreate(Page.Uri, attributeValue, out Uri? uri))
            {
                return null;
            }
            string title = imgNode.GetAttributeValue("alt", null);
            if (string.IsNullOrEmpty(title))
            {
                title = imgNode.GetAttributeValue("title", null);
            }

            string extension = Path.GetExtension(attributeValue).ToLower();
            if (extension != ".jpg" && extension != ".jpeg")
            {
                return null;
            }

            return new Media()
            {
                mediaType = MediaType.image,
                url = uri,
                title = title,
            };
        }
    }
}
