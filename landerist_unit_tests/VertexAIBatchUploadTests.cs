using System.Text.Json;
using landerist_library.Infrastructure.Ai.Batch;
using landerist_library.Pages;

namespace landerist_unit_tests;

public sealed class VertexAIBatchUploadTests
{
    [Fact]
    public void GetJson_UsesInjectedPromptSchemaAndPageLabel()
    {
        Page page = new("https://example.com/listing/1")
        {
            UriHash = "page-hash",
        };
        Dictionary<string, object> schema = new()
        {
            ["type"] = "object",
        };

        string? json = VertexAIBatchUpload.GetJson(
            page,
            "listing input",
            "system prompt",
            schema);

        using JsonDocument document = JsonDocument.Parse(json!);
        JsonElement request = document.RootElement.GetProperty("request");
        Assert.Equal(
            "system prompt",
            request
                .GetProperty("system_instruction")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString());
        Assert.Equal(
            "page-hash",
            request
                .GetProperty("labels")
                .GetProperty("uri_hash")
                .GetString());
        Assert.Equal(
            "object",
            request
                .GetProperty("generation_config")
                .GetProperty("response_schema")
                .GetProperty("type")
                .GetString());
    }
}
