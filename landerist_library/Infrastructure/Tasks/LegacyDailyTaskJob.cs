using landerist_library.Application.Tasks;
using landerist_library.Database;
using landerist_library.Landerist_com;
using landerist_library.Statistics;

namespace landerist_library.Infrastructure.Tasks;

public sealed class LegacyDailyTaskJob : IRecurringTaskJob
{
    public void Run()
    {
        Console.WriteLine("Daily task started ..");
        try
        {
            GlobalStatistics.TakeSnapshots();
            HostStatistics.TakeSnapshots();
            DownloadsUpdater.Update();
            global::landerist_library.Landerist_com.Landerist_com.UpdateAllPages();
            AddressLatLng.Clean();
            AddressCadastralReference.Clean();
            NotListingsCache.Clean();
            Backup.Update();
        }
        finally
        {
            Console.WriteLine("Daily task finished ..");
        }
    }
}
