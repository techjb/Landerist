using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Web;

namespace landerist_library.Parse.ListingParser.UserInput
{
    internal static partial class ListingHtmlAttributeCleaner
    {
        private static readonly HashSet<string> AttributesToKeep = new(StringComparer.OrdinalIgnoreCase)
        {
            "href",
            "src",
            "srcset",
            "alt",
            "title",
            "content",
            "itemprop",
            "property",
            "name",
            "type",
            "datetime",
            "aria-label",
        };

        private static readonly HashSet<string> DataAttributesToKeep = new(StringComparer.OrdinalIgnoreCase)
        {
            "data-src",
            "data-srcset",
            "data-original",
            "data-lazy",
            "data-lazy-src",
            "data-lazy-srcset",
            "data-image",
            "data-images",
            "data-img",
            "data-full",
            "data-full-src",
            "data-background",
            "data-bg",
            "data-url",
            "data-href",
        };

        public static void Clean(HtmlDocument htmlDocument)
        {
            foreach (HtmlNode node in htmlDocument.DocumentNode.Descendants())
            {
                if (!node.HasAttributes)
                {
                    continue;
                }

                var attributesToRemove = node.Attributes
                    .Where(attribute => !ShouldKeepAttribute(attribute))
                    .ToList();

                foreach (var attribute in attributesToRemove)
                {
                    node.Attributes.Remove(attribute);
                }

                foreach (var attribute in node.Attributes.ToList())
                {
                    string value = CleanAttributeValue(attribute.Value);
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        node.Attributes.Remove(attribute);
                        continue;
                    }

                    attribute.Value = value;
                }
            }
        }

        private static bool ShouldKeepAttribute(HtmlAttribute attribute)
        {
            string name = attribute.Name;
            if (string.IsNullOrWhiteSpace(name) || name.StartsWith("on", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            string value = CleanAttributeValue(attribute.Value);
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            if (AttributesToKeep.Contains(name))
            {
                return HasUsefulAttributeValue(name, value);
            }

            if (name.StartsWith("data-", StringComparison.OrdinalIgnoreCase))
            {
                return IsUsefulDataAttribute(name, value);
            }

            return false;
        }

        private static bool HasUsefulAttributeValue(string name, string value)
        {
            if (IsUrlAttribute(name))
            {
                return IsUsefulUrlValue(value);
            }

            if (name.Equals("srcset", StringComparison.OrdinalIgnoreCase))
            {
                return IsUsefulSrcSetValue(value);
            }

            return true;
        }

        private static bool IsUsefulDataAttribute(string name, string value)
        {
            return DataAttributesToKeep.Contains(name) && IsLikelyUrlOrImageValue(value);
        }

        private static bool IsUrlAttribute(string name)
        {
            return name.Equals("href", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("src", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsUsefulUrlValue(string value)
        {
            value = value.Trim();
            return !value.StartsWith("#", StringComparison.Ordinal) &&
                !value.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase) &&
                !value.StartsWith("data:", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsUsefulSrcSetValue(string value)
        {
            return value.Contains("http", StringComparison.OrdinalIgnoreCase) ||
                value.Contains("//", StringComparison.Ordinal) ||
                value.Contains(".jpg", StringComparison.OrdinalIgnoreCase) ||
                value.Contains(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                value.Contains(".png", StringComparison.OrdinalIgnoreCase) ||
                value.Contains(".webp", StringComparison.OrdinalIgnoreCase) ||
                value.Contains(".avif", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsLikelyUrlOrImageValue(string value)
        {
            value = value.Trim();
            return IsUsefulUrlValue(value) &&
                (value.StartsWith("/", StringComparison.Ordinal) ||
                value.Contains("http", StringComparison.OrdinalIgnoreCase) ||
                IsUsefulSrcSetValue(value));
        }

        private static string CleanAttributeValue(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            value = HttpUtility.HtmlDecode(value);
            value = RegexSpace().Replace(value, " ").Trim();

            const int maxAttributeLength = 4000;
            if (value.Length > maxAttributeLength)
            {
                value = value[..maxAttributeLength].Trim();
            }

            return value;
        }

        [GeneratedRegex(@"\s+")]
        private static partial Regex RegexSpace();
    }
}
