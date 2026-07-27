using landerist_library.Application.Parsing;
using landerist_library.Pages;

namespace landerist_library.Infrastructure.Parsing;

public sealed class HtmlPageContentInspector : IPageContentInspector
{
    public bool ContainsMetaRobotsNoIndex(Page page) =>
        page.ContainsMetaRobotsNoIndex();

    public bool IsNotCanonical(Page page) => page.NotCanonical();

    public bool HasIncorrectLanguage(Page page) => page.IncorrectLanguage();

    public void PrepareListingParserInput(Page page) =>
        page.SetListingParserInput();

    public bool MatchesListingUnavailableRule(Page page) =>
        page.MatchesWebsiteListingUnavailableRule();
}