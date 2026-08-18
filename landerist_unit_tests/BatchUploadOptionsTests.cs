using landerist_library.Application.Parsing;
using landerist_library.Infrastructure.Tasks;
using landerist_library.Infrastructure.Parsing;

namespace landerist_unit_tests;

public sealed class BatchUploadOptionsTests
{
    [Fact]
    public void Constructor_PreservesProviderSpecificLimits()
    {
        BatchUploadOptions options = new(
            BatchProvider.VertexAI,
            maxPagesPerBatch: 20,
            minPagesPerBatch: 3,
            maxInputTokens: 900,
            updateWaitingResponse: true,
            statusUpdateParallelism: 2);

        Assert.Equal(BatchProvider.VertexAI, options.Provider);
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
                BatchProvider.OpenAI,
                maxPagesPerBatch: maximum,
                minPagesPerBatch: minimum,
                maxInputTokens: 100,
                updateWaitingResponse: false));
    }
}
