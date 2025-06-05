using landerist_orels.ES;
using Microsoft.IdentityModel.Tokens;

namespace landerist_library.Parse.Media.Image
{
    internal class ImageParserUrls(MediaParser mediaParser) : ImageParser(mediaParser)
    {
        readonly Dictionary<string, string> SrcAlt = [];

        readonly Dictionary<string, string> SrcTitle = [];



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
            foreach (var image in MediaImages)
            {
                MediaParser.Add(image);
            }
        }


        private void AddImagesUrls(string[] urls)
        {
            HashSet<string> hashSetUrls = [.. urls];
            InitDictionaries();
            foreach (var url in hashSetUrls)
            {
                AddImage(url);
            }
        }

        private void InitDictionaries()
        {
            if (MediaParser.HtmlDocument is null)
            {
                return;
            }
            var imageNodes = MediaParser.HtmlDocument!.DocumentNode.SelectNodes("//img");
            if (imageNodes is null)
            {
                return;
            }
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

        private void AddImage(string url, string? title)
        {
            if (string.IsNullOrEmpty(url) || string.IsNullOrWhiteSpace(url))
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

            if(title.IsNullOrEmpty())
            {
                title = string.Empty;
            }

            var media = new landerist_orels.ES.Media()
            {
                mediaType = MediaType.image,
                url = uri,
                title = title,
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
