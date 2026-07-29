using landerist_library.Infrastructure.Ai.OpenAI;

namespace landerist_unit_tests;

public sealed class OpenAIListingParserOptionsTests
{
    [Fact]
    public void Validate_AcceptsCompleteConfiguration()
    {
        OpenAIListingParserOptions options = new("api-key");

        Assert.Same(options, options.Validate());
        Assert.Equal(OpenAIListingParserOptions.DefaultModel, options.Model);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_RejectsMissingApiKey(string apiKey)
    {
        OpenAIListingParserOptions options = new(apiKey);

        Assert.Throws<ArgumentException>(options.Validate);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_RejectsMissingModel(string model)
    {
        OpenAIListingParserOptions options = new("api-key", model);

        Assert.Throws<ArgumentException>(options.Validate);
    }
}
