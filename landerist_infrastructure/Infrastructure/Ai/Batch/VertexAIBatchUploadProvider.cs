using landerist_library.Pages;
using landerist_library.Application.Parsing;

namespace landerist_library.Infrastructure.Ai.Batch;

public sealed class VertexAIBatchUploadProvider(
    string systemPrompt,
    object responseSchema,
    Func<string, string?> uploadFile,
    Func<string, string?> createBatch) : IListingBatchUploadProvider
{
    public BatchProvider Provider => BatchProvider.VertexAI;

    public string? Serialize(Page page, string userInput) =>
        VertexAIBatchUpload.GetJson(
            page,
            userInput,
            systemPrompt,
            responseSchema);

    public string? UploadFile(string filePath) =>
        uploadFile(filePath);

    public string? CreateBatch(string fileId) =>
        createBatch(fileId);
}
