using landerist_library.Pages;
using landerist_library.Parse.ListingParser;
using landerist_library.Parse.ListingParser.OpenAI.Batch;

namespace landerist_library.Infrastructure.Parsing.OpenAI;

public sealed class OpenAIBatchUploadProvider : IListingBatchUploadProvider
{
    public BatchProvider Provider => BatchProvider.OpenAI;

    public string? Serialize(Page page, string userInput) =>
        OpenAIBatchUpload.GetJson(page, userInput);

    public string? UploadFile(string filePath) =>
        OpenAIBatchClient.UploadFile(filePath);

    public string? CreateBatch(string fileId) =>
        OpenAIBatchClient.CreateBatch(fileId);
}
