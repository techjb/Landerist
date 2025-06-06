using landerist_library.Configuration;

namespace landerist_scraper
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Config.SetToProduction();

            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddHostedService<WorkerScraper>();

            var host = builder.Build();
            host.Run();
        }
    }
}