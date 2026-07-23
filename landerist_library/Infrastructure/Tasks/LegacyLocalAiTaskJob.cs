using landerist_library.Application.Tasks;
using landerist_library.Tasks;

namespace landerist_library.Infrastructure.Tasks;

public sealed class LegacyLocalAiTaskJob : ILocalAiTaskJob
{
    private readonly Func<TaskLocalAIParsing> _factory;
    private TaskLocalAIParsing? _task;

    public LegacyLocalAiTaskJob(Func<TaskLocalAIParsing> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _factory = factory;
    }

    public void Run()
    {
        _task ??= _factory();
        _task.ProcessPages();
    }

    public void Stop() => _task?.Stop();
}
