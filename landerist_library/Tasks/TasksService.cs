using landerist_library.Database;
using landerist_library.Downloaders.Puppeteer;
using landerist_library.Landerist_com;
using landerist_library.Logs;
using landerist_library.Parse.ListingParser.OpenAI.Batch;
using landerist_library.Scrape;
using landerist_library.Statistics;

namespace landerist_library.Tasks
{
    public class TasksService
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

        public TasksService()
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
            Timer1 = new Timer(Scrape!, null, OneSecond, TenSeconds);
            Timer2 = new Timer(BlockingCollection!, null, OneSecond, OneSecond);

            if (Configuration.Config.IsPrincipalMachine())
            {
                Timer3 = new Timer(TenMinutesTasks!, null, 0, TenMinutes);
                Timer4 = new Timer(HourlyTasks!, null, OneHour, OneHour);
                Timer5 = new Timer(DailyTasks!, null, GetDueTime(), OneDay);
            }
        }

        private static int GetDueTime()
        {
            DateTime now = DateTime.Now;
            DateTime twelveAM = new(now.Year, now.Month, now.Day, 0, 0, 30);
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
                if (!PerformPendingTasks())
                {
                    Scraper.Start();
                }
                
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

        private bool PerformPendingTasks()
        {
            if (PerformTenMinutesTasks)
            {
                TenMinutesTasks();
                return true;
            }
            if (PerformHourlyTasks)
            {
                HourlyTasks();
                return true;
            }
            if (PerformDailyTasks)
            {
                DailyTask();
                return true;
            }
            return false;
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
            TaskBatchDownload.Start();
            TaskBatchUpload.Start();
        }

        public void HourlyTasks()
        {
            PerformHourlyTasks = false;
            Websites.Websites.UpdateRobotsTxt();
            Websites.Websites.UpdateSitemaps();
            Websites.Websites.UpdateIpAddress();
            TaskBatchCleaner.Start();
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
                StatisticsSnapshot.TakeSnapshots();
                FilesUpdater.Update();
                Landerist_com.Landerist_com.UpdatePages();
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
