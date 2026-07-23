using landerist_library.Application.Tasks;
using landerist_library.Tasks;

namespace landerist_library.Infrastructure.Tasks;

public sealed class LegacyHourlyTaskJob : IRecurringTaskJob
{
    public void Run()
    {
        global::landerist_library.Websites.Websites.UpdateRobotsTxt();
        global::landerist_library.Websites.Websites.UpdateSitemaps();
        global::landerist_library.Websites.Websites.UpdateIpAddress();
        TaskBatchCleaner.Start();
    }
}
