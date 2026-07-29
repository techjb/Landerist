using landerist_library.Parsing;
using landerist_library.Parse.ListingParser.LocalAI;
using landerist_library.Parse.ListingParser.OpenAI;
using landerist_library.Parse.ListingParser.VertexAI;

namespace landerist_library.Parse.ListingParser;

public sealed record TokenizerOptions
{
    public const string DefaultEncodingName = "o200k_harmony";
    public const int DefaultMaxContextWindow = 128000;

    public TokenizerOptions(
        int maxContextWindow = DefaultMaxContextWindow,
        string encodingName = DefaultEncodingName)
    {
        if (maxContextWindow <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxContextWindow));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(encodingName);
        MaxContextWindow = maxContextWindow;
        EncodingName = encodingName;
    }

    public int MaxContextWindow { get; }

    public string EncodingName { get; }

    public static TokenizerOptions ForProvider(LLMProvider provider) =>
        new(provider switch
        {
            LLMProvider.OpenAI => OpenAIRequest.MAX_CONTEXT_WINDOW,
            LLMProvider.VertexAI => 128000,
            LLMProvider.LocalAI => LocalAIRequest.MAX_CONTEXT_WINDOW,
            _ => DefaultMaxContextWindow
        });
}
