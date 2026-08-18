using landerist_library.Parsing;
using landerist_library.Application.Parsing;
using landerist_library.Database;
using System.Data;

namespace landerist_library.Infrastructure.Sql;

public sealed class SqlBatchStore(IDatabase database) : IBatchStore
{
    public IReadOnlyList<BatchRecord> Select(bool downloaded)
    {
        const string query =
            "SELECT * FROM [BATCHES] " +
            "WHERE [Downloaded] = @Downloaded " +
            "ORDER BY [Created] ASC";
        DataTable table = database.QueryTable(query, new Dictionary<string, object?>
        {
            ["Downloaded"] = downloaded
        });

        return table.Rows.Cast<DataRow>()
            .Select(row => new BatchRecord(
                (DateTime)row["Created"],
                Enum.Parse<BatchProvider>((string)row["LLMProvider"]),
                (string)row["Id"],
                ((string)row["PagesUriHashes"])
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .ToHashSet(StringComparer.Ordinal),
                (bool)row["Downloaded"]))
            .ToArray();
    }

    public bool Delete(string batchId) => database.Query(
        "DELETE FROM [BATCHES] WHERE [Id] = @Id",
        new Dictionary<string, object?> { ["Id"] = batchId });

    public bool MarkDownloaded(string batchId) => database.Query(
        "UPDATE [BATCHES] SET [Downloaded] = @Downloaded WHERE [Id] = @Id",
        new Dictionary<string, object?>
        {
            ["Id"] = batchId,
            ["Downloaded"] = true
        });
}
