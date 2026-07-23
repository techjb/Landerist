namespace landerist_library.Application.Tasks;

public static class DailyTaskSchedule
{
    public static TimeSpan GetDueTime(DateTime now, TimeOnly startTime)
    {
        DateTime nextRun = now.Date.Add(startTime.ToTimeSpan());
        if (nextRun <= now)
        {
            nextRun = nextRun.AddDays(1);
        }

        return nextRun - now;
    }
}
