using landerist_library.Application.Listings;
using landerist_library.Application.Tasks;
using landerist_library.Database;
using landerist_library.Landerist_com;
using landerist_library.Statistics;

namespace landerist_library.Infrastructure.Tasks;

public sealed class LegacyDailyTaskJob : IRecurringTaskJob
{
    private readonly AddressLatLng _addressLatLng;
    private readonly AddressCadastralReference _addressCadastralReference;
    private readonly INotListingCacheMaintenance _notListingCache;
    private readonly IDatabaseBackupService _backup;

    public LegacyDailyTaskJob(
        IDatabase database,
        INotListingCacheMaintenance notListingCache,
        IDatabaseBackupService backup)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(notListingCache);
        ArgumentNullException.ThrowIfNull(backup);
        _addressLatLng = new AddressLatLng(database);
        _addressCadastralReference = new AddressCadastralReference(database);
        _notListingCache = notListingCache;
        _backup = backup;
    }

    public void Run()
    {
        Console.WriteLine("Daily task started ..");
        try
        {
            GlobalStatistics.TakeSnapshots();
            HostStatistics.TakeSnapshots();
            DownloadsUpdater.Update();
            global::landerist_library.Landerist_com.Landerist_com.UpdateAllPages();
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