using HtmlAgilityPack;

namespace landerist_library.Parse.MediaParser
{
    public class ImageParser
    {
        private readonly MediaParser MediaParser;

        public ImageParser(MediaParser mediaParser)
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
                ParseImage(node, attributeValue);
            }
        }

        private void ParseImage(HtmlNode imgNode, string name)
        {
            string attributeValue = imgNode.GetAttributeValue(name, null);
            if (string.IsNullOrEmpty(attributeValue))
            {
                return;
            }

            string extension = Path.GetExtension(attributeValue).ToLower();
            if (!extension.StartsWith(".jpg") && !extension.StartsWith(".jpeg"))
            {
                return;
            }

            string title = imgNode.GetAttributeValue("alt", null);
            if (string.IsNullOrEmpty(title))
            {
                title = imgNode.GetAttributeValue("title", null);
            }

            MediaParser.AddImage(attributeValue, title);
        }
    }
}
