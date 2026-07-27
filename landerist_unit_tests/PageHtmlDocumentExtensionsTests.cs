using landerist_library.Pages;

namespace landerist_unit_tests;

public sealed class PageHtmlDocumentExtensionsTests
{
    [Fact]
    public void GetHtmlDocument_ReusesUnmodifiedDocument()
    {
        Page page = CreatePage("<html><body>listing</body></html>");

        var first = page.GetHtmlDocument();
        var second = page.GetHtmlDocument();

        Assert.NotNull(first);
        Assert.Same(first, second);
    }

    [Fact]
    public void GetHtmlDocument_ReparsesAfterCachedDocumentIsMutated()
    {
        Page page = CreatePage("<html><body>listing</body></html>");
        var first = page.GetHtmlDocument();
        Assert.NotNull(first);
        first.DocumentNode.SelectSingleNode("//body").InnerHtml = "changed";

        var second = page.GetHtmlDocument();

        Assert.NotNull(second);
        Assert.NotSame(first, second);
        Assert.Equal("listing", second.DocumentNode.SelectSingleNode("//body").InnerText);
    }

    [Fact]
    public void GetHtmlDocument_WithoutResponseBodyReturnsNull()
    {
        Page page = new("https://example.com/listing/1");

        Assert.Null(page.GetHtmlDocument());
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