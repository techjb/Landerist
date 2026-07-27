using landerist_library.Infrastructure.Parsing;
using landerist_library.Pages;
using landerist_library.Parse.ListingParser;

namespace landerist_unit_tests;

public sealed class ListingBatchUploadProviderCatalogTests
{
    [Fact]
    public void GetRequired_ReturnsProviderForRequestedModel()
    {
        StubProvider vertexAi = new(LLMProvider.VertexAI);
        ListingBatchUploadProviderCatalog catalog =
            new([new StubProvider(LLMProvider.OpenAI), vertexAi]);

        IListingBatchUploadProvider selected =
            catalog.GetRequired(LLMProvider.VertexAI);

        Assert.Same(vertexAi, selected);
    }

    [Fact]
    public void GetRequired_RejectsUnregisteredProvider()
    {
        ListingBatchUploadProviderCatalog catalog =
            new([new StubProvider(LLMProvider.OpenAI)]);

        Assert.Throws<InvalidOperationException>(() =>
            catalog.GetRequired(LLMProvider.LocalAI));
    }

    [Fact]
    public void Constructor_RejectsDuplicateProvider()
    {
        Assert.Throws<ArgumentException>(() =>
            new ListingBatchUploadProviderCatalog(
            [
                new StubProvider(LLMProvider.OpenAI),
                new StubProvider(LLMProvider.OpenAI)
            ]));
    }

    private sealed class StubProvider(LLMProvider provider)
        : IListingBatchUploadProvider
    {
        public LLMProvider Provider => provider;
        public string? Serialize(Page page, string userInput) => userInput;
        public string? UploadFile(string filePath) => filePath;
        public string? CreateBatch(string fileId) => fileId;
    }
}
