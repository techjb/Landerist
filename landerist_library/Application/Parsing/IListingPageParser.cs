using landerist_library.Pages;
using landerist_orels.ES;

namespace landerist_library.Application.Parsing;

public interface IListingPageParser
{
    (PageType pageType, Listing? listing, bool waitingAIRequest) Parse(Page page);
}

public interface IPageTokenLimitPolicy
{
    bool TooManyTokens(Page page);
}
