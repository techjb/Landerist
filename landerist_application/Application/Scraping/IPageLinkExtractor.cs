using landerist_library.Pages;

namespace landerist_library.Application.Scraping;

public sealed record PageLanguageAlternate(string Language, string Url);

public interface IPageLinkExtractor
{
    bool ContainsMetaRobotsNoFollow(Page page);

    Uri? GetCanonicalUri(Page page);

    IReadOnlyList<string> GetFollowedLinks(Page page);

    IReadOnlyList<PageLanguageAlternate> GetLanguageAlternates(Page page);
}
