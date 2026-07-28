using landerist_library.Infrastructure.Parsing;
using landerist_library.Infrastructure.Sql;
using landerist_library.Infrastructure.Tasks;
using System.Data;

namespace landerist_unit_tests;

public sealed class SqlBatchStoreTests
{
    [Fact]
    public void Select_MapsBatchProviderAndPageHashes()
    {
        RecordingDatabase database = new();
        AddSchema(database.TableResult);
        DateTime created = new(2026, 7, 28, 10, 0, 0);
        database.TableResult.Rows.Add(
            created,
            "VertexAI",
            "batch-id",
            "page-1,page-2",
            true);
        SqlBatchStore store = new(database);

        BatchRecord batch = Assert.Single(store.Select(downloaded: true));

        Assert.Equal(created, batch.Created);
        Assert.Equal(BatchProvider.VertexAI, batch.Provider);
        Assert.Equal("batch-id", batch.Id);
        Assert.Equal(["page-1", "page-2"], batch.PageUriHashes);
        Assert.True(batch.Downloaded);
        Assert.Equal(true, database.LastParameters!["Downloaded"]);
    }

    [Fact]
    public void MarkDownloaded_UpdatesOnlyRequestedBatch()
    {
        RecordingDatabase database = new() { QueryResult = true };
        SqlBatchStore store = new(database);

        bool result = store.MarkDownloaded("batch-id");

        Assert.True(result);
        Assert.Equal("batch-id", database.LastParameters!["Id"]);
        Assert.Equal(true, database.LastParameters["Downloaded"]);
        Assert.Contains("UPDATE [BATCHES]", database.LastQuery);
    }

    private static void AddSchema(DataTable table)
    {
        table.Columns.Add("Created", typeof(DateTime));
        table.Columns.Add("LLMProvider", typeof(string));
        table.Columns.Add("Id", typeof(string));
        table.Columns.Add("PagesUriHashes", typeof(string));
        table.Columns.Add("Downloaded", typeof(bool));
    }
}
