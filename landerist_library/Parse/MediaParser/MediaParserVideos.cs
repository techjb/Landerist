using HtmlAgilityPack;
using landerist_orels.ES;
using System.Text.RegularExpressions;

namespace landerist_library.Parse.MediaParser
{
    public class MediaParserVideos
    {
        private readonly MediaParser MediaParser;

        public MediaParserVideos(MediaParser mediaParser)
        {
            MediaParser = mediaParser;
        }

        public void GetVideos()
        {
            HtmlNodeCollection linkNodes = MediaParser.Page.HtmlDocument!.DocumentNode.SelectNodes("//a[@href]");
            GetYoutubeVideos(linkNodes, "href");

            linkNodes = MediaParser.Page.HtmlDocument!.DocumentNode.SelectNodes("//iframe[@src]");
            GetYoutubeVideos(linkNodes, "src");
        }

        private void GetYoutubeVideos(HtmlNodeCollection linkNodes, string attributeName)
        {
            if (linkNodes == null)
            {
                return;
            }

            Regex regex = new(@"(?:https?:\/\/)?(?:www\.)?(?:youtube\.com\/(?:embed\/|watch\?v=)|youtu\.be\/)([\w\d_-]+)");
            foreach (HtmlNode linkNode in linkNodes)
            {
                string attributeValue = linkNode.GetAttributeValue(attributeName, string.Empty);
                Match match = regex.Match(attributeValue);
                if (!match.Success)
                {
                    continue;
                }
                if (Uri.TryCreate(attributeValue, UriKind.Absolute, out Uri? uri))
                {
                    if (uri != null)
                    {
                        var media = new Media()
                        {
                            mediaType = MediaType.video,
                            url = uri
                        };
                        MediaParser.Media.Add(media);
                    }
                }
            }
        }
    }
}
