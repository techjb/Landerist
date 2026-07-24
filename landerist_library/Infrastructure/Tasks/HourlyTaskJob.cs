using landerist_library.Application.Tasks;

namespace landerist_library.Infrastructure.Tasks;

public sealed class HourlyTaskJob : IRecurringTaskJob
{
    private readonly TaskBatchCleaner _batchCleaner;

    public HourlyTaskJob(TaskBatchCleaner batchCleaner)
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
