using landerist_library.Infrastructure.Ai;

namespace landerist_unit_tests;

public sealed class VertexAddressSelectorOptionsTests
{
    [Fact]
    public void Validate_WithCompleteConfiguration_ReturnsOptions()
    {
        VertexAddressSelectorOptions options = new(
            "{}",
            "project",
            "europe-west1",
            "google",
            "gemini");

        Assert.Same(options, options.Validate());
    }

    [Theory]
    [InlineData("", "project", "location", "publisher", "model")]
    [InlineData("{}", "", "location", "publisher", "model")]
    [InlineData("{}", "project", "", "publisher", "model")]
    [InlineData("{}", "project", "location", "", "model")]
    [InlineData("{}", "project", "location", "publisher", "")]
    public void Validate_WithMissingValue_Throws(
        string credential,
        string project,
        string location,
        string publisher,
        string model)
    {
        VertexAddressSelectorOptions options = new(
            credential,
            project,
            location,
            publisher,
            model);

        Assert.Throws<ArgumentException>(() => options.Validate());
    }
}
