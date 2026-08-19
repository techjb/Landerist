using landerist_library.Infrastructure.Ai.LocalAI;

namespace landerist_unit_tests;

public sealed class LocalAIListingParserOptionsTests
{
    [Fact]
    public void Validate_AcceptsDefaults()
    {
        LocalAIListingParserOptions options = new("localhost");

        Assert.Same(options, options.Validate());
        Assert.Equal(8000, options.Port);
        Assert.Equal(4000, options.MaxCompletionTokens);
        Assert.Equal(60000, options.MaxContextWindow);
    }

    [Theory]
    [InlineData("", 8000)]
    [InlineData(" ", 8000)]
    [InlineData("localhost", 0)]
    [InlineData("localhost", 65536)]
    public void Validate_RejectsInvalidEndpoint(string host, int port)
    {
        LocalAIListingParserOptions options = new(host, port);

        Assert.ThrowsAny<ArgumentException>(options.Validate);
    }
}
