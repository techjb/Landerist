using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace landerist_domain.Parsing.UserInput;

public static class ListingImageUrlPlaceholderCodec
{
    private const string PlaceholderPrefix = "LANDERIST_IMAGE_";
    private const int HashHexLength = 16;

    private static readonly Regex UrlRegex = new(
        "https?://[^\\s\"'<>\\\\]+",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly Regex PlaceholderRegex = new(
        PlaceholderPrefix + "[0-9A-F]{" + HashHexLength + "}",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public static string ReplaceImageUrls(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        return UrlRegex.Replace(text, match =>
            IsImageUrl(match.Value) ? GetPlaceholder(match.Value) : match.Value);
    }

    public static string Resolve(
        string value,
        IReadOnlyDictionary<string, string> urlByPlaceholder)
    {
        string trimmed = value.Trim();
        if (urlByPlaceholder.TryGetValue(trimmed, out string? url))
        {
            return url;
        }

        Match match = PlaceholderRegex.Match(trimmed);
        return match.Success && urlByPlaceholder.TryGetValue(match.Value, out url)
            ? url
            : value;
    }

    public static string GetPlaceholder(string url)
    {
        string normalizedUrl = WebUtility.HtmlDecode(url);
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(normalizedUrl));
        return PlaceholderPrefix + Convert.ToHexString(hash)[..HashHexLength];
    }

    public static bool IsImageUrl(string url)
    {
        if (!Uri.TryCreate(WebUtility.HtmlDecode(url), UriKind.Absolute, out Uri? uri))
        {
            return false;
        }

        string extension = Path.GetExtension(uri.AbsolutePath);
        return extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".webp", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".gif", StringComparison.OrdinalIgnoreCase);
    }
}