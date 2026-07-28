using landerist_library.Application.Parsing;
using landerist_library.Pages;

namespace landerist_library.Infrastructure.Parsing;

public sealed class PageListingInputPreparer : IListingInputPreparer
{
    public void Prepare(Page page) => page.SetListingParserInput();

    public bool MatchesUnavailableRule(Page page) =>
        page.MatchesWebsiteListingUnavailableRule();
}
