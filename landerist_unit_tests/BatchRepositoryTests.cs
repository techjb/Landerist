using landerist_library.Infrastructure.Sql;
using landerist_library.Parse.ListingParser;
using System.Data;

namespace landerist_unit_tests;

public sealed class BatchRepositoryTests
{
    [Fact]
    public void Insert_DelegatesProviderIdAndPageHashes()
    {
        RecordingDatabase database = new() { QueryResult = true };
        BatchRepository repository = new(database);
        HashSet<string> pageHashes = ["page-hash"];

        bool result = repository.Insert(
            "batch-id",
            pageHashes,
            LLMProvider.VertexAI);

        Assert.True(result);
        Assert.Equal("VertexAI", database.LastParameters!["LLMProvider"]);
        Assert.Equal("batch-id", database.LastParameters["Id"]);
        Assert.Equal("page-hash", database.LastParameters["PagesUriHashes"]);
        Assert.Contains("INSERT INTO [BATCHES]", database.LastQuery);
    }

    [Fact]
    public void Delete_DelegatesIdAndResult()
    {
        RecordingDatabase database = new() { QueryResult = true };
        BatchRepository repository = new(database);

        bool result = repository.Delete("batch-id");

        Assert.True(result);
        Assert.Equal("batch-id", database.LastParameters!["Id"]);
        Assert.Contains("DELETE FROM [BATCHES]", database.LastQuery);
    }

    [Fact]
    public void Select_MapsRowsAndDelegatesDownloadedFilter()
    {
        RecordingDatabase database = new();
        AddBatchTableSchema(database.TableResult);
        DateTime created = new(2026, 7, 22, 10, 30, 0);
        database.TableResult.Rows.Add(
            created,
            "OpenAI",
            "batch-id",
            "page-1,page-2",
            false);
        BatchRepository repository = new(database);

        List<landerist_library.Database.Batch> result =
            repository.Select(downloaded: false);

        landerist_library.Database.Batch batch = Assert.Single(result);
        Assert.Equal(created, batch.Created);
        Assert.Equal(LLMProvider.OpenAI, batch.LLMProvider);
        Assert.Equal("batch-id", batch.Id);
        Assert.Equal(["page-1", "page-2"], batch.PagesUriHashes);
        Assert.False(batch.Downloaded);
        Assert.Equal(false, database.LastParameters!["Downloaded"]);
        Assert.Contains("ORDER BY [Created] ASC", database.LastQuery);
    }

    [Fact]
    public void SelectById_ReturnsNullWhenDatabaseHasNoRows()
    {
        RecordingDatabase database = new();
        AddBatchTableSchema(database.TableResult);
        BatchRepository repository = new(database);

        landerist_library.Database.Batch? result =
            repository.Select("missing-batch");

        Assert.Null(result);
        Assert.Equal("missing-batch", database.LastParameters!["Id"]);
        Assert.Contains("SELECT TOP 1", database.LastQuery);
    }

    [Fact]
    public void SelectAll_UsesValidWhitespaceAndReturnsDatabaseList()
    {
        RecordingDatabase database = new();
        database.ListStringResult.Add("batch-id");
        BatchRepository repository = new(database);

        List<string> result = repository.SelectAll(LLMProvider.VertexAI);

        Assert.Same(database.ListStringResult, result);
        Assert.Equal("VertexAI", database.LastParameters!["LLMProvider"]);
        Assert.Contains("@LLMProvider ORDER BY", database.LastQuery);
        Assert.DoesNotContain("@LLMProviderORDER BY", database.LastQuery);
    }

    [Fact]
    public void Update_DelegatesIdDownloadedFlagAndResult()
    {
        RecordingDatabase database = new() { QueryResult = true };
        BatchRepository repository = new(database);

        bool result = repository.Update("batch-id", downloaded: true);

        Assert.True(result);
        Assert.Equal("batch-id", database.LastParameters!["Id"]);
        Assert.Equal(true, database.LastParameters["Downloaded"]);
        Assert.Contains("UPDATE [BATCHES]", database.LastQuery);
    }

    private static void AddBatchTableSchema(DataTable table)
    {
        table.Columns.Add("Created", typeof(DateTime));
        table.Columns.Add("LLMProvider", typeof(string));
        table.Columns.Add("Id", typeof(string));
        table.Columns.Add("PagesUriHashes", typeof(string));
        table.Columns.Add("Downloaded", typeof(bool));
    }
}
