using landerist_library.Application.Statistics;
using System.Data;
using System.Globalization;
using System.Text.Json;

namespace landerist_library.Infrastructure.Distribution;

internal sealed class StatisticsChartFormatter
{
    private readonly GlobalStatistics _statistics;

    public StatisticsChartFormatter(GlobalStatistics statistics) =>
        _statistics = statistics;

    public string TimeSeries(IEnumerable<StatisticsKey> keys, bool yesterday) =>
        TimeSeries(keys.Select(key => key.ToString()), yesterday);

    public string TimeSeries(IEnumerable<string> keys, bool yesterday)
    {
        List<string> data = [];
        foreach (string key in keys)
        {
            List<string> values = GetTimeSeriesValues(key, yesterday);
            data.Add(
                $"{{\"label\": {JsonSerializer.Serialize(key)}, \"values\":[{string.Join(",", values)}]}}");
        }

        return string.Join(",", data);
    }

    public static string LabeledValues(
        string label,
        Dictionary<string, object?> values) =>
        $"{{\"label\": {JsonSerializer.Serialize(label)}, \"values\":[{DictionaryValues(values)}]}}";

    public static string DictionaryValues(Dictionary<string, object?> dictionary)
    {
        List<string> data = [];
        foreach (var pair in dictionary)
        {
            string value = pair.Value switch
            {
                null => "null",
                IFormattable formattable =>
                    formattable.ToString(null, CultureInfo.InvariantCulture) ?? "null",
                _ => JsonSerializer.Serialize(pair.Value)
            };
            data.Add(
                $"{{\"key\": {JsonSerializer.Serialize(pair.Key)}, \"value\": {value}}}");
        }

        return string.Join(",", data);
    }

    public static string Chart(string chartType, string title, string data)
    {
        string safeTitle = title.Replace("\\", "\\\\").Replace("'", "\\'");
        return $"{chartType}('{safeTitle}', [{data}])";
    }

    private List<string> GetTimeSeriesValues(string statisticKey, bool yesterday)
    {
        DataTable table = _statistics.GetLatestStatistics(statisticKey, 15);
        List<string> values = [];
        foreach (DataRow row in table.Rows.Cast<DataRow>().Reverse())
        {
            int counter = Convert.ToInt32(row["Counter"]);
            DateTime date = (DateTime)row["Date"];
            if (yesterday)
            {
                date = date.AddDays(-1);
            }

            string dateText = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            values.Add(
                $"{{\"key\": {JsonSerializer.Serialize(dateText)}, \"value\": {counter.ToString(CultureInfo.InvariantCulture)}}}");
        }

        return values;
    }
}
