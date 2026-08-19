using landerist_library.Application.Tasks;

namespace landerist_library.Infrastructure.Tasks;

public sealed class InMemoryTaskHealthRegistry : ITaskHealthRegistry
{
    private readonly object _sync = new();
    private readonly Dictionary<string, State> _states = new(StringComparer.Ordinal);

    public void Register(string name, DateTimeOffset firstRun, TimeSpan interval)
    {
        lock (_sync)
        {
            _states[name] = new State(firstRun, interval);
        }
    }

    public void Started(string name, DateTimeOffset at) => Update(name, state =>
    {
        state.Running = true;
        state.LastStartedAt = at;
    });

    public void Succeeded(string name, DateTimeOffset at, TimeSpan duration) => Update(name, state =>
    {
        state.Running = false;
        state.LastSucceededAt = at;
        state.LastCompletedAt = at;
        state.LastDuration = duration;
        state.ConsecutiveFailures = 0;
        state.LastError = null;
    });

    public void Failed(string name, DateTimeOffset at, TimeSpan duration, string error) => Update(name, state =>
    {
        state.Running = false;
        state.LastFailedAt = at;
        state.LastCompletedAt = at;
        state.LastDuration = duration;
        state.ConsecutiveFailures++;
        state.LastError = error;
    });

    public void Cancelled(string name, DateTimeOffset at, TimeSpan duration) => Update(name, state =>
    {
        state.Running = false;
        state.LastCompletedAt = at;
        state.LastDuration = duration;
    });

    public IReadOnlyList<TaskHealthSnapshot> Snapshot(DateTimeOffset now)
    {
        lock (_sync)
        {
            return _states
                .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                .Select(pair => CreateSnapshot(pair.Key, pair.Value, now))
                .ToArray();
        }
    }

    private void Update(string name, Action<State> update)
    {
        lock (_sync)
        {
            if (_states.TryGetValue(name, out State? state))
            {
                update(state);
            }
        }
    }

    private static TaskHealthSnapshot CreateSnapshot(string name, State state, DateTimeOffset now)
    {
        TimeSpan staleAfter = TimeSpan.FromTicks(Math.Max(
            TimeSpan.FromMinutes(5).Ticks,
            checked(state.Interval.Ticks * 2)));
        bool stuck = state.Running && state.LastStartedAt is not null &&
            now - state.LastStartedAt.Value > staleAfter;
        DateTimeOffset expectedFrom = state.LastCompletedAt ?? state.FirstRun;
        bool overdue = !state.Running && now - expectedFrom > staleAfter;
        string status = stuck || overdue || state.ConsecutiveFailures > 0
            ? "degraded"
            : "healthy";
        return new TaskHealthSnapshot(
            name,
            status,
            state.LastStartedAt,
            state.LastSucceededAt,
            state.LastFailedAt,
            state.LastDuration?.TotalMilliseconds,
            state.ConsecutiveFailures,
            stuck ? "Execution appears to be stuck." : overdue ? "Execution is overdue." : state.LastError);
    }

    private sealed class State(DateTimeOffset firstRun, TimeSpan interval)
    {
        public DateTimeOffset FirstRun { get; } = firstRun;
        public TimeSpan Interval { get; } = interval;
        public bool Running { get; set; }
        public DateTimeOffset? LastStartedAt { get; set; }
        public DateTimeOffset? LastSucceededAt { get; set; }
        public DateTimeOffset? LastFailedAt { get; set; }
        public DateTimeOffset? LastCompletedAt { get; set; }
        public TimeSpan? LastDuration { get; set; }
        public int ConsecutiveFailures { get; set; }
        public string? LastError { get; set; }
    }
}
