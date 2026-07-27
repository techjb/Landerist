using landerist_library.Websites;

namespace landerist_unit_tests;

public sealed class WebsiteHttpRequestProfileTests
{
    [Fact]
    public void From_CreatesImmutableNormalizedProfile()
    {
        Website website = new("example.com")
        {
            UserAgent = "  Landerist-Test/1.0  ",
            HttpRequestHeaders = """
                {
                    "X-Site": " example ",
                    "User-Agent": "must-not-override"
                }
                """
        };

        WebsiteHttpRequestProfile profile =
            WebsiteHttpRequestProfile.From(website);
        website.UserAgent = "changed";
        website.HttpRequestHeaders = null;

        Assert.Equal("Landerist-Test/1.0", profile.UserAgent);
        Assert.Equal("example", profile.Headers["X-Site"]);

        using HttpRequestMessage request =
            profile.CreateRequest(HttpMethod.Get, new Uri("https://example.com/a"));
        Assert.Equal("Landerist-Test/1.0", request.Headers.UserAgent.ToString());
        Assert.Equal("example", Assert.Single(request.Headers.GetValues("X-Site")));
    }

    [Fact]
    public void From_ParsesLineBasedHeadersAndIgnoresInvalidLines()
    {
        Website website = new("example.com")
        {
            HttpRequestHeaders = """
                X-One: first
                invalid
                X-Two: second:value
                """
        };

        WebsiteHttpRequestProfile profile =
            WebsiteHttpRequestProfile.From(website);

        Assert.Equal("first", profile.Headers["X-One"]);
        Assert.Equal("second:value", profile.Headers["X-Two"]);
        Assert.Equal(2, profile.Headers.Count);
    }
}
