using landerist_library.Infrastructure.Tasks;

namespace landerist_unit_tests;

public sealed class SystemRecurringTaskSchedulerTests
{
    [Fact]
    public async Task ScheduleAsync_WhenDisposed_CancelsRunningCallback()
    {
        SystemRecurringTaskScheduler scheduler = new();
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
    }
}