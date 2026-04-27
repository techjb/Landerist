using HtmlAgilityPack;
using landerist_library.Pages;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace landerist_library.Parse.ListingParser
{
    public partial class ParseListingUserInput
    {
        private static readonly HashSet<string> TagsToRemove =
        [
            "//script",
            "//style",
            "//link",
            "//code",
            "//canvas",
            "//option",
            "//select",
            "//progress",
            "//svg",
            "//textarea",
            "//button",
            "//input",
            "//iframe",
            "//audio",
            "//video",
            "//map",
            "//area",
            "//source",
            "//embed",
            "//object",
            "//param",
        ];

        private static readonly HashSet<string> TagsToRemove2 =
        [
            "//head",
            "//noscript",
        ];

        private static readonly HashSet<string> AttributesToRemove =
        [
            "width",
            "height",
            "target",
            "style",
            "tabindex",
        ];

        private static readonly string XpathTagsToRemove = InitXpathTagsToRemove();

        private static readonly string XpathTagsToRemove2 = InitXpathTagsToRemove2();

        private static string InitXpathTagsToRemove()
        {
            return string.Join(" | ", TagsToRemove.ToList());
        }

        private static string InitXpathTagsToRemove2()
        {
            return string.Join(" | ", TagsToRemove2.ToList());
        }

        public static string? GetHtml(string responseBody)
        {
            return GetText(responseBody, true);
        }

        public static string? GetText(string responseBody, bool html)
        {
            if (string.IsNullOrWhiteSpace(responseBody))
            {
                return null;
            }

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(responseBody);

            if (html)
            {
                return GetHtml(htmlDocument);
            }

            return GetText(htmlDocument);
        }

        public static string? GetText(Page page)
        {
            try
            {
                var htmlDocument = page.GetHtmlDocument();
                if (htmlDocument != null)
                {
                    return GetText(htmlDocument);
                }
            }
            catch
            {

            }

            return null;
        }

        public static string? GetHtml(Page page)
        {
            try
            {
                var htmlDocument = page.GetHtmlDocument();
                if (htmlDocument != null)
                {
                    return GetHtml(htmlDocument);
                }
            }
            catch
            {
            }
            return null;
        }

        private static string? GetHtml(HtmlDocument htmlDocument)
        {
            string? text = null;
            try
            {
                RemoveNodes(htmlDocument, XpathTagsToRemove);
                RemoveAttributes(htmlDocument);
                text = Clean(htmlDocument);
                return text;
            }
            catch { }

            return text;
        }

        public static string? GetText(HtmlDocument htmlDocument)
        {
            try
            {
                RemoveNodes(htmlDocument, XpathTagsToRemove);
                RemoveNodes(htmlDocument, XpathTagsToRemove2);
                string text = GetVisibleText(htmlDocument);
                return Clean(text);
            }
            catch { }

            return null;
        }

        private static string GetVisibleText(HtmlDocument htmlDocument)
        {
            var stringBuilder = new StringBuilder();
            foreach (var node in htmlDocument.DocumentNode.DescendantsAndSelf())
            {
                if (node.NodeType == HtmlNodeType.Text)
                {
                    string innerText = node.InnerText;
                    if (!string.IsNullOrWhiteSpace(innerText))
                    {
                        stringBuilder.AppendLine(innerText.Trim());
                    }
                }
            }

            return stringBuilder.ToString();
        }

        private static void RemoveNodes(HtmlDocument htmlDocument, string select)
        {
            var htmlNodeCollection = htmlDocument.DocumentNode.SelectNodes(select);
            if (htmlNodeCollection != null)
            {
                List<HtmlNode> nodesToRemove = [.. htmlNodeCollection];
                foreach (var node in nodesToRemove)
                {
                    node.Remove();
                }
            }
        }

        private static void RemoveAttributes(HtmlDocument htmlDocument)
        {
            foreach (HtmlNode node in htmlDocument.DocumentNode.Descendants())
            {
                if (!node.HasAttributes)
                {
                    continue;
                }

                var onAttributes = node.Attributes.Where(attr => attr.Name.StartsWith("on", StringComparison.OrdinalIgnoreCase)).ToList();
                foreach (var attribute in onAttributes)
                {
                    node.Attributes.Remove(attribute);
                }

                foreach (var attribute in AttributesToRemove)
                {
                    if (node.Attributes[attribute] != null)
                    {
                        node.Attributes.Remove(attribute);
                    }
                }
            }
        }

        static string Clean(HtmlDocument htmlDocument)
        {
            string html = htmlDocument.DocumentNode.OuterHtml;
            return Clean(html);
        }

        static string Clean(string html)
        {
            html = HttpUtility.HtmlDecode(html);
            html = RegexSpace().Replace(html, " ");
            return html.Trim();
        }

        [GeneratedRegex(@"\s+")]
        private static partial Regex RegexSpace();
    }
}
