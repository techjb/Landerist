using landerist_library.Pages;
using landerist_library.Infrastructure.Parsing;

namespace landerist_library.Infrastructure.Ai.OpenAI.Batch;

public sealed class OpenAIBatchUploadProvider(
    OpenAIBatchUpload serializer,
    OpenAIBatchClient client) : IListingBatchUploadProvider
{
    public BatchProvider Provider => BatchProvider.OpenAI;

    public string? Serialize(Page page, string userInput) =>
        serializer.Serialize(page, userInput);

    public string? UploadFile(string filePath) =>
        client.UploadFile(filePath);

    public string? CreateBatch(string fileId) =>
        client.CreateBatch(fileId);
}
