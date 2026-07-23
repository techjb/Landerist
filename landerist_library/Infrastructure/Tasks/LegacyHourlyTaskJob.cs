using landerist_library.Application.Tasks;
using landerist_library.Tasks;

namespace landerist_library.Infrastructure.Tasks;

public sealed class LegacyHourlyTaskJob : IRecurringTaskJob
{
    private readonly TaskBatchCleaner _batchCleaner;

    public LegacyHourlyTaskJob(TaskBatchCleaner batchCleaner)
    {
        ArgumentNullException.ThrowIfNull(batchCleaner);
        _batchCleaner = batchCleaner;
    }

    public void Run()
    {
        global::landerist_library.Websites.Websites.UpdateRobotsTxt();
        global::landerist_library.Websites.Websites.UpdateSitemaps();
        global::landerist_library.Websites.Websites.UpdateIpAddress();
        _batchCleaner.Start();
    }
}
