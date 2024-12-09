using landerist_orels.ES;

namespace landerist_library.Parse.Media.Image
{
    internal class ImageParserUrls(MediaParser mediaParser) : ImageParser(mediaParser)
    {
        readonly Dictionary<string, string> SrcTitle = [];
        public void AddImagesFromUrls(string[] list)
        {
            AddImagesOpenGraph();
            AddImagesUrls(list);
            foreach (var image in MediaImages)
            {
                MediaParser.Add(image);
            }
        }

        private void AddImagesUrls(string[] list)
        {
            HashSet<string> hashSet = new(list);
            InitSrcTitles();
            foreach (var image in hashSet)
            {
                AddImage(image);
            }
        }

        private void InitSrcTitles()
        {
            var imageNodes = MediaParser.HtmlDocument!.DocumentNode.SelectNodes("//img");
            foreach (var imageNode in imageNodes)
            {
                var title = imageNode.GetAttributeValue("title", "");
                var src = imageNode.GetAttributeValue("src", "");
                if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(src))
                {
                    SrcTitle.TryAdd(src, title);
                }
            }
        }

        private void AddImage(string url)
        {
            if (string.IsNullOrEmpty(url) || string.IsNullOrWhiteSpace(url))
            {
                return;
            }

            if (!MediaParser.HtmlDocument!.DocumentNode.OuterHtml.Contains(url))
            {
                return;
            }

            if (!Uri.TryCreate(MediaParser.Page.Uri, url, out Uri? uri))
            {
                return;
            }

            if (!IsValidImageUri(uri))
            {
                return;
            }

            var media = new landerist_orels.ES.Media()
            {
                mediaType = MediaType.image,
                url = uri,
                title = GetTitle(url),
            };

            MediaImages.Add(media);
        }

        private string? GetTitle(string url)
        {
            if (SrcTitle.TryGetValue(url, out string? value))
            {
                return value;
            }
            return null;    
        }
    }
}
