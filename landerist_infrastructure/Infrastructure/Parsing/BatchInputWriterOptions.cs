using landerist_library.Application.Parsing;
namespace landerist_library.Infrastructure.Parsing;

public sealed record BatchInputWriterOptions(
    BatchProvider Provider,
    string Directory,
    long MaxFileSizeInBytes,
    int MinPagesPerBatch);
