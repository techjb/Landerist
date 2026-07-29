using landerist_library.Parsing;
using landerist_library.Application.Parsing;
using landerist_library.Pages;

namespace landerist_unit_tests;

public sealed class ListingParserClientCatalogTests
{
    [Fact]
    public void TryGet_ReturnsClientForRequestedProvider()
    {
        StubClient openAi = new(LLMProvider.OpenAI);
        StubClient localAi = new(LLMProvider.LocalAI);
        ListingParserClientCatalog catalog = new([openAi, localAi]);

        bool found = catalog.TryGet(
            LLMProvider.LocalAI,
            out IListingParserClient? selected);

        Assert.True(found);
        Assert.Same(localAi, selected);
    }

    [Fact]
    public void TryGet_ReturnsFalseForUnregisteredProvider()
    {
        ListingParserClientCatalog catalog =
            new([new StubClient(LLMProvider.OpenAI)]);

        bool found = catalog.TryGet(
            LLMProvider.VertexAI,
            out IListingParserClient? selected);

        Assert.False(found);
        Assert.Null(selected);
    }

    [Fact]
    public void Constructor_RejectsDuplicateProvider()
    {
        Assert.Throws<ArgumentException>(() =>
            new ListingParserClientCatalog(
            [
                new StubClient(LLMProvider.OpenAI),
                new StubClient(LLMProvider.OpenAI)
            ]));
    }

    private sealed class StubClient(LLMProvider provider)
        : IListingParserClient
    {
        public LLMProvider Provider => provider;

        public ListingParserClientResult GetResponse(
            Page page,
            string userInput) =>
            new(null, WaitingAIRequest: true);
    }
}
