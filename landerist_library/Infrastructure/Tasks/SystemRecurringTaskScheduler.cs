using landerist_library.Application.Tasks;

namespace landerist_library.Infrastructure.Tasks;

public sealed class SystemRecurringTaskScheduler : IRecurringTaskScheduler
{
    public IDisposable Schedule(
        string name,
        Action callback,
        TimeSpan dueTime,
        TimeSpan interval)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(callback);
        if (dueTime < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(dueTime));
        }
        if (interval <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(interval));
        }

        ScheduledOperation operation = new(callback, interval);
        operation.Start(dueTime);
        return operation;
    }

    private sealed class ScheduledOperation : IDisposable
    {
        private readonly Action _callback;
        private readonly TimeSpan _interval;
        private readonly object _sync = new();
        private readonly Timer _timer;
        private bool _disposed;

        public ScheduledOperation(Action callback, TimeSpan interval)
        {
            _callback = callback;
            _interval = interval;
            _timer = new Timer(Execute, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }

        public void Start(TimeSpan dueTime)
        {
            lock (_sync)
            {
                ObjectDisposedException.ThrowIf(_disposed, this);
                _timer.Change(dueTime, Timeout.InfiniteTimeSpan);
            }
        }

        private void Execute(object? state)
        {
            try
            {
                _callback();
            }
            finally
            {
                lock (_sync)
                {
                    if (!_disposed)
                    {
                        _timer.Change(_interval, Timeout.InfiniteTimeSpan);
                    }
                }
            }
        }

        public void Dispose()
        {
            lock (_sync)
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;
                _timer.Dispose();
            }
        }
    }
}
