using landerist_library.Application.Statistics;
using System.Globalization;

namespace landerist_library.Infrastructure.Distribution;

internal sealed class StatisticsSummaryTableBuilder
{
    private static readonly CultureInfo SummaryCulture =
        CultureInfo.GetCultureInfo("es-ES");

    private readonly GlobalStatistics _statistics;

    public StatisticsSummaryTableBuilder(GlobalStatistics statistics) =>
        _statistics = statistics;

    public string Build()
    {
        List<(string Label, StatisticsKey Key)> rows =
        [
            ("Websites", StatisticsKey.Websites),
            ("Pages", StatisticsKey.Pages),
            ("Unknown Page Type", StatisticsKey.UnknownPageType),
            ("Need Update", StatisticsKey.NeedUpdate),
            ("Waiting AI Request", StatisticsKey.WaitingAIRequest),
            ("Listings", StatisticsKey.Listings),
            ("Published Listings", StatisticsKey.PublishedListings),
            ("Unpublished Listings", StatisticsKey.UnpublishedListings),
        ];

        string tableRows = string.Join(
            Environment.NewLine,
            rows.Select(row =>
                "                        <tr>" + Environment.NewLine +
                $"                            <td>{row.Label}</td>" + Environment.NewLine +
                $"                            <td>{GetLatestCounter(row.Key).ToString("N0", SummaryCulture)}</td>" + Environment.NewLine +
                "                        </tr>"));

        return
            "                <table>" + Environment.NewLine +
            "                    <thead>" + Environment.NewLine +
            "                        <tr>" + Environment.NewLine +
            "                            <th>Data Type</th>" + Environment.NewLine +
            "                            <th>Value</th>" + Environment.NewLine +
            "                        </tr>" + Environment.NewLine +
            "                    </thead>" + Environment.NewLine +
            "                    <tbody>" + Environment.NewLine +
            tableRows + Environment.NewLine +
            "                    </tbody>" + Environment.NewLine +
            "                </table>";
    }

    private int GetLatestCounter(StatisticsKey key)
    {
        var table = _statistics.GetLatestStatistics(key.ToString(), 1);
        return table.Rows.Count == 0
            ? 0
            : Convert.ToInt32(table.Rows[0]["Counter"]);
    }
}
