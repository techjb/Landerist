using landerist_library.Infrastructure.Tasks;
using landerist_library.Parse.ListingParser;

namespace landerist_unit_tests;

public sealed class BatchUploadOptionsTests
{
    [Fact]
    public void Constructor_PreservesProviderSpecificLimits()
    {
        BatchUploadOptions options = new(
            LLMProvider.VertexAI,
            "batch",
            maxFileSizeInBytes: 200,
            maxPagesPerBatch: 20,
            minPagesPerBatch: 3,
            maxInputTokens: 900,
            updateWaitingResponse: true,
            statusUpdateParallelism: 2);

        Assert.Equal(LLMProvider.VertexAI, options.Provider);
        Assert.Equal("batch", options.Directory);
        Assert.Equal(200, options.MaxFileSizeInBytes);
        Assert.Equal(20, options.MaxPagesPerBatch);
        Assert.Equal(3, options.MinPagesPerBatch);
        Assert.Equal(900, options.MaxInputTokens);
        Assert.True(options.UpdateWaitingResponse);
        Assert.Equal(2, options.CreateStatusParallelOptions().MaxDegreeOfParallelism);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(3, 2)]
    public void Constructor_RejectsInvalidMinimumBatchSize(
        int minimum,
        int maximum)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new BatchUploadOptions(
                LLMProvider.OpenAI,
                "batch",
                maxFileSizeInBytes: 100,
                maxPagesPerBatch: maximum,
                minPagesPerBatch: minimum,
                maxInputTokens: 100,
                updateWaitingResponse: false));
    }
}
