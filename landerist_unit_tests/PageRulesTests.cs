using landerist_library.Pages;

namespace landerist_unit_tests;

public sealed class PageRulesTests
{
    private static readonly PageRules CustomRules = new(
        MaxPageTypeCounter: 2,
        MinListingParserInputLength: 3,
        MaxListingParserInputLength: 5,
        MaxScreenshotSize: 4);

    [Fact]
    public void PageTypeCounters_RespectCustomMaximum()
    {
        Page page = new("https://example.com/listing/1", CustomRules);

        page.SetPageType(PageType.Listing);
        page.SetPageType(PageType.Listing);
        page.SetPageType(PageType.Listing);
        page.SetPageType(PageType.HttpStatusCodeNull);
        page.SetPageType(PageType.HttpStatusCodeNull);
        page.SetPageType(PageType.HttpStatusCodeNull);

        Assert.Equal((short)2, page.PageTypeCounter);
        Assert.Equal((short)2, page.TransientErrorCounter);
    }

    [Theory]
    [InlineData(null, true, false)]
    [InlineData("ab", true, false)]
    [InlineData("abc", false, false)]
    [InlineData("abcde", false, false)]
    [InlineData("abcdef", false, true)]
    public void ListingParserInputLength_RespectsCustomBounds(
        string? parserInput,
        bool expectedTooShort,
        bool expectedTooLarge)
    {
        Page page = new("https://example.com/listing/1", CustomRules)
        {
            ListingParserInput = parserInput
        };

        Assert.Equal(expectedTooShort, page.ListingParserInputIsTooShort());
        Assert.Equal(expectedTooLarge, page.ListingParserInputIsTooLarge());
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(3, true)]
    [InlineData(4, false)]
    [InlineData(5, false)]
    public void ScreenshotSize_RespectsCustomMaximum(int size, bool expected)
    {
        Page page = new("https://example.com/listing/1", CustomRules)
        {
            Screenshot = new byte[size]
        };

        Assert.Equal(expected, page.ContainsScreenshot());
    }
}
