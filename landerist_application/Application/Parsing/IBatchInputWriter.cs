using landerist_library.Pages;

namespace landerist_library.Application.Parsing;

public sealed record BatchInputWriteResult(
    string? FilePath,
    IReadOnlySet<string> WrittenPageHashes,
    IReadOnlySet<string> InvalidPageHashes);

public interface IBatchInputWriter
{
    BatchInputWriteResult Write(IReadOnlyList<Page> pages);
}
