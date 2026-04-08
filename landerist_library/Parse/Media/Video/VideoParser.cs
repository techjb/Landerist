using HtmlAgilityPack;
using landerist_orels.ES;
using landerist_orels;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace landerist_library.Parse.Media.Video
{
    public partial class VideoParser(MediaParser mediaParser)
    {
        private readonly MediaParser _mediaParser = mediaParser;
        private readonly HashSet<string> _addedVideoUrls = new(StringComparer.OrdinalIgnoreCase);

        public void GetVideos()
        {
            HtmlDocument? htmlDocument = _mediaParser.HtmlDocument;
            if (htmlDocument == null)
            {
                return;
            }

            GetYoutubeVideos(htmlDocument.DocumentNode.SelectNodes("//a[@href]"), "href");
            GetYoutubeVideos(htmlDocument.DocumentNode.SelectNodes("//iframe[@src]"), "src");
        }

        private void GetYoutubeVideos(HtmlNodeCollection? linkNodes, string attributeName)
        {
            if (linkNodes == null)
            {
                return;
            }

            Regex regex = RegexYoutube();
            foreach (HtmlNode linkNode in linkNodes)
            {
                string attributeValue = linkNode.GetAttributeValue(attributeName, string.Empty);
                if (string.IsNullOrWhiteSpace(attributeValue))
                {
                    continue;
                }

                string normalizedUrl = NormalizeUrl(attributeValue);
                if (!regex.IsMatch(normalizedUrl))
                {
                    continue;
                }

                AddVideo(normalizedUrl);
            }
        }

        public void AddVideo(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return;
            }

            string normalizedUrl = NormalizeUrl(url);
            if (!Uri.TryCreate(normalizedUrl, UriKind.Absolute, out Uri? uri) || uri == null)
            {
                return;
            }

            if (!_addedVideoUrls.Add(uri.AbsoluteUri))
            {
                return;
            }

            var media = new landerist_orels.Media()
            {
                mediaType = MediaType.video,
                url = uri
            };

            _mediaParser.Add(media);
        }

        private static string NormalizeUrl(string url)
        {
            string normalizedUrl = HtmlEntity.DeEntitize(url).Trim();

            if (normalizedUrl.StartsWith("//", StringComparison.Ordinal))
            {
                normalizedUrl = "https:" + normalizedUrl;
            }

            return normalizedUrl;
        }

        [GeneratedRegex(@"^(?:https?:\/\/)?(?:www\.|m\.)?(?:youtube\.com\/(?:embed\/|watch\?(?:.*&)?v=|shorts\/|live\/)|youtube-nocookie\.com\/embed\/|youtu\.be\/)[\w-]+(?:[?&].*)?$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
        private static partial Regex RegexYoutube();
    }
}
