using landerist_library.Application.Tasks;
using landerist_library.Application.Websites;

namespace landerist_library.Infrastructure.Tasks;

public sealed class HourlyTaskJob : IRecurringTaskJob
{
    private readonly IWebsiteRefreshService _websites;
    private readonly TaskBatchCleaner _batchCleaner;

    public HourlyTaskJob(
        IWebsiteRefreshService websites,
        TaskBatchCleaner batchCleaner)
    {
        ArgumentNullException.ThrowIfNull(websites);
        ArgumentNullException.ThrowIfNull(batchCleaner);
        _websites = websites;
        _batchCleaner = batchCleaner;
    }

    public void Run()
    {
        _websites.Refresh();
        _batchCleaner.Start();
    }
}