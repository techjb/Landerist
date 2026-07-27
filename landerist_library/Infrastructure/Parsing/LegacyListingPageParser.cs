using landerist_library.Websites;
using landerist_library.Application.Parsing;
using landerist_library.Pages;
using landerist_library.Parse.ListingParser;
using landerist_library.Application.Statistics;
using landerist_library.Application.Websites;
using landerist_orels.ES;

namespace landerist_library.Infrastructure.Parsing;

public sealed class LegacyListingPageParser(HostStatistics statistics, ParseListing parser) : IListingPageParser
{
    public (PageType pageType, Listing? listing, bool waitingAIRequest) Parse(Page page) =>
        parser.Parse(page, statistics);
}

public sealed class LegacyPageTokenLimitPolicy : IPageTokenLimitPolicy
{
    public bool TooManyTokens(Page page) => Tokenizer.TooManyTokens(page);
}
