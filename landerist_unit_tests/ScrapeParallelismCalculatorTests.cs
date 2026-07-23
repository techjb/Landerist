using landerist_library.Application.Scraping;
using landerist_library.Pages;
using landerist_library.Websites;

namespace landerist_unit_tests;

public sealed class ScrapeParallelismCalculatorTests
{
    [Fact]
    public void Calculate_WhenExecutionIsLocal_ReturnsOne()
    {
        ScrapeParallelismCalculator calculator = CreateCalculator(
            isLocal: true,
            maximum: 8);

        int result = calculator.Calculate(CreatePages("a.test", "b.test"));

        Assert.Equal(1, result);
    }

    [Fact]
    public void Calculate_WhenHostsAreDistinct_RespectsConfiguredMaximum()
    {
        ScrapeParallelismCalculator calculator = CreateCalculator(
            isLocal: false,
            maximum: 2);

        int result = calculator.Calculate(CreatePages("a.test", "b.test", "c.test"));

        Assert.Equal(2, result);
    }

    [Fact]
    public void Calculate_WhenMaximumIsAutomatic_UsesPageCountForDistinctHosts()
    {
        ScrapeParallelismCalculator calculator = CreateCalculator(
            isLocal: false,
            maximum: 0);

        int result = calculator.Calculate(CreatePages("a.test", "b.test", "c.test"));

        Assert.Equal(3, result);
    }

    [Fact]
    public void Calculate_WhenHostsRepeat_AppliesHostThrottleLimit()
    {
        ScrapeParallelismCalculator calculator = CreateCalculator(
            isLocal: false,
            maximum: 8);

        int result = calculator.Calculate(
            CreatePages("a.test", "a.test", "b.test", "b.test"));

        Assert.Equal(1, result);
    }

    private static ScrapeParallelismCalculator CreateCalculator(
        bool isLocal,
        int maximum) =>
        new(
            new ScraperExecutionOptions(
                isProduction: true,
                isLocal,
                maximum,
                estimatedMinimumScrapeSeconds: 2,
                minimumHostThrottleSeconds: 3));

    private static List<Page> CreatePages(params string[] hosts) =>
        hosts
            .Select(
                (host, index) =>
                {
                    Uri mainUri = new($"https://{host}");
                    return new Page(
                        new Website(mainUri),
                        new Uri(mainUri, $"/listing/{index}"));
                })
            .ToList();
}
