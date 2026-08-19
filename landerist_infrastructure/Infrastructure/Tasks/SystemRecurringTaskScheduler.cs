using landerist_library.Application.Tasks;
using landerist_library.Application.Logging;

namespace landerist_library.Infrastructure.Tasks;

public sealed class SystemRecurringTaskScheduler : IRecurringTaskScheduler
{
    private readonly IApplicationLogger _logger;
    private readonly TimeProvider _timeProvider;

    public SystemRecurringTaskScheduler(
        IApplicationLogger logger,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(timeProvider);
        _logger = logger;
        _timeProvider = timeProvider;
    }

    public IDisposable Schedule(
        string name,
        Action callback,
        TimeSpan dueTime,
        TimeSpan interval)
    {
        ValidateArguments(name, callback, dueTime, interval);
        ScheduledOperation operation = new(
            name, callback, interval, _logger, _timeProvider);
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
        AsyncScheduledOperation operation = new(
            name, callback, interval, _logger, _timeProvider);
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
        private readonly string _name;
        private readonly Action _callback;
        private readonly TimeSpan _interval;
        private readonly object _sync = new();
        private readonly Timer _timer;
        private readonly IApplicationLogger _logger;
        private readonly TimeProvider _timeProvider;
        private bool _disposed;

        public ScheduledOperation(
            string name,
            Action callback,
            TimeSpan interval,
            IApplicationLogger logger,
            TimeProvider timeProvider)
        {
            _name = name;
            _callback = callback;
            _interval = interval;
            _logger = logger;
            _timeProvider = timeProvider;
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
            long startedAt = _timeProvider.GetTimestamp();
            SafeInfo(_logger, _name, "Started");
            try
            {
                _callback();
                SafeInfo(
                    _logger,
                    _name,
                    $"Completed in {_timeProvider.GetElapsedTime(startedAt).TotalMilliseconds:F0} ms");
            }
            catch (Exception exception)
            {
                SafeError(
                    _logger,
                    _name,
                    $"Failed after {_timeProvider.GetElapsedTime(startedAt).TotalMilliseconds:F0} ms: {exception}");
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
        private readonly string _name;
        private readonly Func<CancellationToken, Task> _callback;
        private readonly TimeSpan _interval;
        private readonly object _sync = new();
        private readonly CancellationTokenSource _cancellation = new();
        private readonly CancellationToken _cancellationToken;
        private readonly Timer _timer;
        private readonly IApplicationLogger _logger;
        private readonly TimeProvider _timeProvider;
        private bool _disposed;

        public AsyncScheduledOperation(
            string name,
            Func<CancellationToken, Task> callback,
            TimeSpan interval,
            IApplicationLogger logger,
            TimeProvider timeProvider)
        {
            _name = name;
            _callback = callback;
            _interval = interval;
            _logger = logger;
            _timeProvider = timeProvider;
            _cancellationToken = _cancellation.Token;
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
            long startedAt = _timeProvider.GetTimestamp();
            SafeInfo(_logger, _name, "Started");
            try
            {
                await _callback(_cancellationToken).ConfigureAwait(false);
                SafeInfo(
                    _logger,
                    _name,
                    $"Completed in {_timeProvider.GetElapsedTime(startedAt).TotalMilliseconds:F0} ms");
            }
            catch (OperationCanceledException) when (_cancellationToken.IsCancellationRequested)
            {
                SafeInfo(
                    _logger,
                    _name,
                    $"Cancelled after {_timeProvider.GetElapsedTime(startedAt).TotalMilliseconds:F0} ms");
            }
            catch (Exception exception)
            {
                SafeError(
                    _logger,
                    _name,
                    $"Failed after {_timeProvider.GetElapsedTime(startedAt).TotalMilliseconds:F0} ms: {exception}");
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

    private static void SafeInfo(
        IApplicationLogger logger,
        string name,
        string message)
    {
        try
        {
            logger.WriteInfo($"ScheduledTask {name}", message);
        }
        catch
        {
            // Observability must not affect scheduling reliability.
        }
    }

    private static void SafeError(
        IApplicationLogger logger,
        string name,
        string message)
    {
        try
        {
            logger.WriteError($"ScheduledTask {name}", message);
        }
        catch
        {
            // Observability must not affect scheduling reliability.
        }
    }
}
