using landerist_library.Infrastructure.Ai.Batch;

namespace landerist_unit_tests;

public sealed class VertexBatchOptionsTests
{
    [Fact]
    public void Validate_WithCompleteConfiguration_ReturnsOptions()
    {
        VertexBatchOptions options = new(
            "{}",
            "project",
            "europe-west1",
            "gemini",
            "bucket",
            "C:\\batch");

        Assert.Same(options, options.Validate());
    }

    [Theory]
    [InlineData("", "project", "location", "model", "bucket", "batch")]
    [InlineData("{}", "", "location", "model", "bucket", "batch")]
    [InlineData("{}", "project", "", "model", "bucket", "batch")]
    [InlineData("{}", "project", "location", "", "bucket", "batch")]
    [InlineData("{}", "project", "location", "model", "", "batch")]
    [InlineData("{}", "project", "location", "model", "bucket", "")]
    public void Validate_WithMissingValue_Throws(
        string credential,
        string project,
        string location,
        string model,
        string bucket,
        string directory)
    {
        VertexBatchOptions options = new(
            credential,
            project,
            location,
            model,
            bucket,
            directory);

        Assert.Throws<ArgumentException>(() => options.Validate());
    }
}
