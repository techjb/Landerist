using landerist_library.Application.Listings;
using landerist_library.Application.Scraping;
using landerist_library.Pages;
using landerist_library.Websites;

namespace landerist_library.Infrastructure.Scraping;

public sealed class PageIndexingService : IPageIndexingService
{
    private readonly bool _enabled;
    private readonly IPageLinkService _links;
    private readonly IPageLinkExtractor _extractor;

    public PageIndexingService(
        bool enabled,
        IPageLinkService links,
        IPageLinkExtractor extractor)
    {
        ArgumentNullException.ThrowIfNull(links);
        ArgumentNullException.ThrowIfNull(extractor);
        _enabled = enabled;
        _links = links;
        _extractor = extractor;
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
            Uri? canonical = _extractor.GetCanonicalUri(page);
            if (canonical is not null)
            {
                _links.Index(page, canonical);
            }
            return;
        }

        if (_extractor.ContainsMetaRobotsNoFollow(page) ||
            !page.Website.HtmlIndexingEnabled)
        {
            return;
        }

        foreach (string url in _extractor.GetFollowedLinks(page))
        {
            Index(page, url);
        }
    }

    private void IndexLanguageAlternates(Page page)
    {
        foreach (PageLanguageAlternate alternate in
            _extractor.GetLanguageAlternates(page))
        {
            if (!LanguageValidator.IsValidLanguageAndCountry(
                page.Website,
                alternate.Language))
            {
                continue;
            }

            Index(page, alternate.Url);
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
}
