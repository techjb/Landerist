using landerist_library.Application.Distribution;
using landerist_library.Application.Listings;
using landerist_library.Application.Logging;
using landerist_library.Application.Statistics;
using landerist_library.Application.Tasks;

namespace landerist_library.Infrastructure.Tasks;

public sealed class DailyTaskJob : IRecurringTaskJob
{
    private readonly IAddressDataMaintenance _addresses;
    private readonly INotListingCacheMaintenance _notListingCache;
    private readonly IDatabaseBackupService _backup;
    private readonly GlobalStatistics _globalStatistics;
    private readonly HostStatistics _hostStatistics;
    private readonly IDistributionPublisher _distribution;
    private readonly IApplicationLogger _logger;

    public DailyTaskJob(
        IAddressDataMaintenance addresses,
        INotListingCacheMaintenance notListingCache,
        IDatabaseBackupService backup,
        GlobalStatistics globalStatistics,
        HostStatistics hostStatistics,
        IDistributionPublisher distribution,
        IApplicationLogger logger)
    {
        ArgumentNullException.ThrowIfNull(addresses);
        ArgumentNullException.ThrowIfNull(notListingCache);
        ArgumentNullException.ThrowIfNull(backup);
        ArgumentNullException.ThrowIfNull(globalStatistics);
        ArgumentNullException.ThrowIfNull(hostStatistics);
        ArgumentNullException.ThrowIfNull(distribution);
        ArgumentNullException.ThrowIfNull(logger);
        _addresses = addresses;
        _notListingCache = notListingCache;
        _backup = backup;
        _globalStatistics = globalStatistics;
        _hostStatistics = hostStatistics;
        _distribution = distribution;
        _logger = logger;
    }

    public void Run()
    {
        _logger.WriteInfo(nameof(DailyTaskJob), "Started");
        try
        {
            _globalStatistics.TakeSnapshots();
            _hostStatistics.TakeSnapshots();
            _distribution.Publish();
            _addresses.Clean();
            _notListingCache.Clean();
            _backup.Update();
        }
        finally
        {
            _logger.WriteInfo(nameof(DailyTaskJob), "Finished");
        }
    }
}
