using landerist_library.Infrastructure.Parsing.Tokenization;
using landerist_domain.Parsing.Tokenization;
using landerist_library.Application.Parsing;
using landerist_library.Infrastructure.Parsing;
using landerist_library.Infrastructure.Tasks;
using landerist_library.Pages;

namespace landerist_unit_tests;

public sealed class TokenizerTests
{
    [Fact]
    public void TooManyTokens_UsesInjectedContextWindow()
    {
        Page page = new("https://example.com/listing/1")
        {
            ListingParserInput = "A deliberately short listing parser input."
        };
        Tokenizer measuringTokenizer = new();
        var (pageTokens, systemTokens) =
            measuringTokenizer.CountPageAndSystemTokens(page);
        Tokenizer constrainedTokenizer = new(
            new TokenizerOptions(systemTokens + pageTokens - 1));

        bool tooMany = constrainedTokenizer.TooManyTokens(page);

        Assert.True(tooMany);
        Assert.Equal(pageTokens, page.TokenCount);
    }

    [Fact]
    public void TooManyTokens_AllowsInputWithinInjectedContextWindow()
    {
        Page page = new("https://example.com/listing/1")
        {
            ListingParserInput = "A deliberately short listing parser input."
        };
        Tokenizer measuringTokenizer = new();
        var (pageTokens, systemTokens) =
            measuringTokenizer.CountPageAndSystemTokens(page);
        Tokenizer tokenizer = new(
            new TokenizerOptions(systemTokens + pageTokens));

        Assert.False(tokenizer.TooManyTokens(page));
    }

    [Fact]
    public void LocalAiTokenBudget_UsesInjectedModelAndCompletionLimits()
    {
        Tokenizer tokenizer = new();
        int systemTokens = tokenizer.CountSystemTokens();
        LocalAiParsingTaskOptions options = new(
            completionTokens: 123,
            modelMaxTokens: systemTokens + 1000);

        int budget = new LegacyLocalAiTokenBudget(tokenizer).Calculate(options);

        Assert.Equal(877, budget);
    }
}
