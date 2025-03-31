using HtmlAgilityPack;
using landerist_orels.ES;

namespace landerist_library.Parse.Media.Other
{
    public class OtherParser(MediaParser mediaParser)
    {
        private readonly MediaParser MediaParser = mediaParser;

        public void GetOthers()
        {
            HtmlNodeCollection htmlNodeCollection = MediaParser.HtmlDocument!.DocumentNode.SelectNodes("//a[@href]");
            GetOthers(htmlNodeCollection, ".pdf");
        }

        private void GetOthers(HtmlNodeCollection htmlNodeCollection, string fileExtension)
        {
            if (htmlNodeCollection == null)
            {
                return;
            }
            foreach (var node in htmlNodeCollection)
            {
                GetOthers(node, fileExtension);
            }
        }

        private void GetOthers(HtmlNode htmlNode, string fileExtension)
        {
            string? attributeValue = htmlNode.GetAttributeValue("href", "");
            if (string.IsNullOrEmpty(attributeValue))
            {
                return;
            }

            string extension = Path.GetExtension(attributeValue).ToLower();
            if (!extension.Equals(fileExtension))
            {
                return;
            }

            if (!Uri.TryCreate(MediaParser.Page.Uri, attributeValue, out Uri? uri))
            {
                return;
            }

            string title = MediaParser.GetTitle(htmlNode);

            var media = new landerist_orels.ES.Media()
            {
                mediaType = MediaType.other,
                url = uri,
                title = title,
            };

            MediaParser.Add(media);
        }
    }
}
