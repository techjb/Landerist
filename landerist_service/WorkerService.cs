using landerist_library.Configuration;
using landerist_library.Downloaders.Puppeteer;
using landerist_library.Logs;
using landerist_library.Tasks;


namespace landerist_service
{
    public class WorkerService(ILogger<WorkerService> logger) : BackgroundService
    {
        private readonly ILogger<WorkerService> Logger = logger;

        private Timer? Timer1;
        private Timer? Timer2;
        private bool RunningTimer2 = false;

        private const int OneSecond = 1000;
        private const int TenSeconds = 10 * OneSecond;
        private const int OneMinute = 60 * OneSecond;
        private const int OneHour = 60 * OneMinute;
        private const int OneDay = 24 * OneHour;

        private readonly ServiceTasks ServiceTasks = new();

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Logger.LogInformation("ExecuteAsync");
            while (!stoppingToken.IsCancellationRequested)
            {
                Log.WriteInfo("landerist_service", "Started. Version: " + Config.VERSION);
                PuppeteerDownloader.UpdateChrome();
                SetTimers();
                //Scraper.DoTest();

                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
        }

        private void SetTimers()
        {
            DateTime nowTime = DateTime.Now;

            DateTime twelveAM = new(nowTime.Year, nowTime.Month, nowTime.Day, 0, 0, 0);
            if (nowTime > twelveAM)
            {
                twelveAM = twelveAM.AddDays(1);
            }

            int dueTimeOneAM = (int)(twelveAM - nowTime).TotalMilliseconds;

            Timer1 = new Timer(DailyTasks!, null, dueTimeOneAM, OneDay);
            Timer2 = new Timer(UpdateAndScrape!, null, 0, TenSeconds);            
        }

        private void DailyTasks(object state)
        {
            try
            {
                ServiceTasks.DailyTask();                
            }
            catch (Exception exception)
            {
                Log.WriteError("WorkerService DailyTasks", exception);
            }
        }

        private void UpdateAndScrape(object state)
        {
            if (RunningTimer2)
            {
                return;
            }

            RunningTimer2 = true;
            try
            {
                ServiceTasks.UpdateAndScrape();
            }
            catch (Exception exception)
            {
                Log.WriteError("WorkerService UpdateAndScrape", exception);
            }
            finally
            {
                RunningTimer2 = false;
            }
        }       

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("StopAsync");
            ServiceTasks.Stop();
            Log.WriteInfo("landerist_service", "Stopped. Version: " + Config.VERSION);
            Timer1?.Change(Timeout.Infinite, 0);
            Timer2?.Change(Timeout.Infinite, 0);            
            await base.StopAsync(cancellationToken);
        }
    }
}
