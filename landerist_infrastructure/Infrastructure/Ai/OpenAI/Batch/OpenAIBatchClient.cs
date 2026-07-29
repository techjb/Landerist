using landerist_library.Application.Logging;
using OpenAI;
using OpenAI.Batch;
using OpenAI.Files;

namespace landerist_library.Infrastructure.Ai.OpenAI.Batch;

public sealed record OpenAIBatchOptions(
    string ApiKey,
    string Model,
    string LocalDirectory)
{
    public OpenAIBatchOptions Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ApiKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(Model);
        ArgumentException.ThrowIfNullOrWhiteSpace(LocalDirectory);
        return this;
    }
}

public sealed class OpenAIBatchClient
{
    private readonly OpenAIBatchOptions _options;
    private readonly IApplicationLogger _logger;
    private readonly OpenAIClient _client;

    public OpenAIBatchClient(OpenAIBatchOptions options, IApplicationLogger logger)
    {
        _options = options.Validate();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _client = new OpenAIClient(_options.ApiKey);
    }

    public bool DeleteFile(string? fileId)
    {
        if (string.IsNullOrWhiteSpace(fileId))
        {
            return true;
        }

        try
        {
            _client.FilesEndpoint.DeleteFileAsync(fileId).GetAwaiter().GetResult();
            return true;
        }
        catch (Exception exception)
        {
            _logger.WriteError(nameof(DeleteFile), exception.ToString());
            return false;
        }
    }

    public BatchResponse? GetBatch(string batchId)
    {
        try
        {
            return _client.BatchEndpoint.RetrieveBatchAsync(batchId).GetAwaiter().GetResult();
        }
        catch (Exception exception)
        {
            _logger.WriteError(nameof(GetBatch), exception.ToString());
            return null;
        }
    }

    public static bool IsCompleted(BatchResponse batch) =>
        batch.Status.Equals(BatchStatus.Completed);

    public string? DownloadFile(string? fileId)
    {
        if (string.IsNullOrWhiteSpace(fileId))
        {
            return null;
        }

        try
        {
            return _client.FilesEndpoint
                .DownloadFileAsync(fileId, _options.LocalDirectory)
                .GetAwaiter()
                .GetResult();
        }
        catch (Exception exception)
        {
            _logger.WriteError(nameof(DownloadFile), exception.ToString());
            return null;
        }
    }

    public IReadOnlyList<FileResponse> ListFiles()
    {
        try
        {
            return [.. _client.FilesEndpoint.ListFilesAsync().GetAwaiter().GetResult()];
        }
        catch (Exception exception)
        {
            _logger.WriteError(nameof(ListFiles), exception.ToString());
            return [];
        }
    }

    public string? UploadFile(string filePath)
    {
        try
        {
            return _client.FilesEndpoint
                .UploadFileAsync(filePath, FilePurpose.Batch)
                .GetAwaiter()
                .GetResult()
                .Id;
        }
        catch (Exception exception)
        {
            _logger.WriteError(nameof(UploadFile), exception.ToString());
            return null;
        }
    }

    public string? CreateBatch(string fileId)
    {
        try
        {
            CreateBatchRequest request = new(fileId, Endpoint.ChatCompletions);
            return _client.BatchEndpoint
                .CreateBatchAsync(request)
                .GetAwaiter()
                .GetResult()
                .Id;
        }
        catch (Exception exception)
        {
            _logger.WriteError(nameof(CreateBatch), exception.ToString());
            return null;
        }
    }
}
