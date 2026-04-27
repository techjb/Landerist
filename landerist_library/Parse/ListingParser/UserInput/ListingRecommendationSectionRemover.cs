using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Web;

namespace landerist_library.Parse.ListingParser.UserInput
{
    internal static partial class ListingRecommendationSectionRemover
    {
        private const int MinimumRecommendationLinks = 3;

        private const int MinimumAggressiveRecommendationLinks = 8;

        private static readonly HashSet<string> RecommendationSectionHints =
        [
            "anuncios similares",
            "anuncios relacionados",
            "inmuebles similares",
            "inmuebles relacionados",
            "propiedades similares",
            "propiedades relacionadas",
            "pisos similares",
            "casas similares",
            "otros inmuebles",
            "otros anuncios",
            "también te puede interesar",
            "tambien te puede interesar",
            "te puede interesar",
            "similar properties",
            "similar homes",
            "related properties",
            "related listings",
            "you may also like",
            "recommended",
            "featured properties",
        ];

        private static readonly HashSet<string> RecommendationContainerTags =
        [
            "section",
            "div",
            "aside",
            "ul",
            "ol",
            "nav",
        ];

        private static readonly HashSet<string> HeadingTags =
        [
            "h1",
            "h2",
            "h3",
            "h4",
            "h5",
            "h6",
            "header",
        ];

        public static void Remove(HtmlDocument htmlDocument)
        {
            var candidates = htmlDocument.DocumentNode.Descendants()
                .Where(node => node.NodeType == HtmlNodeType.Element)
                .Where(node => RecommendationContainerTags.Contains(node.Name))
                .OrderBy(node => node.Ancestors().Count())
                .ToList();

            HashSet<HtmlNode> nodesToRemove = [];
            foreach (var node in candidates)
            {
                if (node.ParentNode == null || HasAncestorMarkedToRemove(node, nodesToRemove))
                {
                    continue;
                }

                if (IsLikelyRecommendationSection(node))
                {
                    nodesToRemove.Add(node);
                }
            }

            foreach (var node in nodesToRemove)
            {
                node.Remove();
            }
        }

        private static bool HasAncestorMarkedToRemove(HtmlNode node, HashSet<HtmlNode> nodesToRemove)
        {
            return node.Ancestors().Any(nodesToRemove.Contains);
        }

        private static bool IsLikelyRecommendationSection(HtmlNode node)
        {
            string text = NormalizeText(node.InnerText);
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            int wordCount = CountWords(text);
            if (wordCount == 0)
            {
                return false;
            }

            var anchors = node.Descendants("a")
                .Select(anchor => NormalizeText(anchor.InnerText))
                .Where(anchorText => !string.IsNullOrEmpty(anchorText))
                .ToList();

            int linkCount = anchors.Count;
            if (linkCount == 0)
            {
                return false;
            }

            bool hasHint = HasRecommendationHint(node, text);
            bool hasRepeatedLinkedChildren = HasRepeatedLinkedChildren(node);
            int linkWords = anchors.Sum(CountWords);
            double linkDensity = (double)linkWords / Math.Max(1, wordCount);

            if (hasHint && (linkCount >= MinimumRecommendationLinks || hasRepeatedLinkedChildren || linkDensity >= 0.20d))
            {
                return true;
            }

            return linkCount >= MinimumAggressiveRecommendationLinks &&
                hasRepeatedLinkedChildren &&
                linkDensity >= 0.55d &&
                wordCount <= 350;
        }

        private static bool HasRecommendationHint(HtmlNode node, string normalizedText)
        {
            string leadingText = TakeWords(normalizedText, 60);
            if (RecommendationSectionHints.Any(leadingText.Contains))
            {
                return true;
            }

            string headingText = NormalizeText(string.Join(" ",
                node.Descendants()
                    .Where(child => HeadingTags.Contains(child.Name))
                    .Take(3)
                    .Select(child => child.InnerText)));

            if (!string.IsNullOrEmpty(headingText) && RecommendationSectionHints.Any(headingText.Contains))
            {
                return true;
            }

            string attributeText = NormalizeText(
                node.GetAttributeValue("id", string.Empty) + " " +
                node.GetAttributeValue("class", string.Empty));

            return attributeText.Contains("related", StringComparison.Ordinal) ||
                attributeText.Contains("recommend", StringComparison.Ordinal) ||
                attributeText.Contains("featured", StringComparison.Ordinal) ||
                attributeText.Contains("destacad", StringComparison.Ordinal) ||
                attributeText.Contains("nearby", StringComparison.Ordinal);
        }

        private static bool HasRepeatedLinkedChildren(HtmlNode node)
        {
            var linkedChildren = node.ChildNodes
                .Where(child => child.NodeType == HtmlNodeType.Element)
                .Where(child => child.Descendants("a").Any(anchor => !string.IsNullOrWhiteSpace(anchor.InnerText)))
                .ToList();

            if (linkedChildren.Count < 3)
            {
                return false;
            }

            int repeatedStructureCount = linkedChildren
                .Select(GetStructureSignature)
                .GroupBy(signature => signature)
                .Select(group => group.Count())
                .DefaultIfEmpty(0)
                .Max();

            return repeatedStructureCount >= 3 || linkedChildren.Count >= 4;
        }

        private static string GetStructureSignature(HtmlNode node)
        {
            string children = string.Join(",",
                node.ChildNodes
                    .Where(child => child.NodeType == HtmlNodeType.Element)
                    .Select(child => child.Name)
                    .Take(4));

            return $"{node.Name}>{children}";
        }

        private static string NormalizeText(string? text)
        {
            string value = HttpUtility.HtmlDecode(text ?? string.Empty).ToLowerInvariant();
            value = RegexSpace().Replace(value, " ");
            return value.Trim();
        }

        private static string TakeWords(string text, int maxWords)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            string[] words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (words.Length <= maxWords)
            {
                return text;
            }

            return string.Join(" ", words.Take(maxWords));
        }

        private static int CountWords(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return 0;
            }

            char[] delimiters = [' ', '\r', '\n'];
            return text.Split(delimiters, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Length;
        }

        [GeneratedRegex(@"\s+")]
        private static partial Regex RegexSpace();
    }
}
