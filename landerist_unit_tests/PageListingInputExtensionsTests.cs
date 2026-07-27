using landerist_library.Pages;
using landerist_library.Websites;

namespace landerist_unit_tests;

public sealed class PageListingInputExtensionsTests
{
    [Fact]
    public void ApplyListingParserInput_TracksRepeatedInput()
    {
        Page page = new(
            "https://example.com/listing/1",
            new PageRules(
                MaxPageTypeCounter: 2,
                MinListingParserInputLength: 1,
                MaxListingParserInputLength: 100,
                MaxScreenshotSize: 100));

        page.ApplyListingParserInput("same input");
        Assert.False(page.ListingParserInputNotChanged);
        Assert.Equal((short)0, page.ListingParserInputNotChangedCounter);

        page.ApplyListingParserInput("same input");
        page.ApplyListingParserInput("same input");
        page.ApplyListingParserInput("same input");

        Assert.True(page.ListingParserInputNotChanged);
        Assert.Equal((short)2, page.ListingParserInputNotChangedCounter);
    }

    [Fact]
    public void SetListingParserInput_ExtractsInputFromResponseBody()
    {
        Page page = CreatePage(
            "<html><body><h1>Flat for sale</h1><p>Madrid</p></body></html>");

        page.SetListingParserInput();

        Assert.False(string.IsNullOrWhiteSpace(page.ListingParserInput));
        Assert.NotNull(page.ListingParserInputHash);
    }

    [Fact]
    public void SetListingParserInput_WithoutHtmlResetsComparisonState()
    {
        Page page = new("https://example.com/listing/1")
        {
            ListingParserInputNotChanged = true
        };

        page.SetListingParserInput();

        Assert.False(page.ListingParserInputNotChanged);
        Assert.Null(page.ListingParserInputNotChangedCounter);
    }

    [Fact]
    public void UnavailableRule_CanMatchVisibleParserInputText()
    {
        Website website = new(new Uri("https://example.com"))
        {
            ListingUnavailableRegex = "^unavailable$"
        };
        Page page = new(website)
        {
            ListingParserInput = "<div>unavailable</div>"
        };

        Assert.True(page.MatchesWebsiteListingUnavailableRule());
    }

    private static Page CreatePage(string content)
    {
        Page page = new("https://example.com/listing/1");
        page.SetDownloadedData(new PageDownloadResult(
            Content: content,
            Screenshot: null,
            HttpStatusCode: 200,
            RedirectUrl: null,
            Etag: null,
            LastModified: null));
        return page;
    }
}