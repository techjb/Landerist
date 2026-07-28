using landerist_library.Infrastructure.Parsing;
using landerist_library.Infrastructure.Tasks;
using landerist_library.Parse.ListingParser;

namespace landerist_library.Infrastructure.Sql;

public sealed class LegacyBatchRegistrationStore(BatchRepository batches)
    : IBatchRegistrationStore
{
    public bool Register(
        string batchId,
        IReadOnlySet<string> pageUriHashes,
        BatchProvider provider) =>
        batches.Insert(
            batchId,
            [.. pageUriHashes],
            provider switch
            {
                BatchProvider.OpenAI => LLMProvider.OpenAI,
                BatchProvider.VertexAI => LLMProvider.VertexAI,
                _ => throw new ArgumentOutOfRangeException(nameof(provider))
            });
}
