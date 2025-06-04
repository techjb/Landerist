using landerist_library.Database;
using landerist_library.Downloaders.Puppeteer;
using landerist_library.Landerist_com;
using landerist_library.Logs;
using landerist_library.Parse.ListingParser.OpenAI.Batch;
using landerist_library.Scrape;
using landerist_library.Statistics;
using landerist_library.Websites;

namespace landerist_library.Tasks
{
    public class ServiceTasks
    {
        private readonly Scraper Scraper;
        private Timer? Timer1;
        private Timer? Timer2;
        private Timer? Timer3;
        private Timer? Timer4;
        private Timer? Timer5;

        private bool RunningTimer2 = false;
        private bool RunningTimer3 = false;

        private const int OneSecond = 1000;
        private const int TenSeconds = 10 * OneSecond;
        private const int OneMinute = 60 * OneSecond;
        private const int TenMinutes = 10 * OneMinute;
        private const int OneHour = 60 * OneMinute;
        private const int OneDay = 24 * OneHour;

        private bool PerformDailyTasks = false;

        private bool PerformTenMinutesTasks = false;

        private bool PerformHourlyTasks = false;

        public ServiceTasks()
        {
            Scraper = new();
        }

        public void Start()
        {
            PuppeteerDownloader.UpdateChrome();
            SetTimers();
        }

        private void SetTimers()
        {
            Timer1 = new Timer(Scrape!, null, 0, TenSeconds);
            Timer2 = new Timer(BlockingCollection!, null, OneSecond, OneSecond);

            if (Configuration.Config.IsPrincipalMachine())
            {
                Timer3 = new Timer(TenMinutesTasks!, null, TenMinutes, TenMinutes);
                Timer4 = new Timer(HourlyTasks!, null, OneHour, OneHour);
                Timer5 = new Timer(DailyTasks!, null, GetDueTime(), OneDay);
            }
        }

        private static int GetDueTime()
        {
            DateTime now = DateTime.Now;
            DateTime twelveAM = new(now.Year, now.Month, now.Day, 0, 0, 10);
            if (now > twelveAM)
            {
                twelveAM = twelveAM.AddDays(1);
            }
            return (int)(twelveAM - now).TotalMilliseconds;
        }

        public void PerformDailyTask()
        {
            Console.WriteLine("Daily task ..");
            PerformDailyTasks = true;
        }
        private void DailyTasks(object state)
        {
            PerformDailyTasks = true;
        }

        private void TenMinutesTasks(object state)
        {
            PerformTenMinutesTasks = true;
        }

        private void HourlyTasks(object state)
        {
            PerformHourlyTasks = true;
        }

        private void Scrape(object state)
        {
            if (RunningTimer2)
            {
                return;
            }

            RunningTimer2 = true;
            try
            {
                PerformOtherTasks();
                Scraper.Start();
            }
            catch (Exception exception)
            {
                Log.WriteError("ServiceTasks UpdateAndScrape", exception);
            }
            finally
            {
                RunningTimer2 = false;
            }
        }

        private void PerformOtherTasks()
        {
            if (PerformTenMinutesTasks)
            {
                TenMinutesTasks();
            }
            if (PerformHourlyTasks)
            {
                HourlyTasks();
            }
            if (PerformDailyTasks)
            {
                DailyTask();
            }
        }

        private void BlockingCollection(object state)
        {
            if (RunningTimer3)
            {
                return;
            }

            RunningTimer3 = true;
            try
            {
                Scraper.FinalizeBlockingCollection();
            }
            catch (Exception exception)
            {
                Log.WriteError("ServiceTasks BlockingCollection", exception);
            }
            finally
            {
                RunningTimer3 = false;
            }
        }

        public void TenMinutesTasks()
        {
            PerformTenMinutesTasks = false;
            BatchDownload.Start();
            BatchUpload.Start();
        }

        public void HourlyTasks()
        {
            PerformHourlyTasks = false;
            Websites.Websites.UpdateRobotsTxt();
            Websites.Websites.UpdateSitemaps();
            Websites.Websites.UpdateIpAddress();
            BatchCleaner.Start();
        }

        public void DailyTask()
        {
            PerformDailyTasks = false;

            if (!Configuration.Config.IsPrincipalMachine())
            {
                return;
            }

            try
            {
                Pages.DeleteUnpublishedListings();
                StatisticsSnapshot.TakeSnapshots();
                DownloadFilesUpdater.UpdateListingsAndUpdates();
                Landerist_com.Landerist_com.UpdateDownloadsAndStatisticsPages();
                Backup.Update();
            }
            catch (Exception exception)
            {
                Log.WriteError("ServiceTasks DailyTask", exception);
            }
        }

        public void Stop()
        {
            Console.WriteLine("Stopping ServiceTasks ..");
            Scraper.Stop();
            Timer1?.Change(Timeout.Infinite, 0);
            Timer2?.Change(Timeout.Infinite, 0);
            Timer3?.Change(Timeout.Infinite, 0);
            Timer4?.Change(Timeout.Infinite, 0);
            Timer5?.Change(Timeout.Infinite, 0);
        }
    }
}
