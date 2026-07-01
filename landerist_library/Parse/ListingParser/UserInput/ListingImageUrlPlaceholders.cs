using landerist_library.Pages;
using landerist_library.Parse.ListingParser.StructuredOutputs;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace landerist_library.Parse.ListingParser.UserInput
{
    internal static class ListingImageUrlPlaceholders
    {
        private const int MinUrlLengthToReplace = 500;
        private const string PlaceholderPrefix = "LANDERIST_IMAGE_";
        private const int HashHexLength = 16;

        private static readonly Regex UrlRegex = new(
            "https?://[^\\s\"'<>\\\\]+",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        private static readonly Regex PlaceholderRegex = new(
            PlaceholderPrefix + "[0-9A-F]{" + HashHexLength + "}",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        public static string ReplaceLongImageUrls(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return text;
            }

            return UrlRegex.Replace(text, match =>
            {
                var url = match.Value;
                return ShouldReplace(url) ? GetPlaceholder(url) : url;
            });
        }

        public static void Resolve(Page page, StructuredOutputEs? structuredOutputEs)
        {
            var images = structuredOutputEs?.Anuncio?.ImagenesDelAnuncio;
            if (images == null || images.Count == 0)
            {
                return;
            }

            var urlByPlaceholder = GetImageUrlsByPlaceholder(page);
            if (urlByPlaceholder.Count == 0)
            {
                return;
            }

            foreach (var image in images)
            {
                if (image?.Url == null)
                {
                    continue;
                }

                image.Url = Resolve(image.Url, urlByPlaceholder);
            }
        }

        private static string Resolve(string value, Dictionary<string, string> urlByPlaceholder)
        {
            var trimmed = value.Trim();
            if (urlByPlaceholder.TryGetValue(trimmed, out var url))
            {
                return url;
            }

            var match = PlaceholderRegex.Match(trimmed);
            if (match.Success && urlByPlaceholder.TryGetValue(match.Value, out url))
            {
                return url;
            }

            return value;
        }

        private static Dictionary<string, string> GetImageUrlsByPlaceholder(Page page)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var source = GetSource(page);
            if (string.IsNullOrWhiteSpace(source))
            {
                return result;
            }

            foreach (Match match in UrlRegex.Matches(source))
            {
                var url = match.Value;
                if (!ShouldReplace(url))
                {
                    continue;
                }

                result.TryAdd(GetPlaceholder(url), WebUtility.HtmlDecode(url));
            }

            return result;
        }

        private static string? GetSource(Page page)
        {
            var htmlDocument = page.GetHtmlDocument();
            if (htmlDocument?.DocumentNode == null)
            {
                page.SetResponseBodyFromZipped();
                htmlDocument = page.GetHtmlDocument();
            }

            return htmlDocument?.DocumentNode?.OuterHtml;
        }

        private static string GetPlaceholder(string url)
        {
            var normalizedUrl = WebUtility.HtmlDecode(url);
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(normalizedUrl));
            return PlaceholderPrefix + Convert.ToHexString(hash)[..HashHexLength];
        }

        private static bool ShouldReplace(string url)
        {
            if (url.Length < MinUrlLengthToReplace)
            {
                return false;
            }

            if (!Uri.TryCreate(WebUtility.HtmlDecode(url), UriKind.Absolute, out var uri))
            {
                return false;
            }

            var extension = Path.GetExtension(uri.AbsolutePath);
            return extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
                extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                extension.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
                extension.Equals(".webp", StringComparison.OrdinalIgnoreCase) ||
                extension.Equals(".gif", StringComparison.OrdinalIgnoreCase);
        }
    }
}
