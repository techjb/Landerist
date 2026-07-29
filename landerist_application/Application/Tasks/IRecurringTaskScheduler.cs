namespace landerist_library.Application.Tasks;

public interface IRecurringTaskScheduler
{
    IDisposable Schedule(
        string name,
        Action callback,
        TimeSpan dueTime,
        TimeSpan interval);

    IDisposable ScheduleAsync(
        string name,
        Func<CancellationToken, Task> callback,
        TimeSpan dueTime,
        TimeSpan interval);
}
