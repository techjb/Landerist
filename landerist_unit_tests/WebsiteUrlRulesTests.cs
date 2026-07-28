using landerist_library.Websites;

namespace landerist_unit_tests;

public sealed class WebsiteUrlRulesTests
{
    [Fact]
    public void Normalize_DeduplicatesKeyedParametersAndPreservesFlags()
    {
        Uri result = WebsiteUrlRules.Normalize(new Uri(
            "https://example.com/listing?a=1&flag&a=2#details"));

        Assert.Equal("?a=2&flag", result.Query);
        Assert.Equal("#details", result.Fragment);
    }

    [Theory]
    [InlineData("https://example.com/listing", true)]
    [InlineData("https://example.com/listing.html", true)]
    [InlineData("https://example.com/image.jpg", false)]
    public void IsWebPage_UsesPathExtension(string url, bool expected)
    {
        Assert.Equal(expected, WebsiteUrlRules.IsWebPage(new Uri(url)));
    }
}