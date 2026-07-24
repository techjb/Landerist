using landerist_library.Application.Distribution;
using landerist_library.Application.Listings;
using landerist_library.Application.Tasks;
using landerist_library.Database;
using landerist_library.Statistics;

namespace landerist_library.Infrastructure.Tasks;

public sealed class DailyTaskJob : IRecurringTaskJob
{
    private readonly AddressLatLng _addressLatLng;
    private readonly AddressCadastralReference _addressCadastralReference;
    private readonly INotListingCacheMaintenance _notListingCache;
    private readonly IDatabaseBackupService _backup;
    private readonly GlobalStatistics _globalStatistics;
    private readonly HostStatistics _hostStatistics;
    private readonly IDistributionPublisher _distribution;

    public DailyTaskJob(
        IDatabase database,
        INotListingCacheMaintenance notListingCache,
        IDatabaseBackupService backup,
        GlobalStatistics globalStatistics,
        HostStatistics hostStatistics,
        IDistributionPublisher distribution)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(notListingCache);
        ArgumentNullException.ThrowIfNull(backup);
        ArgumentNullException.ThrowIfNull(globalStatistics);
        ArgumentNullException.ThrowIfNull(hostStatistics);
        ArgumentNullException.ThrowIfNull(distribution);
        _addressLatLng = new AddressLatLng(database);
        _addressCadastralReference = new AddressCadastralReference(database);
        _notListingCache = notListingCache;
        _backup = backup;
        _globalStatistics = globalStatistics;
        _hostStatistics = hostStatistics;
        _distribution = distribution;
    }

    public void Run()
    {
        Console.WriteLine("Daily task started ..");
        try
        {
            _globalStatistics.TakeSnapshots();
            _hostStatistics.TakeSnapshots();
            _distribution.Publish();
            _addressLatLng.Clean();
            _addressCadastralReference.Clean();
            _notListingCache.Clean();
            _backup.Update();
        }
        finally
        {
            Console.WriteLine("Daily task finished ..");
        }
    }
}