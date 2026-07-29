using landerist_library.Application.Logging;

namespace landerist_library.Application.Tasks;

public sealed class TasksService : IDisposable
{
    private readonly TasksServiceOptions _options;
    private readonly IRecurringTaskScheduler _scheduler;
    private readonly IApplicationLogger _logger;
    private readonly IScrapeTaskJob _scrapeJob;
    private readonly ILocalAiTaskJob _localAiJob;
    private readonly IRecurringTaskJob _tenMinuteJob;
    private readonly IRecurringTaskJob _hourlyJob;
    private readonly IRecurringTaskJob _dailyJob;
    private readonly TimeProvider _timeProvider;
    private readonly object _lifecycleSync = new();
    private readonly List<IDisposable> _schedules = [];
    private bool _started;
    private bool _disposed;

    public TasksService(
        TasksServiceOptions options,
        IRecurringTaskScheduler scheduler,
        IApplicationLogger logger,
        IScrapeTaskJob scrapeJob,
        ILocalAiTaskJob localAiJob,
        IRecurringTaskJob tenMinuteJob,
        IRecurringTaskJob hourlyJob,
        IRecurringTaskJob dailyJob,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(scheduler);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(scrapeJob);
        ArgumentNullException.ThrowIfNull(localAiJob);
        ArgumentNullException.ThrowIfNull(tenMinuteJob);
        ArgumentNullException.ThrowIfNull(hourlyJob);
        ArgumentNullException.ThrowIfNull(dailyJob);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _options = options;
        _scheduler = scheduler;
        _logger = logger;
        _scrapeJob = scrapeJob;
        _localAiJob = localAiJob;
        _tenMinuteJob = tenMinuteJob;
        _hourlyJob = hourlyJob;
        _dailyJob = dailyJob;
        _timeProvider = timeProvider;
    }

    public void Start()
    {
        lock (_lifecycleSync)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (_started)
            {
                return;
            }

            try
            {
                switch (_options.Mode)
                {
                    case TasksExecutionMode.LocalAi:
                        AddSchedule(
                            "LocalAIParsing",
                            _localAiJob.Run,
                            _options.LocalAiDueTime,
                            _options.LocalAiInterval);
                        break;

                    case TasksExecutionMode.Principal:
                        AddSchedule(
                            "TenMinutesTasks",
                            _tenMinuteJob.Run,
                            TimeSpan.Zero,
                            _options.TenMinuteInterval);
                        AddSchedule(
                            "HourlyTasks",
                            _hourlyJob.Run,
                            _options.HourlyInterval,
                            _options.HourlyInterval);
                        AddSchedule(
                            "DailyTask",
                            _dailyJob.Run,
                            DailyTaskSchedule.GetDueTime(
                                _timeProvider.GetLocalNow().DateTime,
                                _options.DailyStartTime),
                            _options.DailyInterval);
                        break;

                    case TasksExecutionMode.Scraper:
                        _scrapeJob.Prepare();
                        AddSchedule(
                            "UpdateAndScrape",
                            _scrapeJob.Run,
                            _options.ScraperDueTime,
                            _options.ScraperInterval);
                        break;

                    default:
                        throw new InvalidOperationException(
                            $"Unsupported tasks execution mode: {_options.Mode}");
                }

                _started = true;
            }
            catch
            {
                DisposeSchedules();
                throw;
            }
        }
    }

    public void PerformDailyTask(object? state) => DailyTasks(state);

    public void TenMinutesTasks(object? state) =>
        RunSafely("TenMinutesTasks", _tenMinuteJob.Run);

    public void HourlyTasks(object? state) =>
        RunSafely("HourlyTasks", _hourlyJob.Run);

    public void DailyTasks(object? state)
    {
        if (_options.Mode != TasksExecutionMode.Principal)
        {
            return;
        }

        RunSafely("DailyTask", _dailyJob.Run);
    }

    public void Stop()
    {
        lock (_lifecycleSync)
        {
            if (_disposed || !_started)
            {
                return;
            }

            DisposeSchedules();
            _started = false;
        }

        try
        {
            _scrapeJob.Stop();
        }
        finally
        {
            _localAiJob.Stop();
        }
    }

    public void Dispose()
    {
        lock (_lifecycleSync)
        {
            if (_disposed)
            {
                return;
            }
        }

        Stop();

        lock (_lifecycleSync)
        {
            _disposed = true;
        }

        GC.SuppressFinalize(this);
    }

    private void AddSchedule(
        string name,
        Action action,
        TimeSpan dueTime,
        TimeSpan interval)
    {
        int running = 0;
        IDisposable schedule = _scheduler.Schedule(
            name,
            () =>
            {
                if (Interlocked.Exchange(ref running, 1) == 1)
                {
                    return;
                }

                try
                {
                    RunSafely(name, action);
                }
                finally
                {
                    Interlocked.Exchange(ref running, 0);
                }
            },
            dueTime,
            interval);
        _schedules.Add(schedule);
    }

    private void RunSafely(string name, Action action)
    {
        try
        {
            action();
        }
        catch (Exception exception)
        {
            _logger.WriteError(
                "ServiceTasks " + name,
                exception.ToString());
        }
    }

    private void DisposeSchedules()
    {
        foreach (IDisposable schedule in _schedules)
        {
            schedule.Dispose();
        }

        _schedules.Clear();
    }
}
