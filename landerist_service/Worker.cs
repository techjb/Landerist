using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Downloaders.Puppeteer;
using landerist_library.Landerist_com;
using landerist_library.Logs;
using landerist_library.Scrape;
using landerist_library.Statistics;
using landerist_library.Websites;

namespace landerist_service
{
    public class Worker(ILogger<Worker> logger) : BackgroundService
    {
        private readonly ILogger<Worker> Logger = logger;

        private Timer? Timer1;
        private Timer? Timer2;
        private bool RunningTimer2 = false;

        private const int OneSecond = 1000;
        private const int TenSeconds = 10 * OneSecond;
        private const int OneMinute = 60 * OneSecond;
        private const int OneHour = 60 * OneMinute;
        private const int OneDay = 24 * OneHour;
        private readonly Scraper Scraper = new();

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Logger.LogInformation("ExecuteAsync");
            while (!stoppingToken.IsCancellationRequested)
            {
                Log.WriteLogInfo("service", "Started. Version: " + Config.VERSION);
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

            Timer1 = new Timer(TimerCallback1!, null, dueTimeOneAM, OneDay);
            Timer2 = new Timer(TimerCallback2!, null, 0, TenSeconds);
        }

        private void TimerCallback1(object state)
        {
            try
            {
                Pages.DeleteUnpublishedListings();
                StatisticsSnapshot.TakeSnapshots();
                DownloadFilesUpdater.UpdateFiles();
                Landerist_com.UpdateDownloadsAndStatisticsPages();
                Backup.Update();
            }
            catch (Exception exception)
            {
                Log.WriteLogErrors("Worker TimerCallback1", exception);
            }
        }

        private void TimerCallback2(object state)
        {
            if (RunningTimer2)
            {
                return;
            }

            RunningTimer2 = true;
            try
            {
                Websites.UpdateRobotsTxt();
                Websites.UpdateSitemaps();
                Websites.UpdateIpAddress();
                Scraper.Start();
            }
            catch (Exception exception)
            {
                Log.WriteLogErrors("Worker TimerCallback2", exception);
            }
            finally
            {
                RunningTimer2 = false;
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("StopAsync");
            Scraper.Stop();
            Log.WriteLogInfo("service", "Stopped. Version: " + Config.VERSION);
            Timer1?.Change(Timeout.Infinite, 0);
            Timer2?.Change(Timeout.Infinite, 0);
            await base.StopAsync(cancellationToken);
        }
    }
}
