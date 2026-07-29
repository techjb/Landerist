using landerist_library.Application.Logging;
using landerist_library.Application.Pages;
using landerist_library.Application.Parsing;
using landerist_library.Pages;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace landerist_library.Infrastructure.Ai.OpenAI.Batch;

public sealed class OpenAIBatchDownload(
    OpenAIBatchClient client,
    IApplicationLogger logger) : IListingBatchProvider
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, false) },
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    public (string? fileSuccess, string? fileError)? GetFiles(string batchId)
    {
        var batch = client.GetBatch(batchId);
        return batch is not null && OpenAIBatchClient.IsCompleted(batch)
            ? (batch.OutputFileId, batch.ErrorFileId)
            : null;
    }

    public string? DownloadFile(string file) => client.DownloadFile(file);

    public (Page page, string? text)? ReadLine(
        string batchId,
        string line,
        IPageCatalog pages)
    {
        OpenAIBatchResponse? result;
        try
        {
            result = JsonSerializer.Deserialize<OpenAIBatchResponse>(line, SerializerOptions);
        }
        catch (Exception exception)
        {
            logger.WriteError(nameof(OpenAIBatchDownload), exception.ToString());
            return null;
        }

        if (result is null)
        {
            logger.WriteError(nameof(OpenAIBatchDownload), "Batch response is null.");
            return null;
        }

        Page? page = pages.GetByHash(result.CustomId);
        if (page is null)
        {
            logger.WriteError(nameof(OpenAIBatchDownload), $"Page not found: {result.CustomId}");
            return null;
        }

        string? text = result.Response is { StatusCode: 200, Body.FirstChoice: not null }
            ? result.Response.Body.FirstChoice
            : null;

        if (text is null)
        {
            logger.WriteError(nameof(OpenAIBatchDownload), $"Invalid response: {result.CustomId}");
        }

        return (page, text);
    }
}
