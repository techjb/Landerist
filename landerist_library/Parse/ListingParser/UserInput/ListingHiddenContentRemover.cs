using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Web;

namespace landerist_library.Parse.ListingParser.UserInput
{
    internal static partial class ListingHiddenContentRemover
    {
        private static readonly HashSet<string> HiddenAttributeHints = new(StringComparer.OrdinalIgnoreCase)
        {
            "hidden",
            "hide",
            "is-hidden",
            "u-hidden",
            "sr-only",
            "screen-reader-only",
            "visually-hidden",
            "visuallyhidden",
            "d-none",
            "display-none",
            "invisible",
            "collapsed",
            "collapse",
        };

        private static readonly HashSet<string> MediaTags =
        [
            "img",
            "picture",
            "source",
        ];

        public static void Remove(HtmlDocument htmlDocument, bool preserveMediaNodes)
        {
            var hiddenNodes = htmlDocument.DocumentNode.Descendants()
                .Where(node => node.NodeType == HtmlNodeType.Element)
                .Where(IsHiddenNode)
                .OrderBy(node => node.Ancestors().Count())
                .ToList();

            HashSet<HtmlNode> nodesToRemove = [];
            foreach (var node in hiddenNodes)
            {
                if (node.ParentNode == null || HasAncestorMarkedToRemove(node, nodesToRemove))
                {
                    continue;
                }

                if (preserveMediaNodes && ContainsMediaNode(node))
                {
                    RemoveHiddenTextDescendants(node);
                    continue;
                }

                nodesToRemove.Add(node);
            }

            foreach (var node in nodesToRemove)
            {
                node.Remove();
            }
        }

        public static bool HasHiddenAncestor(HtmlNode node)
        {
            return node.Ancestors().Any(IsHiddenNode);
        }

        private static void RemoveHiddenTextDescendants(HtmlNode node)
        {
            var textNodes = node.DescendantsAndSelf()
                .Where(descendant => descendant.NodeType == HtmlNodeType.Text)
                .ToList();

            foreach (var textNode in textNodes)
            {
                textNode.Remove();
            }
        }

        private static bool HasAncestorMarkedToRemove(HtmlNode node, HashSet<HtmlNode> nodesToRemove)
        {
            return node.Ancestors().Any(nodesToRemove.Contains);
        }

        private static bool ContainsMediaNode(HtmlNode node)
        {
            return MediaTags.Contains(node.Name) ||
                node.Descendants().Any(descendant => MediaTags.Contains(descendant.Name));
        }

        private static bool IsHiddenNode(HtmlNode node)
        {
            if (node.NodeType != HtmlNodeType.Element)
            {
                return false;
            }

            if (node.Attributes["hidden"] != null)
            {
                return true;
            }

            if (node.GetAttributeValue("aria-hidden", string.Empty).Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (node.GetAttributeValue("type", string.Empty).Equals("hidden", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            string style = NormalizeAttributeText(node.GetAttributeValue("style", string.Empty));
            if (IsHiddenStyle(style))
            {
                return true;
            }

            return HasHiddenAttributeHint(node.GetAttributeValue("class", string.Empty)) ||
                HasHiddenAttributeHint(node.GetAttributeValue("id", string.Empty));
        }

        private static bool IsHiddenStyle(string style)
        {
            if (string.IsNullOrWhiteSpace(style))
            {
                return false;
            }

            style = style.Replace(" ", string.Empty);
            return style.Contains("display:none", StringComparison.OrdinalIgnoreCase) ||
                style.Contains("visibility:hidden", StringComparison.OrdinalIgnoreCase) ||
                style.Contains("opacity:0", StringComparison.OrdinalIgnoreCase);
        }

        private static bool HasHiddenAttributeHint(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            value = NormalizeAttributeText(value);
            var tokens = value.Split([' ', '_'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return tokens.Any(token => HiddenAttributeHints.Contains(token));
        }

        private static string NormalizeAttributeText(string value)
        {
            value = HttpUtility.HtmlDecode(value ?? string.Empty).ToLowerInvariant();
            value = RegexHorizontalSpace().Replace(value, " ");
            return value.Trim();
        }

        [GeneratedRegex(@"[^\S\r\n]+")]
        private static partial Regex RegexHorizontalSpace();
    }
}
