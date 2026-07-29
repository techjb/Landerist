using landerist_library.Application.Logging;

namespace landerist_library.Infrastructure.Ai.OpenAI.Batch;

public sealed class OpenAIBatchArtifactCleaner(
    OpenAIBatchClient client,
    IApplicationLogger logger,
    int retentionDays,
    TimeProvider timeProvider)
{
    public bool DeleteBatchFiles(string batchId)
    {
        var batch = client.GetBatch(batchId);
        return batch is not null &&
               OpenAIBatchClient.IsCompleted(batch) &&
               client.DeleteFile(batch.InputFileId) &&
               client.DeleteFile(batch.OutputFileId) &&
               client.DeleteFile(batch.ErrorFileId);
    }

    public void DeleteExpiredFiles()
    {
        DateTime cutoff = timeProvider.GetLocalNow().DateTime.AddDays(retentionDays);
        foreach (var file in client.ListFiles().Where(file => file.CreatedAt < cutoff))
        {
            if (!client.DeleteFile(file.Id))
            {
                logger.WriteError(nameof(OpenAIBatchArtifactCleaner), $"Could not delete {file.Id}.");
            }
        }
    }
}
