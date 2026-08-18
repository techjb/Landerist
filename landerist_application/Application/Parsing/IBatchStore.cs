namespace landerist_library.Application.Parsing;

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
