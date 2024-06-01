using HtmlAgilityPack;
using landerist_orels.ES;
using System.Text.RegularExpressions;

namespace landerist_library.Parse.Media.Video
{
    public partial class VideoParser(MediaParser mediaParser)
    {
        private readonly MediaParser MediaParser = mediaParser;

        public void GetVideos()
        {
            if(MediaParser.HtmlDocument == null)
            {
                return;
            }
            
            HtmlNodeCollection linkNodes = MediaParser.HtmlDocument.DocumentNode.SelectNodes("//a[@href]");
            GetYoutubeVideos(linkNodes, "href");

            linkNodes = MediaParser.HtmlDocument.DocumentNode.SelectNodes("//iframe[@src]");
            GetYoutubeVideos(linkNodes, "src");
        }

        private void GetYoutubeVideos(HtmlNodeCollection linkNodes, string attributeName)
        {
            if (linkNodes == null)
            {
                return;
            }

            Regex regex = RegexYoutube();
            foreach (HtmlNode linkNode in linkNodes)
            {
                string attributeValue = linkNode.GetAttributeValue(attributeName, string.Empty);
                if (attributeValue == null)
                {
                    continue;
                }
                Match match = regex.Match(attributeValue);
                if (!match.Success)
                {
                    continue;
                }
                if (attributeValue.StartsWith("//"))
                {
                    attributeValue = "https:" + attributeValue;
                }
                AddVideo(attributeValue);
            }
        }

        public void AddVideo(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
            {
                return;
            }
            if (uri == null)
            {
                return;
            }
            var media = new landerist_orels.ES.Media()
            {
                mediaType = MediaType.video,
                url = uri
            };
            MediaParser.Add(media);
        }

        [GeneratedRegex(@"(?:https?:\/\/)?(?:www\.)?(?:youtube\.com\/(?:embed\/|watch\?v=)|youtu\.be\/)([\w\d_-]+)")]
        private static partial Regex RegexYoutube();
    }
}
