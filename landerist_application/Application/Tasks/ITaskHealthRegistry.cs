namespace landerist_library.Application.Tasks;

public interface ITaskHealthRegistry
{
    void Register(
        string name,
        DateTimeOffset firstRun,
        TimeSpan interval,
        TimeSpan? maxProgressSilence = null);
    void Started(string name, DateTimeOffset at);
    void Progress(string name, DateTimeOffset at);
    void Succeeded(string name, DateTimeOffset at, TimeSpan duration);
    void Failed(string name, DateTimeOffset at, TimeSpan duration, string error);
    void Cancelled(string name, DateTimeOffset at, TimeSpan duration);
    IReadOnlyList<TaskHealthSnapshot> Snapshot(DateTimeOffset now);
}

public sealed record TaskHealthSnapshot(
    string Name,
    string Status,
    DateTimeOffset? LastStartedAt,
    DateTimeOffset? LastProgressAt,
    DateTimeOffset? LastSucceededAt,
    DateTimeOffset? LastFailedAt,
    double? LastDurationMilliseconds,
    int ConsecutiveFailures,
    string? LastError);
