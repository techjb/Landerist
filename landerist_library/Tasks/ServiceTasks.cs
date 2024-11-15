using landerist_library.Database;
using landerist_library.Landerist_com;
using landerist_library.Logs;
using landerist_library.Parse.Listing.OpenAI.Batch;
using landerist_library.Scrape;
using landerist_library.Statistics;
using landerist_library.Websites;

namespace landerist_library.Tasks
{
    public class ServiceTasks
    {
        private readonly Scraper Scraper;
        private bool RunningTimer = false;
        private readonly Timer? Timer;
        private const int OneSecond = 1000;

        public ServiceTasks()
        {
            Scraper = new();
            Timer = new Timer(BlockingCollection!, null, OneSecond, OneSecond);
        }

        private void BlockingCollection(object state)
        {
            if (RunningTimer)
            {
                return;
            }

            RunningTimer = true;
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
                RunningTimer = false;
            }
        }

        public static void DailyTask()
        {
            Pages.DeleteUnpublishedListings();
            StatisticsSnapshot.TakeSnapshots();
            DownloadFilesUpdater.UpdateFiles();
            Landerist_com.Landerist_com.UpdateDownloadsAndStatisticsPages();
            Backup.Update();
        }

        public void UpdateAndScrape()
        {
            Update();
            Scrape();
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
            Timer?.Change(Timeout.Infinite, 0);
        }
    }
}
