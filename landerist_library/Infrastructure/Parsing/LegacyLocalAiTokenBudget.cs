using landerist_library.Parse.ListingParser;
using landerist_library.Infrastructure.Tasks;

namespace landerist_library.Infrastructure.Parsing;

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
