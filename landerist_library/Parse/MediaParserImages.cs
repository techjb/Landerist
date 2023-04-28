using HtmlAgilityPack;
using landerist_orels.ES;

namespace landerist_library.Parse
{
    public class MediaParserImages
    {
        private readonly MediaParser MediaParser;

        public MediaParserImages(MediaParser mediaParser)
        {
            MediaParser = mediaParser;
        }

        public void GetImages()
        {
            GetImagesSrc();
            GetImagesA();
        }

        private void GetImagesSrc()
        {
            var imageNodes = MediaParser.Page.HtmlDocument!.DocumentNode.SelectNodes("//img");
            ParseImages(imageNodes, "src");
        }

        private void GetImagesA()
        {
            var imageNodes = MediaParser.Page.HtmlDocument!.DocumentNode.SelectNodes("//a");
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
                    MediaParser.Media.Add(media);
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
            if (!Uri.TryCreate(MediaParser.Page.Uri, attributeValue, out Uri? uri))
            {
                return null;
            }
            
            string extension = Path.GetExtension(attributeValue).ToLower();
            if (!extension.StartsWith(".jpg") && !extension.StartsWith(".jpeg"))
            {
                return null;
            }

            string title = imgNode.GetAttributeValue("alt", null);
            if (string.IsNullOrEmpty(title))
            {
                title = imgNode.GetAttributeValue("title", null);
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
