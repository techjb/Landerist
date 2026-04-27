using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Web;

namespace landerist_library.Parse.ListingParser.UserInput
{
    internal static partial class ListingInputCleaner
    {
        private static readonly string[] HtmlBlockTags =
        [
            "address",
            "article",
            "aside",
            "blockquote",
            "br",
            "dd",
            "details",
            "div",
            "dl",
            "dt",
            "figcaption",
            "figure",
            "h1",
            "h2",
            "h3",
            "h4",
            "h5",
            "h6",
            "header",
            "li",
            "main",
            "ol",
            "p",
            "pre",
            "section",
            "summary",
            "table",
            "tbody",
            "td",
            "tfoot",
            "th",
            "thead",
            "tr",
            "ul",
        ];

        public static string CleanHtml(HtmlDocument htmlDocument)
        {
            string html = htmlDocument.DocumentNode.OuterHtml;
            html = HttpUtility.HtmlDecode(html);
            html = RegexHorizontalSpace().Replace(html, " ");
            html = AddHtmlBlockBreaks(html);
            return NormalizeLines(html);
        }

        public static string CleanText(string text)
        {
            text = HttpUtility.HtmlDecode(text);
            return NormalizeLines(text);
        }

        private static string AddHtmlBlockBreaks(string html)
        {
            foreach (string tag in HtmlBlockTags)
            {
                html = Regex.Replace(html, $@"\s*(</?{tag}\b[^>]*>)\s*", Environment.NewLine + "$1" + Environment.NewLine, RegexOptions.IgnoreCase);
            }

            return html;
        }

        private static string NormalizeLines(string text)
        {
            var lines = RegexLineBreak().Split(text)
                .Select(line => RegexHorizontalSpace().Replace(line, " ").Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line));

            return string.Join(Environment.NewLine, lines).Trim();
        }

        [GeneratedRegex(@"[^\S\r\n]+")]
        private static partial Regex RegexHorizontalSpace();

        [GeneratedRegex(@"\r\n?|\n")]
        private static partial Regex RegexLineBreak();
    }
}
