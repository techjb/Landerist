using landerist_library.Infrastructure.Ai;

namespace landerist_unit_tests;

public sealed class VertexListingParserOptionsTests
{
    [Fact]
    public void Validate_WithCompleteConfiguration_ReturnsOptions()
    {
        VertexListingParserOptions options = new(
            "{}",
            "project",
            "europe-west1",
            "google",
            "gemini");

        Assert.Same(options, options.Validate());
    }

    [Fact]
    public void Validate_WithoutModel_Throws()
    {
        VertexListingParserOptions options = new(
            "{}",
            "project",
            "europe-west1",
            "google",
            "");

        Assert.Throws<ArgumentException>(() => options.Validate());
    }
}
