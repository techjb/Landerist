using landerist_library.Websites;
using HtmlAgilityPack;
using landerist_library.Application.Listings;
using landerist_library.Application.Scraping;
using landerist_library.Infrastructure.Indexing;
using landerist_library.Pages;
using System.Text.RegularExpressions;

namespace landerist_library.Infrastructure.Scraping;

public sealed class PageIndexingService : IPageIndexingService
{
    private readonly bool _enabled;
    private readonly IPageLinkService _links;

    public PageIndexingService(bool enabled, IPageLinkService links)
    {
        ArgumentNullException.ThrowIfNull(links);
        _enabled = enabled;
        _links = links;
    }

    public void Index(Page page)
    {
        ArgumentNullException.ThrowIfNull(page);
        if (!_enabled)
        {
            return;
        }

        if (!string.IsNullOrEmpty(page.RedirectUrl))
        {
            Index(page, page.RedirectUrl);
            return;
        }

        if (page.PageType == PageType.IncorrectLanguage)
        {
            IndexLanguageAlternates(page);
            return;
        }

        if (page.PageType == PageType.NotCanonical)
        {
            Uri? canonical = page.GetCanonicalUri();
            if (canonical is not null)
            {
                _links.Index(page, canonical);
            }
            return;
        }

        if (page.ContainsMetaRobotsNoFollow() || !page.Website.HtmlIndexingEnabled)
        {
            return;
        }

        HtmlDocument? document = page.GetHtmlDocument();
        if (document is null)
        {
            return;
        }

        foreach (string url in document.DocumentNode
            .Descendants("a")
            .Where(link => !HasNoFollow(link) && !IsHidden(link))
            .Select(link => link.GetAttributeValue("href", string.Empty))
            .Where(url => !string.IsNullOrWhiteSpace(url))
            .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            Index(page, url);
        }
    }

    private void IndexLanguageAlternates(Page page)
    {
        HtmlDocument? document = page.GetHtmlDocument();
        if (document is null)
        {
            return;
        }

        foreach (HtmlNode link in document.DocumentNode
            .Descendants("link")
            .Where(node => node.GetAttributeValue("rel", string.Empty)
                .Contains("alternate", StringComparison.OrdinalIgnoreCase)))
        {
            string language = link.GetAttributeValue("hreflang", string.Empty);
            if (!LanguageValidator.IsValidLanguageAndCountry(page.Website, language))
            {
                continue;
            }
            Index(page, link.GetAttributeValue("href", string.Empty));
        }
    }

    private void Index(Page page, string? url)
    {
        Uri? uri = _links.Resolve(page, url);
        if (uri is not null)
        {
            _links.Index(page, uri);
        }
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