using landerist_domain.Parsing.StructuredOutputs;
using landerist_domain.Parsing.UserInput;
using landerist_library.Pages;
using System.Net;
using System.Text.RegularExpressions;

namespace landerist_library.Infrastructure.Parsing.UserInput;

public static class ListingImageUrlPlaceholders
{
    private static readonly Regex UrlRegex = new(
        "https?://[^\\s\"'<>\\\\]+",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public static void Resolve(Page page, StructuredOutputEs? output)
    {
        var images = output?.Anuncio?.ImagenesDelAnuncio;
        if (images is null || images.Count == 0)
        {
            return;
        }

        Dictionary<string, string> urls = GetImageUrlsByPlaceholder(page);
        foreach (var image in images.Where(image => image?.Url is not null))
        {
            image!.Url = ListingImageUrlPlaceholderCodec.Resolve(image.Url!, urls);
        }
    }

    private static Dictionary<string, string> GetImageUrlsByPlaceholder(Page page)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        string? source = GetSource(page);
        if (string.IsNullOrWhiteSpace(source))
        {
            return result;
        }

        foreach (Match match in UrlRegex.Matches(source))
        {
            string url = match.Value;
            if (ListingImageUrlPlaceholderCodec.IsImageUrl(url))
            {
                result.TryAdd(
                    ListingImageUrlPlaceholderCodec.GetPlaceholder(url),
                    WebUtility.HtmlDecode(url));
            }
        }

        return result;
    }

    private static string? GetSource(Page page)
    {
        var document = page.GetHtmlDocument();
        if (document?.DocumentNode is null)
        {
            page.SetResponseBodyFromZipped();
            document = page.GetHtmlDocument();
        }

        return document?.DocumentNode?.OuterHtml;
    }
}
