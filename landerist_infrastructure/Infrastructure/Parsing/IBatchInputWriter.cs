using landerist_library.Application.Parsing;
using landerist_library.Pages;

namespace landerist_library.Infrastructure.Parsing;

public sealed record BatchInputWriterOptions(
    BatchProvider Provider,
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
