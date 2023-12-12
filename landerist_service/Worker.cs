using landerist_library.Logs;
using landerist_library.Export;

namespace landerist_service
{
    public class Worker(ILogger<Worker> logger) : BackgroundService
    {
        private readonly ILogger<Worker> Logger = logger;

        private Timer? Timer1;
        private const int OneSeccond = 1000;
        private const int OneMinute = 60 * OneSeccond;
        private const int OneHour = 60 * OneMinute;
        private const int OneDay = 24 * OneHour;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Logger.LogInformation("ExecuteAsync");
            while (!stoppingToken.IsCancellationRequested)
            {
                Start();
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
        }

        private void Start()
        {
            Log.WriteLogInfo("service", "Service started");
            SetTimers();
            // todo: start scrapper
        }

        private void SetTimers()
        {
            DateTime nowTime = DateTime.Now;

            DateTime oneAM = new(nowTime.Year, nowTime.Month, nowTime.Day, 1, 0, 0);
            if (nowTime > oneAM)
            {
                oneAM = oneAM.AddDays(1);
            }

            int dueTimeOneAM = (int)(oneAM - nowTime).TotalMilliseconds;

            Timer1 = new Timer(TimerCallback1!, null, dueTimeOneAM, OneDay);
        }
        private void TimerCallback1(object state)
        {
            try
            {
                FilesUpdater.Start();
            }
            catch (Exception exception)
            {
                Log.WriteLogErrors(exception);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("StopAsync");
            Log.WriteLogInfo("service", "Service stopped");
            Timer1?.Change(Timeout.Infinite, 0);
            await base.StopAsync(cancellationToken);
        }
    }
}
