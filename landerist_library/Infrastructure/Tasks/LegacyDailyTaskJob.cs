using landerist_library.Application.Listings;
using landerist_library.Application.Tasks;
using landerist_library.Database;
using landerist_library.Landerist_com;
using landerist_library.Statistics;
using landerist_library.Infrastructure.Sql;
using landerist_library.Infrastructure.WebsiteServices;

namespace landerist_library.Infrastructure.Tasks;

public sealed class LegacyDailyTaskJob : IRecurringTaskJob
{
    private readonly AddressLatLng _addressLatLng;
    private readonly AddressCadastralReference _addressCadastralReference;
    private readonly INotListingCacheMaintenance _notListingCache;
    private readonly IDatabaseBackupService _backup;
    private readonly GlobalStatistics _globalStatistics;
    private readonly HostStatistics _hostStatistics;
    private readonly PageStatisticsRepository _pageStatistics;
    private readonly WebsiteMetricsService _websiteMetrics;

    public LegacyDailyTaskJob(
        IDatabase database,
        INotListingCacheMaintenance notListingCache,
        IDatabaseBackupService backup,
        GlobalStatistics globalStatistics,
        HostStatistics hostStatistics,
        PageStatisticsRepository pageStatistics,
        WebsiteMetricsService websiteMetrics)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(notListingCache);
        ArgumentNullException.ThrowIfNull(backup);
        ArgumentNullException.ThrowIfNull(globalStatistics);
        ArgumentNullException.ThrowIfNull(hostStatistics);
        ArgumentNullException.ThrowIfNull(pageStatistics);
        ArgumentNullException.ThrowIfNull(websiteMetrics);
        _addressLatLng = new AddressLatLng(database);
        _addressCadastralReference = new AddressCadastralReference(database);
        _notListingCache = notListingCache;
        _backup = backup;
        _globalStatistics = globalStatistics;
        _hostStatistics = hostStatistics;
        _pageStatistics = pageStatistics;
        _websiteMetrics = websiteMetrics;
    }

    public void Run()
    {
        Console.WriteLine("Daily task started ..");
        try
        {
            _globalStatistics.TakeSnapshots();
            _hostStatistics.TakeSnapshots();
            DownloadsUpdater.Update();
            global::landerist_library.Landerist_com.Landerist_com.UpdateAllPages(_globalStatistics, _hostStatistics, _pageStatistics, _websiteMetrics);
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