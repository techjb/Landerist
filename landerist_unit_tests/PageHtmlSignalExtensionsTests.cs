using landerist_library.Pages;
using landerist_library.Websites;

namespace landerist_unit_tests;

public sealed class PageHtmlSignalExtensionsTests
{
    [Theory]
    [InlineData("noindex", true, false, false)]
    [InlineData("nofollow", false, true, false)]
    [InlineData("noimageindex", false, false, true)]
    [InlineData("none", true, true, true)]
    public void MetaRobotsSignals_AreExtracted(
        string content,
        bool noIndex,
        bool noFollow,
        bool noImageIndex)
    {
        Page page = CreatePage(
            "https://example.com/listing/1",
            $"<html><head><meta name='robots' content='{content}'></head></html>");

        Assert.Equal(noIndex, page.ContainsMetaRobotsNoIndex());
        Assert.Equal(noFollow, page.ContainsMetaRobotsNoFollow());
        Assert.Equal(noImageIndex, page.ContainsMetaRobotsNoImageIndex());
    }

    [Fact]
    public void CanonicalSignal_ResolvesRelativeUriAndRemovesFragment()
    {
        Page page = CreatePage(
            "https://example.com/listing/1",
            "<html><head><link rel='canonical' href='../canonical#photos'></head></html>");

        Uri? canonical = page.GetCanonicalUri();

        Assert.Equal("https://example.com/canonical", canonical?.AbsoluteUri);
        Assert.True(page.NotCanonical());
    }

    [Theory]
    [InlineData("es-ES", false)]
    [InlineData("en", true)]
    [InlineData(null, false)]
    public void LanguageSignal_UsesWebsiteLanguage(
        string? language,
        bool expectedIncorrect)
    {
        string attribute = language is null ? string.Empty : $" lang='{language}'";
        Page page = CreatePage(
            "https://example.com/listing/1",
            $"<html{attribute}><body></body></html>");

        Assert.Equal(expectedIncorrect, page.IncorrectLanguage());
    }

    private static Page CreatePage(string url, string content)
    {
        Website website = new(new Uri("https://example.com"));
        Page page = new(website, new Uri(url));
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