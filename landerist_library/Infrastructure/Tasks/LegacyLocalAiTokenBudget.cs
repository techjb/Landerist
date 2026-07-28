using landerist_library.Parse.ListingParser;

namespace landerist_library.Infrastructure.Tasks;

public sealed class LegacyLocalAiTokenBudget(Tokenizer tokenizer)
    : ILocalAiTokenBudget
{
    public int Calculate(LocalAiParsingTaskOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return options.ModelMaxTokens -
            (tokenizer.CountSystemTokens() + options.CompletionTokens);
    }
}
