using landerist_library.Parsing;

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
            LLMProvider.OpenAI => 128000,
            LLMProvider.VertexAI => 128000,
            LLMProvider.LocalAI => 48000,
            _ => DefaultMaxContextWindow
        });
}
