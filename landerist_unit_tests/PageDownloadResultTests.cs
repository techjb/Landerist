using landerist_library.Pages;

namespace landerist_unit_tests;

public sealed class PageDownloadResultTests
{
    [Fact]
    public void SetDownloadedData_AppliesResultAndNormalizesHeaders()
    {
        Page page = new("https://example.com/listing/1")
        {
            Etag = " previous-etag "
        };
        byte[] screenshot = [1, 2, 3];

        page.SetDownloadedData(new PageDownloadResult(
            Content: "<html>listing</html>",
            Screenshot: screenshot,
            HttpStatusCode: 200,
            RedirectUrl: "https://example.com/listing/2",
            Etag: " previous-etag ",
            LastModified: " updated "));

        Assert.Equal("previous-etag", page.Etag);
        Assert.Equal("updated", page.LastModified);
        Assert.Equal((short)200, page.HttpStatusCode);
        Assert.Equal("https://example.com/listing/2", page.RedirectUrl);
        Assert.Same(screenshot, page.Screenshot);
        Assert.True(page.DownloadedHeadersHaveNotChanged());
        Assert.False(page.ResponseBodyIsNullOrEmpty());
    }

    [Fact]
    public void SetDownloadedData_UsesLastModifiedWhenEtagCannotBeCompared()
    {
        Page page = new("https://example.com/listing/1")
        {
            LastModified = "same"
        };

        page.SetDownloadedData(new PageDownloadResult(
            Content: null,
            Screenshot: null,
            HttpStatusCode: null,
            RedirectUrl: null,
            Etag: null,
            LastModified: "same"));

        Assert.True(page.DownloadedHeadersHaveNotChanged());
    }
}