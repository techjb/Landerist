using System.Text.Json;
using System.Text.Json.Serialization;
using landerist_library.Application.Logging;
using landerist_library.Application.Pages;
using landerist_library.Application.Parsing;
using landerist_library.Pages;

namespace landerist_library.Infrastructure.Ai.Batch;

public sealed class VertexAIBatchDownload(
    VertexBatchJobClient jobs,
    VertexCloudStorageClient storage,
    IApplicationLogger logger) : IListingBatchProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition =
            JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {
            new JsonStringEnumConverter(
                JsonNamingPolicy.CamelCase,
                allowIntegerValues: false),
        },
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
    };

    public (string? fileSuccess, string? fileError)? GetFiles(
        string batchId)
    {
        VertexBatchJobResult? job = jobs.Get(batchId);
        if (job?.State == VertexBatchJobState.Succeeded
            && !string.IsNullOrWhiteSpace(job.OutputDirectory))
        {
            return (
                job.OutputDirectory.TrimEnd('/')
                    + "/predictions.jsonl",
                null);
        }

        return job?.State == VertexBatchJobState.Failed
            ? (null, job.InputObject)
            : null;
    }

    public string? DownloadFile(string file) =>
        storage.Download(file);

    public (Page page, string? text)? ReadLine(
        string id,
        string line,
        IPageCatalog pages)
    {
        VertexAIBatchResponse? response;
        try
        {
            response = JsonSerializer.Deserialize<VertexAIBatchResponse>(
                line,
                JsonOptions);
        }
        catch (Exception exception)
        {
            Log("ReadLine", exception.ToString());
            return null;
        }

        if (response is null)
        {
            Log("ReadLine", "Response is null.");
            return null;
        }

        Page? page = GetPage(response, pages);
        if (page is null)
        {
            Log("ReadLine", $"Page is null. Id: {id}");
            return null;
        }

        VertexAIBatchResponseCandidate? candidate =
            response.Response?.Candidates?.FirstOrDefault();
        if (candidate is null)
        {
            Log("ReadLine", "Candidate is null.");
            return (page, null);
        }

        if (!string.Equals(
                candidate.FinishReason,
                "STOP",
                StringComparison.OrdinalIgnoreCase))
        {
            Log(
                "ReadLine",
                $"Invalid finish reason: {candidate.FinishReason}");
            return (page, null);
        }

        string? text = candidate.Content?.Parts?
            .FirstOrDefault()?.Text;
        if (string.IsNullOrWhiteSpace(text))
        {
            Log(
                "ReadLine",
                $"Response text is empty. Id: {id}");
        }

        return (page, text);
    }

    private static Page? GetPage(
        VertexAIBatchResponse response,
        IPageCatalog pages)
    {
        Dictionary<string, string>? labels =
            response.Request?.labels;
        return labels is not null
            && labels.TryGetValue(
                VertexAIBatchUpload.LABEL_URIHASH,
                out string? uriHash)
            && !string.IsNullOrWhiteSpace(uriHash)
                ? pages.GetByHash(uriHash)
                : null;
    }

    private void Log(string phase, string message) =>
        logger.WriteError(
            $"{nameof(VertexAIBatchDownload)}.{phase}",
            message);
}
