using landerist_library.Infrastructure.Distribution;

namespace landerist_unit_tests;

public sealed class StatisticsChartFormatterTests
{
    [Fact]
    public void Chart_EscapesTitleForJavascriptLiteral()
    {
        string chart = StatisticsChartFormatter.Chart(
            "BarChart",
            "Host's \\ status",
            "{}");

        Assert.Equal("BarChart('Host\\'s \\\\ status', [{}])", chart);
    }

    [Fact]
    public void DictionaryValues_UsesInvariantNumbersAndJsonStrings()
    {
        Dictionary<string, object?> values = new()
        {
            ["decimal"] = 12.5m,
            ["text"] = "hello",
            ["empty"] = null
        };

        string result = StatisticsChartFormatter.DictionaryValues(values);

        Assert.Equal(
            "{\"key\": \"decimal\", \"value\": 12.5}," +
            "{\"key\": \"text\", \"value\": \"hello\"}," +
            "{\"key\": \"empty\", \"value\": null}",
            result);
    }
}
