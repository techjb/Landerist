using landerist_library.Infrastructure.Ai.OpenAI.Batch;

namespace landerist_unit_tests;

public sealed class OpenAIBatchOptionsTests
{
    [Fact]
    public void Validate_AcceptsCompleteConfiguration()
    {
        OpenAIBatchOptions options = new("api-key", "model", "batch-directory");

        Assert.Same(options, options.Validate());
    }

    [Theory]
    [InlineData("", "model", "directory")]
    [InlineData("key", "", "directory")]
    [InlineData("key", "model", "")]
    [InlineData(" ", "model", "directory")]
    [InlineData("key", " ", "directory")]
    [InlineData("key", "model", " ")]
    public void Validate_RejectsIncompleteConfiguration(
        string apiKey,
        string model,
        string directory)
    {
        OpenAIBatchOptions options = new(apiKey, model, directory);

        Assert.Throws<ArgumentException>(options.Validate);
    }
}
