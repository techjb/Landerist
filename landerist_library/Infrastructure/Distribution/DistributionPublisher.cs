using landerist_library.Application.Distribution;
using landerist_library.Application.Websites;
using landerist_library.Infrastructure.Sql;
using landerist_library.Infrastructure.WebsiteServices;
using landerist_library.Application.Statistics;

namespace landerist_library.Infrastructure.Distribution;

public sealed class DistributionPublisher : IDistributionPublisher
{
    private readonly GlobalStatistics _globalStatistics;
    private readonly HostStatistics _hostStatistics;
    private readonly PageStatisticsRepository _pageStatistics;
    private readonly WebsiteMetricsService _websiteMetrics;
    private readonly IWebsiteCatalog _websites;
    private readonly WebsiteQueryRepository _websiteQueries;

    public DistributionPublisher(
        GlobalStatistics globalStatistics,
        HostStatistics hostStatistics,
        PageStatisticsRepository pageStatistics,
        WebsiteMetricsService websiteMetrics,
        IWebsiteCatalog websites,
        WebsiteQueryRepository websiteQueries)
    {
        ArgumentNullException.ThrowIfNull(globalStatistics);
        ArgumentNullException.ThrowIfNull(hostStatistics);
        ArgumentNullException.ThrowIfNull(pageStatistics);
        ArgumentNullException.ThrowIfNull(websiteMetrics);
        ArgumentNullException.ThrowIfNull(websites);
        ArgumentNullException.ThrowIfNull(websiteQueries);
        _globalStatistics = globalStatistics;
        _hostStatistics = hostStatistics;
        _pageStatistics = pageStatistics;
        _websiteMetrics = websiteMetrics;
        _websites = websites;
        _websiteQueries = websiteQueries;
    }

    public void Publish()
    {
        DownloadsUpdater.Update(_websites, _websiteQueries);
        DistributionArtifacts.UpdateAllPages(
            _globalStatistics,
            _hostStatistics,
            _pageStatistics,
            _websiteMetrics,
            _websites);
    }
}