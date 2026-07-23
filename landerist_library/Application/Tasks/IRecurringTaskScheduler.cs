namespace landerist_library.Application.Tasks;

public interface IRecurringTaskScheduler
{
    IDisposable Schedule(
        string name,
        Action callback,
        TimeSpan dueTime,
        TimeSpan interval);
}
