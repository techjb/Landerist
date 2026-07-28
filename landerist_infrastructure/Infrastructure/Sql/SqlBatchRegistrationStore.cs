using landerist_library.Database;
using landerist_library.Infrastructure.Parsing;
using landerist_library.Infrastructure.Tasks;

namespace landerist_library.Infrastructure.Sql;

public sealed class SqlBatchRegistrationStore(IDatabase database)
    : IBatchRegistrationStore
{
    public bool Register(
        string batchId,
        IReadOnlySet<string> pageUriHashes,
        BatchProvider provider)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(batchId);
        ArgumentNullException.ThrowIfNull(pageUriHashes);

        const string query =
            "INSERT INTO [BATCHES] " +
            "VALUES (GETDATE(), @LLMProvider, @Id, @PagesUriHashes, 0)";

        return database.Query(query, new Dictionary<string, object?>
        {
            ["LLMProvider"] = provider.ToString(),
            ["Id"] = batchId,
            ["PagesUriHashes"] = string.Join(",", pageUriHashes)
        });
    }
}
