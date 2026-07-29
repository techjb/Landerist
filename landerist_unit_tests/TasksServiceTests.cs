using landerist_library.Application.Logging;
using landerist_library.Application.Tasks;

namespace landerist_unit_tests;

public sealed class TasksServiceTests
{
    [Fact]
    public void Start_InLocalAiMode_SchedulesOnlyLocalAiJob()
    {
        TestContext context = CreateContext(TasksExecutionMode.LocalAi);

        context.Service.Start();

        RecordingSchedule schedule = Assert.Single(context.Scheduler.Schedules);
        Assert.Equal("LocalAIParsing", schedule.Name);
        Assert.Equal(TimeSpan.FromSeconds(1), schedule.DueTime);
        Assert.Equal(TimeSpan.FromSeconds(2), schedule.Interval);
        Assert.Equal(0, context.Scrape.PrepareCalls);
    }

    [Fact]
    public void Start_InPrincipalMode_SchedulesAllOperationalWorkflows()
    {
        TestContext context = CreateContext(TasksExecutionMode.Principal);

        context.Service.Start();

        Assert.Equal(
            ["TenMinutesTasks", "HourlyTasks", "DailyTask"],
            context.Scheduler.Schedules.Select(schedule => schedule.Name));
        Assert.Equal(TimeSpan.Zero, context.Scheduler.Schedules[0].DueTime);
        Assert.Equal(TimeSpan.FromHours(1), context.Scheduler.Schedules[1].DueTime);
        Assert.Equal(TimeSpan.FromSeconds(30), context.Scheduler.Schedules[2].DueTime);
    }

    [Fact]
    public void Start_InScraperMode_PreparesResourcesAndSchedulesScraping()
    {
        TestContext context = CreateContext(TasksExecutionMode.Scraper);

        context.Service.Start();

        Assert.Equal(1, context.Scrape.PrepareCalls);
        RecordingSchedule schedule = Assert.Single(context.Scheduler.Schedules);
        Assert.Equal("UpdateAndScrape", schedule.Name);
        schedule.Callback();
        Assert.Equal(1, context.Scrape.RunCalls);
    }

    [Fact]
    public void Start_WhenCalledTwice_DoesNotDuplicateSchedules()
    {
        TestContext context = CreateContext(TasksExecutionMode.Principal);

        context.Service.Start();
        context.Service.Start();

        Assert.Equal(3, context.Scheduler.Schedules.Count);
    }

    [Fact]
    public void Stop_DisposesSchedulesAndStopsLongRunningJobs()
    {
        TestContext context = CreateContext(TasksExecutionMode.Principal);
        context.Service.Start();

        context.Service.Stop();

        Assert.All(context.Scheduler.Schedules, schedule => Assert.True(schedule.Disposed));
        Assert.Equal(1, context.Scrape.StopCalls);
        Assert.Equal(1, context.LocalAi.StopCalls);
    }

    [Fact]
    public void ScheduledJob_WhenItThrows_IsLoggedWithoutEscapingCallback()
    {
        TestContext context = CreateContext(TasksExecutionMode.Principal);
        context.TenMinute.OnRun = () => throw new InvalidOperationException("failure");
        context.Service.Start();

        context.Scheduler.Schedules[0].Callback();

        var error = Assert.Single(context.Logger.Errors);
        Assert.Equal("ServiceTasks TenMinutesTasks", error.Source);
        Assert.Contains("failure", error.Message);
    }

    [Fact]
    public async Task ScheduledJob_WhenAlreadyRunning_SkipsOverlappingInvocation()
    {
        TestContext context = CreateContext(TasksExecutionMode.Principal);
        using ManualResetEventSlim entered = new();
        using ManualResetEventSlim release = new();
        context.TenMinute.OnRun = () =>
        {
            entered.Set();
            release.Wait();
        };
        context.Service.Start();
        RecordingSchedule schedule = context.Scheduler.Schedules[0];
        Task firstRun = Task.Run(schedule.Callback);
        try
        {
            Assert.True(entered.Wait(TimeSpan.FromSeconds(5)));
            schedule.Callback();
        }
        finally
        {
            release.Set();
            await firstRun;
        }

        Assert.Equal(1, context.TenMinute.RunCalls);
    }

    [Fact]
    public void PerformDailyTask_OutsidePrincipalMode_DoesNothing()
    {
        TestContext context = CreateContext(TasksExecutionMode.Scraper);

        context.Service.PerformDailyTask(null);

        Assert.Equal(0, context.Daily.RunCalls);
    }

    [Fact]
    public async Task StopAsync_DisposesSchedulesAndStopsLongRunningJobs()
    {
        TestContext context = CreateContext(TasksExecutionMode.Scraper);
        context.Service.Start();

        await context.Service.StopAsync(CancellationToken.None);

        Assert.All(context.Scheduler.Schedules, schedule => Assert.True(schedule.Disposed));
        Assert.Equal(1, context.Scrape.StopAsyncCalls);
        Assert.Equal(1, context.LocalAi.StopCalls);
    }

    [Fact]
    public async Task StopAsync_WhenCancelled_StillStopsLocalAiJob()
    {
        TestContext context = CreateContext(TasksExecutionMode.Scraper);
        context.Service.Start();
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            context.Service.StopAsync(cancellation.Token));

        Assert.Equal(0, context.Scrape.StopAsyncCalls);
        Assert.Equal(1, context.LocalAi.StopCalls);
    }
    [Fact]
    public void Dispose_BeforeStart_DoesNotStopJobs()
    {
        TestContext context = CreateContext(TasksExecutionMode.Scraper);

        context.Service.Dispose();

        Assert.Equal(0, context.Scrape.StopCalls);
        Assert.Equal(0, context.LocalAi.StopCalls);
    }
    [Fact]
    public void Start_AfterDispose_ThrowsObjectDisposedException()
    {
        TestContext context = CreateContext(TasksExecutionMode.Scraper);
        context.Service.Dispose();

        Assert.Throws<ObjectDisposedException>(() => context.Service.Start());
    }

    [Fact]
    public void DailySchedule_WhenStartTimeHasPassed_UsesNextDay()
    {
        DateTime now = new(2026, 7, 23, 0, 0, 31);

        TimeSpan dueTime = DailyTaskSchedule.GetDueTime(
            now,
            new TimeOnly(0, 0, 30));

        Assert.Equal(TimeSpan.FromDays(1) - TimeSpan.FromSeconds(1), dueTime);
    }

    private static TestContext CreateContext(TasksExecutionMode mode)
    {
        RecordingScheduler scheduler = new();
        RecordingApplicationLogger logger = new();
        RecordingScrapeTaskJob scrape = new();
        RecordingLocalAiTaskJob localAi = new();
        RecordingRecurringTaskJob tenMinute = new();
        RecordingRecurringTaskJob hourly = new();
        RecordingRecurringTaskJob daily = new();
        TasksService service = new(
            new TasksServiceOptions(mode),
            scheduler,
            logger,
            scrape,
            localAi,
            tenMinute,
            hourly,
            daily,
            new FixedTimeProvider(
                new DateTimeOffset(2026, 7, 23, 0, 0, 0, TimeSpan.Zero)));

        return new TestContext(
            service,
            scheduler,
            logger,
            scrape,
            localAi,
            tenMinute,
            hourly,
            daily);
    }

    private sealed record TestContext(
        TasksService Service,
        RecordingScheduler Scheduler,
        RecordingApplicationLogger Logger,
        RecordingScrapeTaskJob Scrape,
        RecordingLocalAiTaskJob LocalAi,
        RecordingRecurringTaskJob TenMinute,
        RecordingRecurringTaskJob Hourly,
        RecordingRecurringTaskJob Daily);

    private sealed class RecordingScheduler : IRecurringTaskScheduler
    {
        public List<RecordingSchedule> Schedules { get; } = [];

        public IDisposable Schedule(
            string name,
            Action callback,
            TimeSpan dueTime,
            TimeSpan interval)
        {
            RecordingSchedule schedule = new(name, callback, dueTime, interval);
            Schedules.Add(schedule);
            return schedule;
        }
    }

    private sealed class RecordingSchedule(
        string name,
        Action callback,
        TimeSpan dueTime,
        TimeSpan interval) : IDisposable
    {
        public string Name { get; } = name;

        public Action Callback { get; } = callback;

        public TimeSpan DueTime { get; } = dueTime;

        public TimeSpan Interval { get; } = interval;

        public bool Disposed { get; private set; }

        public void Dispose() => Disposed = true;
    }

    private sealed class RecordingScrapeTaskJob : IScrapeTaskJob
    {
        public int PrepareCalls { get; private set; }

        public int RunCalls { get; private set; }

        public int StopCalls { get; private set; }

        public int StopAsyncCalls { get; private set; }

        public void Prepare() => PrepareCalls++;

        public void Run() => RunCalls++;

        public void Stop() => StopCalls++;

        public Task StopAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            StopAsyncCalls++;
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingLocalAiTaskJob : ILocalAiTaskJob
    {
        public int RunCalls { get; private set; }

        public int StopCalls { get; private set; }

        public void Run() => RunCalls++;

        public void Stop() => StopCalls++;


    }

    private sealed class RecordingRecurringTaskJob : IRecurringTaskJob
    {
        public Action? OnRun { get; set; }

        public int RunCalls { get; private set; }

        public void Run()
        {
            RunCalls++;
            OnRun?.Invoke();
        }
    }

    private sealed class RecordingApplicationLogger : IApplicationLogger
    {
        public List<(string Source, string Message)> Errors { get; } = [];

        public void WriteError(string source, string message) =>
            Errors.Add((source, message));

        public void WriteInfo(string source, string message)
        {
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;

        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
    }
}
