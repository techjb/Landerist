using landerist_library.Application.Parsing;
using landerist_library.Infrastructure.Parsing;
using landerist_library.Pages;

namespace landerist_unit_tests;

public sealed class ListingBatchUploadProviderCatalogTests
{
    [Fact]
    public void GetRequired_ReturnsProviderForRequestedModel()
    {
        StubProvider vertexAi = new(BatchProvider.VertexAI);
        ListingBatchUploadProviderCatalog catalog =
            new([new StubProvider(BatchProvider.OpenAI), vertexAi]);

        IListingBatchUploadProvider selected =
            catalog.GetRequired(BatchProvider.VertexAI);

        Assert.Same(vertexAi, selected);
    }

    [Fact]
    public void GetRequired_RejectsUnregisteredProvider()
    {
        ListingBatchUploadProviderCatalog catalog =
            new([new StubProvider(BatchProvider.OpenAI)]);

        Assert.Throws<InvalidOperationException>(() =>
            catalog.GetRequired((BatchProvider)999));
    }

    [Fact]
    public void Constructor_RejectsDuplicateProvider()
    {
        Assert.Throws<ArgumentException>(() =>
            new ListingBatchUploadProviderCatalog(
            [
                new StubProvider(BatchProvider.OpenAI),
                new StubProvider(BatchProvider.OpenAI)
            ]));
    }

    private sealed class StubProvider(BatchProvider provider)
        : IListingBatchUploadProvider
    {
        public BatchProvider Provider => provider;
        public string? Serialize(Page page, string userInput) => userInput;
        public string? UploadFile(string filePath) => filePath;
        public string? CreateBatch(string fileId) => fileId;
    }
}
