using landerist_library.Pages;
using landerist_library.Parse.ListingParser;

namespace landerist_library.Infrastructure.Parsing;

public sealed record BatchInputWriterOptions(
    LLMProvider Provider,
    string Directory,
    long MaxFileSizeInBytes,
    int MinPagesPerBatch);

public sealed record BatchInputWriteResult(
    string? FilePath,
    IReadOnlySet<string> WrittenPageHashes,
    IReadOnlySet<string> InvalidPageHashes);

public interface IBatchInputWriter
{
    BatchInputWriteResult Write(IReadOnlyList<Page> pages);
}
