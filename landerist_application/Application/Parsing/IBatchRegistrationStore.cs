namespace landerist_library.Application.Parsing;

public interface IBatchRegistrationStore
{
    bool Register(
        string batchId,
        IReadOnlySet<string> pageUriHashes,
        BatchProvider provider);
}
