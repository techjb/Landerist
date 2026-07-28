using landerist_library.Infrastructure.Parsing;

namespace landerist_library.Infrastructure.Tasks;

public interface IBatchRegistrationStore
{
    bool Register(
        string batchId,
        IReadOnlySet<string> pageUriHashes,
        BatchProvider provider);
}
