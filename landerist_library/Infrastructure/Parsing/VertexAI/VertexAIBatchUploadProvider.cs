using landerist_library.Pages;
using landerist_library.Parse.ListingParser;
using landerist_library.Parse.ListingParser.VertexAI.Batch;

namespace landerist_library.Infrastructure.Parsing.VertexAI;

public sealed class VertexAIBatchUploadProvider : IListingBatchUploadProvider
{
    public LLMProvider Provider => LLMProvider.VertexAI;

    public string? Serialize(Page page, string userInput) =>
        VertexAIBatchUpload.GetJson(page, userInput);

    public string? UploadFile(string filePath) =>
        CloudStorage.UploadFile(filePath);

    public string? CreateBatch(string fileId) =>
        BatchPredictions.CreateBatch(fileId);
}
