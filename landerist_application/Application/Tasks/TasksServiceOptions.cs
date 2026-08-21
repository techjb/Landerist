namespace landerist_library.Application.Tasks;

public enum TasksExecutionMode
{
    LocalAi,
    Principal,
    Scraper
}

public sealed class TasksServiceOptions
{
    public TasksServiceOptions(
        TasksExecutionMode mode,
        TimeSpan? localAiDueTime = null,
        TimeSpan? localAiInterval = null,
        TimeSpan? scraperDueTime = null,
        TimeSpan? scraperInterval = null,
        TimeSpan? tenMinuteInterval = null,
        TimeSpan? hourlyInterval = null,
        TimeSpan? dailyInterval = null,
        TimeOnly? dailyStartTime = null,
        TimeSpan? localAiMaxProgressSilence = null,
        TimeSpan? scraperMaxProgressSilence = null)
    {
        Mode = mode;
        LocalAiDueTime = ValidateNonNegative(
            localAiDueTime ?? TimeSpan.FromSeconds(1),
            nameof(localAiDueTime));
        LocalAiInterval = ValidatePositive(
            localAiInterval ?? TimeSpan.FromSeconds(2),
            nameof(localAiInterval));
        ScraperDueTime = ValidateNonNegative(
            scraperDueTime ?? TimeSpan.FromSeconds(1),
            nameof(scraperDueTime));
        ScraperInterval = ValidatePositive(
            scraperInterval ?? TimeSpan.FromSeconds(3),
            nameof(scraperInterval));
        TenMinuteInterval = ValidatePositive(
            tenMinuteInterval ?? TimeSpan.FromMinutes(10),
            nameof(tenMinuteInterval));
        HourlyInterval = ValidatePositive(
            hourlyInterval ?? TimeSpan.FromHours(1),
            nameof(hourlyInterval));
        DailyInterval = ValidatePositive(
            dailyInterval ?? TimeSpan.FromDays(1),
            nameof(dailyInterval));
        DailyStartTime = dailyStartTime ?? new TimeOnly(0, 0, 30);
        LocalAiMaxProgressSilence = ValidatePositive(
            localAiMaxProgressSilence ?? TimeSpan.FromMinutes(15),
            nameof(localAiMaxProgressSilence));
        ScraperMaxProgressSilence = ValidatePositive(
            scraperMaxProgressSilence ?? TimeSpan.FromMinutes(10),
            nameof(scraperMaxProgressSilence));
    }

    public TasksExecutionMode Mode { get; }

    public TimeSpan LocalAiDueTime { get; }

    public TimeSpan LocalAiInterval { get; }

    public TimeSpan ScraperDueTime { get; }

    public TimeSpan ScraperInterval { get; }

    public TimeSpan TenMinuteInterval { get; }

    public TimeSpan HourlyInterval { get; }

    public TimeSpan DailyInterval { get; }

    public TimeOnly DailyStartTime { get; }

    public TimeSpan LocalAiMaxProgressSilence { get; }

    public TimeSpan ScraperMaxProgressSilence { get; }

    private static TimeSpan ValidateNonNegative(TimeSpan value, string parameterName)
    {
        if (value < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(parameterName);
        }

        return value;
    }

    private static TimeSpan ValidatePositive(TimeSpan value, string parameterName)
    {
        if (value <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(parameterName);
        }

        return value;
    }
}
