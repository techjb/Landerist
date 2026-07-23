using landerist_library.Application.Scraping;
using landerist_library.Pages;
using landerist_library.Websites;

namespace landerist_unit_tests;

public sealed class PageBatchOrdererTests
{
    [Fact]
    public void SpreadByHost_InterleavesPagesWhilePreservingHostOrder()
    {
        Page a1 = CreatePage("a.test", 1);
        Page a2 = CreatePage("a.test", 2);
        Page b1 = CreatePage("b.test", 1);
        Page b2 = CreatePage("b.test", 2);
        Page c1 = CreatePage("c.test", 1);

        List<Page> result = PageBatchOrderer.SpreadByHost(
            [a1, a2, b1, b2, c1]);

        Assert.Equal([a1, b1, c1, a2, b2], result);
    }

    [Fact]
    public void SpreadByHost_WhenEmpty_ReturnsEmptyList()
    {
        List<Page> result = PageBatchOrderer.SpreadByHost([]);

        Assert.Empty(result);
    }

    [Fact]
    public void SpreadByHost_RejectsNullInput()
    {
        Assert.Throws<ArgumentNullException>(
            () => PageBatchOrderer.SpreadByHost(null!));
    }

    private static Page CreatePage(string host, int id)
    {
        Uri mainUri = new($"https://{host}");
        return new Page(
            new Website(mainUri),
            new Uri(mainUri, $"/listing/{id}"));
    }
}
