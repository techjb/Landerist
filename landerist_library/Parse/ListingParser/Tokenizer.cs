using landerist_library.Pages;
using SharpToken;

namespace landerist_library.Parse.ListingParser;

public sealed class Tokenizer
{
    private readonly GptEncoding _encoding;
    private readonly int _maxContextWindow;

    public Tokenizer(TokenizerOptions? options = null)
    {
        options ??= new TokenizerOptions();
        _encoding = GptEncoding.GetEncoding(options.EncodingName);
        _maxContextWindow = options.MaxContextWindow;
    }

    public int CountSystemTokens() => _encoding.CountTokens(SystemPrompt.Text);

    public (int PageTokens, int SystemTokens) CountPageAndSystemTokens(Page page)
    {
        ArgumentNullException.ThrowIfNull(page);
        string? userInput = page.ListingParserInput;
        int pageTokens = string.IsNullOrWhiteSpace(userInput)
            ? 0
            : _encoding.CountTokens(userInput);
        return (pageTokens, CountSystemTokens());
    }

    public bool TooManyTokens(Page page)
    {
        var (pageTokens, systemTokens) = CountPageAndSystemTokens(page);
        page.TokenCount = pageTokens;
        return systemTokens + pageTokens > _maxContextWindow;
    }
}