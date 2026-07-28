using landerist_library.Application.Websites;
using landerist_library.Pages;
using landerist_library.Parse.ListingParser;

namespace landerist_library.Infrastructure.Parsing;

public sealed class LegacyPageTokenLimitPolicy(Tokenizer tokenizer) : IPageTokenLimitPolicy
{
    public bool TooManyTokens(Page page) => tokenizer.TooManyTokens(page);
}