using landerist_orels.ES;

namespace landerist_library.Parse.Media.Image
{
    internal class ImageParserUrls(MediaParser mediaParser) : ImageParser(mediaParser)
    {
        readonly Dictionary<string, string> SrcAlt = [];

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
            HashSet<string> hashSet = [.. list];
            InitDictionaries();
            foreach (var image in hashSet)
            {
                AddImage(image);
            }
        }

        private void InitDictionaries()
        {
            var imageNodes = MediaParser.HtmlDocument!.DocumentNode.SelectNodes("//img");
            foreach (var imageNode in imageNodes)
            {
                var src = imageNode.GetAttributeValue("src", "");
                var alt = imageNode.GetAttributeValue("alt", "");
                var title = imageNode.GetAttributeValue("title", "");

                if (string.IsNullOrEmpty(src))
                {
                    continue;
                }
                if (!string.IsNullOrEmpty(alt))
                {
                    SrcAlt.TryAdd(src, alt);
                }
                if (!string.IsNullOrEmpty(title))
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
            if (SrcAlt.TryGetValue(url, out string? alt))
            {
                return alt;
            }
            if (SrcTitle.TryGetValue(url, out string? title))
            {
                return title;
            }
            return null;
        }
    }
}
