using landerist_library.Configuration;
using landerist_library.Downloaders.Puppeteer;
using landerist_library.Logs;
using landerist_library.Scrape;

namespace landerist_scraper
{
    public class WorkerScraper(ILogger<WorkerScraper> logger) : BackgroundService
    {
        private readonly ILogger<WorkerScraper> Logger = logger;

        private Timer? Timer1;
        private bool RunningScraper = false;

        private const int OneSecond = 1000;
        private const int TenSeconds = 10 * OneSecond;

        private readonly Scraper Scraper = new();

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Logger.LogInformation("ExecuteAsync");
            while (!stoppingToken.IsCancellationRequested)
            {
                Log.WriteInfo("landerist_scraper", "Started. Version: " + Config.VERSION);
                PuppeteerDownloader.UpdateChrome();
                SetTimers();
                //Scraper.DoTest();

                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
        }

        private void SetTimers()
        {
            Timer1 = new Timer(TimerScrape!, null, 0, TenSeconds);
        }

        private void TimerScrape(object state)
        {
            if (RunningScraper)
            {
                return;
            }

            RunningScraper = true;
            try
            {
                Scraper.Start();
            }
            catch (Exception exception)
            {
                Log.WriteError("WorkerScraper TimerScrape", exception);
            }
            finally
            {
                RunningScraper = false;
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("StopAsync");
            Scraper.Stop();
            Log.WriteInfo("landerist_scraper", "Stopped. Version: " + Config.VERSION);
            Timer1?.Change(Timeout.Infinite, 0);
            await base.StopAsync(cancellationToken);
        }
    }
}
