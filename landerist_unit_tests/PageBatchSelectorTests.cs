using landerist_library.Application.Scraping;
using landerist_library.Pages;
using landerist_library.Websites;

namespace landerist_unit_tests;

public sealed class PageBatchSelectorTests
{
    [Fact]
    public void Select_CleansLocksAndRequestsConfiguredMaximum()
    {
        RecordingPageSelectionRepository repository = new(
            CreatePage("one.example.com", "/1"),
            CreatePage("two.example.com", "/2"));
        PageBatchSelector selector = new(
            repository,
            new PageSelectionOptions(10, 2, 0, false));

        var result = selector.Select();

        Assert.Equal(1, repository.CleanLockedPagesCalls);
        Assert.Equal(10, repository.RequestedMaximumCount);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Select_EnforcesGlobalAndPerHostLimits()
    {
        Page first = CreatePage("one.example.com", "/1");
        Page sameHost = CreatePage("one.example.com", "/2");
        Page secondHost = CreatePage("two.example.com", "/3");
        Page thirdHost = CreatePage("three.example.com", "/4");
        RecordingPageSelectionRepository repository = new(
            first,
            sameHost,
            secondHost,
            thirdHost);
        PageBatchSelector selector = new(
            repository,
            new PageSelectionOptions(2, 1, 0, false));

        var result = selector.Select();

        Assert.Equal(2, result.Count);
        Assert.Same(first, result[0]);
        Assert.Same(secondHost, result[1]);
    }

    [Fact]
    public void Select_RemovesDuplicatesWithinRepositoryBatch()
    {
        Page first = CreatePage("one.example.com", "/1");
        Page duplicate = CreatePage("one.example.com", "/1");
        Page another = CreatePage("two.example.com", "/2");
        RecordingPageSelectionRepository repository = new(
            first,
            duplicate,
            another);
        PageBatchSelector selector = new(
            repository,
            new PageSelectionOptions(10, 2, 0, false));

        var result = selector.Select();

        Assert.Equal(2, result.Count);
        Assert.Same(first, result[0]);
        Assert.Same(another, result[1]);
    }

    [Fact]
    public void Select_WhenProductionMinimumIsNotReached_ReturnsEmptyBatch()
    {
        RecordingPageSelectionRepository repository = new(
            CreatePage("one.example.com", "/1"),
            CreatePage("two.example.com", "/2"));
        PageBatchSelector selector = new(
            repository,
            new PageSelectionOptions(10, 2, 3, true));

        var result = selector.Select();

        Assert.Empty(result);
    }

    [Fact]
    public void Select_WhenMinimumIsNotEnforced_ReturnsSmallBatch()
    {
        Page page = CreatePage("one.example.com", "/1");
        RecordingPageSelectionRepository repository = new(page);
        PageBatchSelector selector = new(
            repository,
            new PageSelectionOptions(10, 2, 3, false));

        var result = selector.Select();

        Assert.Single(result);
        Assert.Same(page, result[0]);
    }

    private static Page CreatePage(string host, string path)
    {
        Website website = new(new Uri($"https://{host}"));
        return new Page(website, new Uri(website.MainUri, path));
    }

    private sealed class RecordingPageSelectionRepository(params Page[] pages)
        : IPageSelectionRepository
    {
        public int CleanLockedPagesCalls { get; private set; }

        public int RequestedMaximumCount { get; private set; }

        public void CleanLockedPages() => CleanLockedPagesCalls++;

        public IReadOnlyList<Page> GetScrapePages(int maximumCount)
        {
            RequestedMaximumCount = maximumCount;
            return pages;
        }
    }
}
