using landerist_library.Pages;

namespace landerist_library.Application.Parsing;

public interface IPageContentInspector
{
    bool ContainsMetaRobotsNoIndex(Page page);

    bool IsNotCanonical(Page page);

    Uri? GetCanonicalUri(Page page);

    bool HasIncorrectLanguage(Page page);

    void PrepareListingParserInput(Page page);

    bool MatchesListingUnavailableRule(Page page);
}