using HtmlAgilityPack;
using landerist_orels.ES;

namespace landerist_library.Parse.MediaParser
{
    public class ImageParser
    {
        private readonly MediaParser MediaParser;

        private static readonly HashSet<string> ProhibitedWords = new(StringComparer.OrdinalIgnoreCase)
        {
            "icon",
            "logo",
        };

        public ImageParser(MediaParser mediaParser)
        {
            MediaParser = mediaParser;
        }

        public void AddImages()
        {
            AddImagesOpenGraph();
            AddImagesImgSrc();
            //GetImagesA(); // Add some invalid images
        }

        private void AddImagesOpenGraph()
        {
            var imageNodes = MediaParser.Page.HtmlDocument!.DocumentNode.SelectNodes("//meta[@property='og:image']");
            AddImages(imageNodes, "content");

            imageNodes = MediaParser.Page.HtmlDocument!.DocumentNode.SelectNodes("//meta[@property='og:image:secure_url']");
            AddImages(imageNodes, "content");
        }

        private void AddImagesImgSrc()
        {
            var imageNodes = MediaParser.Page.HtmlDocument!.DocumentNode.SelectNodes("//img");
            AddImages(imageNodes, "src");
        }

        private void AddImagesA()
        {
            var imageNodes = MediaParser.Page.HtmlDocument!.DocumentNode.SelectNodes("//a");
            AddImages(imageNodes, "href");
        }

        private void AddImages(HtmlNodeCollection? nodeCollection, string attributeValue)
        {
            if (nodeCollection == null)
            {
                return;
            }
            foreach (var node in nodeCollection)
            {
                AddImage(node, attributeValue);
            }
        }

        private void AddImage(HtmlNode imgNode, string name)
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

            if (!Uri.TryCreate(MediaParser.Page.Uri, attributeValue, out Uri? uri))
            {
                return;
            }

            if (!IsValid(uri))
            {
                return;
            }

            var media = new Media()
            {
                mediaType = MediaType.image,
                url = uri,
                title = title,
            };

            MediaParser.Media.Add(media);
        }

        private static bool IsValid(Uri uri)
        {
            string? filename = Path.GetFileNameWithoutExtension(uri.LocalPath);
            if (string.IsNullOrEmpty(filename))
            {
                return true;
            }

            filename = filename.Replace("_", "-");
            var splitted = filename.Split('-');
            foreach (var word in splitted)
            {
                foreach (string prohibitedWord in ProhibitedWords)
                {
                    if (word.Equals(prohibitedWord, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }
            }
            
            return true;
        }
    }
}
