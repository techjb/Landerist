using landerist_library.Infrastructure.Tasks;
using landerist_library.Application.Logging;

namespace landerist_unit_tests;

public sealed class SystemRecurringTaskSchedulerTests
{
    [Fact]
    public async Task ScheduleAsync_WhenDisposed_CancelsRunningCallback()
    {
        RecordingLogger logger = new();
        SystemRecurringTaskScheduler scheduler = new(logger, TimeProvider.System);
        TaskCompletionSource entered = new(
            TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource cancelled = new(
            TaskCreationOptions.RunContinuationsAsynchronously);
        IDisposable schedule = scheduler.ScheduleAsync(
            "async-test",
            async cancellationToken =>
            {
                entered.SetResult();
                try
                {
                    await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                }
                finally
                {
                    cancelled.SetResult();
                }
            },
            TimeSpan.Zero,
            TimeSpan.FromHours(1));

        await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
        schedule.Dispose();
        await cancelled.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Contains(logger.Information, entry => entry.Message.Contains("Cancelled"));
    }

    [Fact]
    public async Task Schedule_WhenCallbackThrows_LogsAndSchedulesNextRun()
    {
        RecordingLogger logger = new();
        SystemRecurringTaskScheduler scheduler = new(logger, TimeProvider.System);
        TaskCompletionSource ranTwice = new(
            TaskCreationOptions.RunContinuationsAsynchronously);
        int executions = 0;
        using IDisposable schedule = scheduler.Schedule(
            "sync-failure",
            () =>
            {
                if (Interlocked.Increment(ref executions) == 2)
                {
                    ranTwice.TrySetResult();
                }

                throw new InvalidOperationException("expected failure");
            },
            TimeSpan.Zero,
            TimeSpan.FromMilliseconds(20));

        await ranTwice.Task.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.Contains(logger.Errors, entry =>
            entry.Source == "ScheduledTask sync-failure" &&
            entry.Message.Contains("expected failure"));
    }

    [Fact]
    public async Task ScheduleAsync_WhenCallbackThrows_ObservesAndLogsFailure()
    {
        RecordingLogger logger = new();
        SystemRecurringTaskScheduler scheduler = new(logger, TimeProvider.System);
        using IDisposable schedule = scheduler.ScheduleAsync(
            "async-failure",
            _ => throw new InvalidOperationException("async failure"),
            TimeSpan.Zero,
            TimeSpan.FromHours(1));

        await logger.ErrorWritten.Task.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.Contains(logger.Errors, entry =>
            entry.Source == "ScheduledTask async-failure" &&
            entry.Message.Contains("async failure"));
    }

    private sealed class RecordingLogger : IApplicationLogger
    {
        private readonly object _sync = new();

        public List<(string Source, string Message)> Information { get; } = [];

        public List<(string Source, string Message)> Errors { get; } = [];

        public TaskCompletionSource ErrorWritten { get; } = new(
            TaskCreationOptions.RunContinuationsAsynchronously);

        public void WriteError(string source, string message)
        {
            lock (_sync)
            {
                Errors.Add((source, message));
            }

            ErrorWritten.TrySetResult();
        }

        public void WriteInfo(string source, string message)
        {
            lock (_sync)
            {
                Information.Add((source, message));
            }
        }
    }
}
