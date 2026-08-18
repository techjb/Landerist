using landerist_library.Application.Distribution;
using landerist_library.Application.Listings;
using landerist_library.Application.Logging;
using landerist_library.Application.Websites;
using landerist_library.Application.Statistics;

namespace landerist_library.Infrastructure.Distribution;

public sealed class DistributionPublisher : IDistributionPublisher
{
    private readonly GlobalStatistics _globalStatistics;
    private readonly HostStatistics _hostStatistics;
    private readonly IPageStatisticsRepository _pageStatistics;
    private readonly IDistributionWebsiteMetrics _websiteMetrics;
    private readonly IWebsiteCatalog _websites;
    private readonly IWebsiteExportSource _websiteQueries;
    private readonly IListingAdministrationService _listings;
    private readonly DistributionOptions _options;
    private readonly IApplicationLogger _logger;

    public DistributionPublisher(
        GlobalStatistics globalStatistics,
        HostStatistics hostStatistics,
        IPageStatisticsRepository pageStatistics,
        IDistributionWebsiteMetrics websiteMetrics,
        IWebsiteCatalog websites,
        IWebsiteExportSource websiteQueries,
        IListingAdministrationService listings,
        DistributionOptions options,
        IApplicationLogger logger)
    {
        ArgumentNullException.ThrowIfNull(globalStatistics);
        ArgumentNullException.ThrowIfNull(hostStatistics);
        ArgumentNullException.ThrowIfNull(pageStatistics);
        ArgumentNullException.ThrowIfNull(websiteMetrics);
        ArgumentNullException.ThrowIfNull(websites);
        ArgumentNullException.ThrowIfNull(websiteQueries);
        ArgumentNullException.ThrowIfNull(listings);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);
        _globalStatistics = globalStatistics;
        _hostStatistics = hostStatistics;
        _pageStatistics = pageStatistics;
        _websiteMetrics = websiteMetrics;
        _websites = websites;
        _websiteQueries = websiteQueries;
        _listings = listings;
        _options = options;
        _logger = logger;
    }

    public void Publish()
    {
        new DownloadsUpdater(_listings, _options, _logger)
            .Update(_websites, _websiteQueries);
        DistributionArtifacts.UpdateAllPages(
            _globalStatistics,
            _hostStatistics,
            _pageStatistics,
            _websiteMetrics,
            _websites,
            _options);
    }
}
