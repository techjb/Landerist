using landerist_library.Infrastructure.Parsing;

namespace landerist_library.Infrastructure.Tasks;

public sealed record BatchRecord(
    DateTime Created,
    BatchProvider Provider,
    string Id,
    IReadOnlySet<string> PageUriHashes,
    bool Downloaded);

public interface IBatchStore
{
    IReadOnlyList<BatchRecord> Select(bool downloaded);

    bool Delete(string batchId);

    bool MarkDownloaded(string batchId);
}
