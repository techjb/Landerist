using landerist_library.Database;
using landerist_library.Downloaders.Puppeteer;
using landerist_library.Landerist_com;
using landerist_library.Logs;
using landerist_library.Parse.ListingParser.OpenAI.Batch;
using landerist_library.Scrape;
using landerist_library.Statistics;

namespace landerist_library.Tasks
{
    public sealed class TasksService : IDisposable
    {
        private readonly Scraper Scraper;
        private readonly TaskLocalAIParsing TaskLocalAIParsing;
        private readonly TaskBatchDownload TaskBatchDownload = new();
        private readonly TaskBatchUpload TaskBatchUpload = new();

        private Timer? Timer1;
        private Timer? Timer2;
        private Timer? Timer3;
        private Timer? Timer4;
        private Timer? Timer5;

        private int RunningTimer1;
        private int RunningTimer2;

        private const int OneSecond = 1000;
        private const int TwoSeconds = 2000;
        private const int TenSeconds = 10 * OneSecond;
        private const int OneMinute = 60 * OneSecond;
        private const int TenMinutes = 10 * OneMinute;
        private const int OneHour = 60 * OneMinute;
        private const int OneDay = 24 * OneHour;

        private volatile bool PerformDailyTasks;
        private volatile bool PerformTenMinutesTasks;
        private volatile bool PerformHourlyTasks;

        private bool Disposed;

        public TasksService()
        {
            Scraper = new();
            TaskLocalAIParsing = new();
        }

        public void Start()
        {
            ObjectDisposedException.ThrowIf(Disposed, this);

            if (Timer1 is not null
                || Timer2 is not null
                || Timer3 is not null
                || Timer4 is not null
                || Timer5 is not null)
            {
                return;
            }

            if (Configuration.Config.IsLocalAIMachine()
                || Configuration.Config.IsConfigurationLocal())
            {
                Timer1 = new Timer(LocalAIParsing, null, OneSecond, TwoSeconds);
                return;
            }

            PuppeteerDownloader.UpdateChrome();

            Timer1 = new Timer(Scrape, null, OneSecond, TenSeconds);
            Timer2 = new Timer(BlockingCollection, null, OneSecond, OneSecond);

            if (Configuration.Config.IsPrincipalMachine())
            {
                Timer3 = new Timer(QueueTenMinutesTasks, null, 0, TenMinutes);
                Timer4 = new Timer(QueueHourlyTasks, null, OneHour, OneHour);
                Timer5 = new Timer(QueueDailyTasks, null, GetDueTime(), OneDay);
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

        private void QueueDailyTasks(object? state)
        {
            PerformDailyTasks = true;
        }

        private void QueueTenMinutesTasks(object? state)
        {
            PerformTenMinutesTasks = true;
        }

        private void QueueHourlyTasks(object? state)
        {
            PerformHourlyTasks = true;
        }

        private void Scrape(object? state)
        {
            if (Interlocked.Exchange(ref RunningTimer1, 1) == 1)
            {
                return;
            }

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
                Interlocked.Exchange(ref RunningTimer1, 0);
            }
        }

        private void LocalAIParsing(object? state)
        {
            if (Interlocked.Exchange(ref RunningTimer1, 1) == 1)
            {
                return;
            }

            try
            {
                TaskLocalAIParsing.ProcessPages();
            }
            catch (Exception exception)
            {
                Log.WriteError("ServiceTasks LocalAIParsing", exception);
            }
            finally
            {
                Interlocked.Exchange(ref RunningTimer1, 0);
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

        private void BlockingCollection(object? state)
        {
            if (Interlocked.Exchange(ref RunningTimer2, 1) == 1)
            {
                return;
            }

            try
            {
                //Scraper.FinalizeBlockingCollection();
            }
            catch (Exception exception)
            {
                Log.WriteError("ServiceTasks BlockingCollection", exception);
            }
            finally
            {
                Interlocked.Exchange(ref RunningTimer2, 0);
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
                GlobalStatistics.TakeSnapshots();
                HostStatistics.TakeSnapshots();
                DownloadsUpdater.Update();
                Landerist_com.Landerist_com.UpdateAllPages();
                CleanDatabase();
                Backup.Update();
            }
            catch (Exception exception)
            {
                Log.WriteError("ServiceTasks DailyTask", exception);
            }
        }

        private static void CleanDatabase()
        {
            AddressLatLng.Clean();
            AddressCadastralReference.Clean();
            NotListingsCache.Clean();
        }

        public void Stop()
        {
            if (Disposed)
            {
                return;
            }

            Console.WriteLine("Stopping ServiceTasks ..");
            Scraper.Stop();
            TaskLocalAIParsing.Stop();

            DisposeTimer(ref Timer1);
            DisposeTimer(ref Timer2);
            DisposeTimer(ref Timer3);
            DisposeTimer(ref Timer4);
            DisposeTimer(ref Timer5);
        }

        public void Dispose()
        {
            if (Disposed)
            {
                return;
            }

            Stop();
            Disposed = true;
            GC.SuppressFinalize(this);
        }

        private static void DisposeTimer(ref Timer? timer)
        {
            timer?.Change(Timeout.Infinite, Timeout.Infinite);
            timer?.Dispose();
            timer = null;
        }
    }
}
