using landerist_library.Database;
using landerist_library.Downloaders.Puppeteer;
using landerist_library.Landerist_com;
using landerist_library.Logs;
using landerist_library.Parse.ListingParser.OpenAI.Batch;
using landerist_library.Parse.ListingParser.VertexAI.Batch;
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

        private bool RunningTimer2 = false;
        private bool RunningTimer3 = false;

        private const int OneSecond = 1000;
        private const int TenSeconds = 10 * OneSecond;
        private const int OneMinute = 60 * OneSecond;
        private const int OneHour = 60 * OneMinute;
        private const int OneDay = 24 * OneHour;

        private bool PerformDailyTasks = false;

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
            DateTime nowTime = DateTime.Now;

            DateTime twelveAM = new(nowTime.Year, nowTime.Month, nowTime.Day, 0, 0, 0);
            if (nowTime > twelveAM)
            {
                twelveAM = twelveAM.AddDays(1);
            }

            int dueTimeOneAM = (int)(twelveAM - nowTime).TotalMilliseconds;

            Timer1 = new Timer(DailyTasks!, null, dueTimeOneAM, OneDay);
            Timer2 = new Timer(UpdateAndScrape!, null, 0, TenSeconds);
            Timer3 = new Timer(BlockingCollection!, null, OneSecond, OneSecond);
        }

        private void DailyTasks(object state)
        {
            PerformDailyTasks = true;
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
                if (PerformDailyTasks)
                {
                    DailyTask();
                    PerformDailyTasks = false;
                }
                Update();
                Scrape();
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

        public static void DailyTask()
        {
            try
            {
                Pages.DeleteUnpublishedListings();
                StatisticsSnapshot.TakeSnapshots();
                DownloadFilesUpdater.UpdateFiles();
                Landerist_com.Landerist_com.UpdateDownloadsAndStatisticsPages();
                Backup.Update();
                VertexAIBatchCleaner.RemoveFiles();
                OpenAIBatchCleaner.RemoveFiles();
            }
            catch (Exception exception)
            {
                Log.WriteError("ServiceTasks DailyTask", exception);
            }

        }

        public static void Update()
        {
            Websites.Websites.UpdateRobotsTxt();
            Websites.Websites.UpdateSitemaps();
            Websites.Websites.UpdateIpAddress();
            BatchTasks.Start();
        }

        public void Scrape()
        {
            Scraper.Start();
        }

        public void Stop()
        {
            Scraper.Stop();
            Timer1?.Change(Timeout.Infinite, 0);
            Timer2?.Change(Timeout.Infinite, 0);
            Timer3?.Change(Timeout.Infinite, 0);
        }
    }
}
