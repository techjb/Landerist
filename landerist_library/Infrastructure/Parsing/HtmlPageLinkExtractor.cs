using HtmlAgilityPack;
using landerist_library.Application.Scraping;
using landerist_library.Pages;
using System.Text.RegularExpressions;

namespace landerist_library.Infrastructure.Parsing;

public sealed class HtmlPageLinkExtractor : IPageLinkExtractor
{
    public bool ContainsMetaRobotsNoFollow(Page page) =>
        page.ContainsMetaRobotsNoFollow();

    public Uri? GetCanonicalUri(Page page) => page.GetCanonicalUri();

    public IReadOnlyList<string> GetFollowedLinks(Page page)
    {
        HtmlDocument? document = page.GetHtmlDocument();
        if (document is null)
        {
            return [];
        }

        return document.DocumentNode
            .Descendants("a")
            .Where(link => !HasNoFollow(link) && !IsHidden(link))
            .Select(link => link.GetAttributeValue("href", string.Empty))
            .Where(url => !string.IsNullOrWhiteSpace(url))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public IReadOnlyList<PageLanguageAlternate> GetLanguageAlternates(Page page)
    {
        HtmlDocument? document = page.GetHtmlDocument();
        if (document is null)
        {
            return [];
        }

        return document.DocumentNode
            .Descendants("link")
            .Where(node => node.GetAttributeValue("rel", string.Empty)
                .Contains("alternate", StringComparison.OrdinalIgnoreCase))
            .Select(node => new PageLanguageAlternate(
                node.GetAttributeValue("hreflang", string.Empty),
                node.GetAttributeValue("href", string.Empty)))
            .ToArray();
    }

    private static bool HasNoFollow(HtmlNode link) =>
        link.GetAttributeValue("rel", string.Empty)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(value => value.Equals("nofollow", StringComparison.OrdinalIgnoreCase));

    private static bool IsHidden(HtmlNode link) =>
        Regex.IsMatch(
            link.GetAttributeValue("style", string.Empty),
            @"display\s*:\s*none|visibility\s*:\s*hidden",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
}
