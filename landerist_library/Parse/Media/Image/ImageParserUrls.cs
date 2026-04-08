using landerist_orels;

namespace landerist_library.Parse.Media.Image
{
    internal class ImageParserUrls(MediaParser mediaParser) : ImageParser(mediaParser)
    {
        readonly Dictionary<string, string> SrcAlt = [];
        readonly Dictionary<string, string> SrcTitle = [];
        readonly HashSet<string> KnownImageUrls = [];

        public void AddImagesFromUrls(string[] urls)
        {
            //AddImagesOpenGraph();
            AddImagesUrls(urls);
            foreach (var image in MediaImages)
            {
                MediaParser.Add(image);
            }
        }

        public void AddImagesFromUrls(List<(string url, string? title)> tuples)
        {
            tuples = [.. tuples.GroupBy(x => x.url).Select(g => g.First())];
            foreach (var (url, title) in tuples)
            {
                AddImage(url, title);
            }

            foreach (var media in MediaImages)
            {
                MediaParser.Add(media);
            }
        }

        private void AddImagesUrls(string[] urls)
        {
            if (MediaParser.HtmlDocument is null)
            {
                return;
            }

            HashSet<string> hashSetUrls = [.. urls];
            InitDictionaries();

            foreach (var url in hashSetUrls)
            {
                AddImage(url);
            }
        }

        private void InitDictionaries()
        {
            SrcAlt.Clear();
            SrcTitle.Clear();
            KnownImageUrls.Clear();

            if (MediaParser.HtmlDocument is null)
            {
                return;
            }

            var imageNodes = MediaParser.HtmlDocument.DocumentNode.SelectNodes("//img");
            if (imageNodes is null)
            {
                return;
            }

            foreach (var imageNode in imageNodes)
            {
                var src = imageNode.GetAttributeValue("src", "");
                var alt = imageNode.GetAttributeValue("alt", "");
                var title = imageNode.GetAttributeValue("title", "");

                if (string.IsNullOrWhiteSpace(src))
                {
                    continue;
                }

                AddKnownImageUrl(src);

                if (Uri.TryCreate(MediaParser.Page.Uri, src, out Uri? absoluteUri))
                {
                    AddKnownImageUrl(absoluteUri.AbsoluteUri);

                    if (!string.IsNullOrEmpty(alt))
                    {
                        SrcAlt.TryAdd(absoluteUri.AbsoluteUri, alt);
                    }

                    if (!string.IsNullOrEmpty(title))
                    {
                        SrcTitle.TryAdd(absoluteUri.AbsoluteUri, title);
                    }
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
            if (string.IsNullOrWhiteSpace(url))
            {
                return;
            }

            if (MediaParser.HtmlDocument is null)
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

            if (!IsKnownImageUrl(url, uri.AbsoluteUri))
            {
                return;
            }

            var media = new landerist_orels.Media()
            {
                mediaType = MediaType.image,
                url = uri,
                title = GetTitle(url, uri.AbsoluteUri),
            };

            MediaImages.Add(media);
        }

        private void AddImage(string url, string? title)
        {
            if (string.IsNullOrWhiteSpace(url))
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

            title ??= string.Empty;

            var media = new landerist_orels.Media()
            {
                mediaType = MediaType.image,
                url = uri,
                title = title,
            };

            MediaImages.Add(media);
        }

        private string? GetTitle(string url, string absoluteUrl)
        {
            if (SrcAlt.TryGetValue(url, out string? alt))
            {
                return alt;
            }

            if (SrcAlt.TryGetValue(absoluteUrl, out alt))
            {
                return alt;
            }

            if (SrcTitle.TryGetValue(url, out string? title))
            {
                return title;
            }

            if (SrcTitle.TryGetValue(absoluteUrl, out title))
            {
                return title;
            }

            return null;
        }

        private void AddKnownImageUrl(string url)
        {
            if (!string.IsNullOrWhiteSpace(url))
            {
                KnownImageUrls.Add(url);
            }
        }

        private bool IsKnownImageUrl(string url, string absoluteUrl)
        {
            return KnownImageUrls.Contains(url) || KnownImageUrls.Contains(absoluteUrl);
        }
    }
}
