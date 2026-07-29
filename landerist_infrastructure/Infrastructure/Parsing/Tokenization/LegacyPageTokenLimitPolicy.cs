using landerist_library.Application.Parsing;
using landerist_library.Pages;

namespace landerist_library.Infrastructure.Parsing.Tokenization;

public sealed class LegacyPageTokenLimitPolicy(Tokenizer tokenizer) : IPageTokenLimitPolicy
{
    public bool TooManyTokens(Page page) => tokenizer.TooManyTokens(page);
}
