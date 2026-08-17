using landerist_library.Application.Statistics;
using landerist_library.Configuration;
using landerist_library.Infrastructure.Sql;
using landerist_library.Logs;

namespace landerist_library.Infrastructure.Distribution;

public sealed class StatisticsPage
{
    private readonly StatisticsChartsBuilder _charts;
    private readonly StatisticsSummaryTableBuilder _summary;
    private readonly StatisticsPageRenderer _renderer;

    public StatisticsPage(
        GlobalStatistics statistics,
        PageStatisticsRepository pageStatistics)
    {
        ArgumentNullException.ThrowIfNull(statistics);
        ArgumentNullException.ThrowIfNull(pageStatistics);

        _charts = new StatisticsChartsBuilder(statistics, pageStatistics);
        _summary = new StatisticsSummaryTableBuilder(statistics);
        _renderer = new StatisticsPageRenderer(
            Path.Combine(
                Config.LANDERIST_COM_TEMPLATES!,
                "statistics",
                "statistics_template.html"),
            Path.Combine(Config.LANDERIST_COM_OUTPUT!, "statistics.html"));
    }

    public void UpdateCharts()
    {
        try
        {
            IReadOnlyList<string> charts = _charts.Build();
            string summary = _summary.Build();
            if (_renderer.Render(summary, charts))
            {
                Log.WriteInfo("StatisticsPage", "Updated statistics page");
            }
        }
        catch (Exception exception)
        {
            Log.WriteError("StatisticsPage AreaChart", exception);
        }
    }
}
