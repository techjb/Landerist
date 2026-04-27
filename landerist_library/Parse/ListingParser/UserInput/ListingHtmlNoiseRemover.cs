using HtmlAgilityPack;

namespace landerist_library.Parse.ListingParser.UserInput
{
    internal static class ListingHtmlNoiseRemover
    {
        private const string UppercaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        private const string LowercaseLetters = "abcdefghijklmnopqrstuvwxyz";

        private static readonly HashSet<string> HtmlNoiseTagsToRemove =
        [
            "//head",
            "//meta",
            "//nav",
            "//footer",
            "//form",
            "//noscript",
            "//template",
            "//dialog",
        ];

        private static readonly HashSet<string> HtmlNoiseAttributeHints =
        [
            "cookie",
            "cookies",
            "consent",
            "gdpr",
            "privac",
            "newsletter",
            "subscribe",
            "suscrib",
            "modal",
            "popup",
            "popover",
            "similar",
            "related",
            "recommend",
            "recommended",
        ];

        private static readonly string NoiseTagsXPath = string.Join(" | ", HtmlNoiseTagsToRemove);

        private static readonly string NoiseIdXPath = ToNoiseXpathContains("@id");

        private static readonly string NoiseClassXPath = ToNoiseXpathContains("@class");

        public static void Remove(HtmlDocument htmlDocument)
        {
            RemoveComments(htmlDocument);
            RemoveTags(htmlDocument);
            ListingHtmlNodeRemover.Remove(htmlDocument, NoiseIdXPath);
            ListingHtmlNodeRemover.Remove(htmlDocument, NoiseClassXPath);
            ListingRecommendationSectionRemover.Remove(htmlDocument);
        }

        public static void RemoveTags(HtmlDocument htmlDocument)
        {
            ListingHtmlNodeRemover.Remove(htmlDocument, NoiseTagsXPath);
        }

        private static string ToNoiseXpathContains(string selector)
        {
            var enumerable = HtmlNoiseAttributeHints.Select(word =>
                $"contains(translate({selector}, '{UppercaseLetters}', '{LowercaseLetters}'), '{word.ToLowerInvariant()}')");

            return "//*[not(self::html or self::body or self::main) and (" + string.Join(" or ", enumerable) + ")]";
        }

        private static void RemoveComments(HtmlDocument htmlDocument)
        {
            var comments = htmlDocument.DocumentNode
                .DescendantsAndSelf()
                .Where(node => node.NodeType == HtmlNodeType.Comment)
                .ToList();

            foreach (var comment in comments)
            {
                comment.Remove();
            }
        }
    }
}
