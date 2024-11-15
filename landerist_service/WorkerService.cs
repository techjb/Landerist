using landerist_library.Configuration;
using landerist_library.Logs;
using landerist_library.Tasks;

namespace landerist_service
{
    public class WorkerService(ILogger<WorkerService> logger) : BackgroundService
    {
        private readonly ILogger<WorkerService> Logger = logger;

        private readonly ServiceTasks ServiceTasks = new();

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Logger.LogInformation("ExecuteAsync");
            while (!stoppingToken.IsCancellationRequested)
            {
                Log.Delete();
                Log.WriteInfo("landerist_service", "Started. Version: " + Config.VERSION);
                ServiceTasks.Start();
                //Scraper.DoTest();

                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("StopAsync");
            ServiceTasks.Stop();
            Log.WriteInfo("landerist_service", "Stopped. Version: " + Config.VERSION);           
            await base.StopAsync(cancellationToken);
        }
    }
}
