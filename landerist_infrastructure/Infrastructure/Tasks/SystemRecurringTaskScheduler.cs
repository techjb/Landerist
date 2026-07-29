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
        ValidateArguments(name, callback, dueTime, interval);
        ScheduledOperation operation = new(callback, interval);
        operation.Start(dueTime);
        return operation;
    }

    public IDisposable ScheduleAsync(
        string name,
        Func<CancellationToken, Task> callback,
        TimeSpan dueTime,
        TimeSpan interval)
    {
        ValidateArguments(name, callback, dueTime, interval);
        AsyncScheduledOperation operation = new(callback, interval);
        operation.Start(dueTime);
        return operation;
    }

    private static void ValidateArguments(
        string name,
        Delegate callback,
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
    private sealed class AsyncScheduledOperation : IDisposable
    {
        private readonly Func<CancellationToken, Task> _callback;
        private readonly TimeSpan _interval;
        private readonly object _sync = new();
        private readonly CancellationTokenSource _cancellation = new();
        private readonly Timer _timer;
        private bool _disposed;

        public AsyncScheduledOperation(
            Func<CancellationToken, Task> callback,
            TimeSpan interval)
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

        private void Execute(object? state) => _ = ExecuteAsync();

        private async Task ExecuteAsync()
        {
            try
            {
                await _callback(_cancellation.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (_cancellation.IsCancellationRequested)
            {
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
                _cancellation.Cancel();
                _timer.Dispose();
                _cancellation.Dispose();
            }
        }
    }
}
