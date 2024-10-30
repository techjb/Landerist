using landerist_library.Configuration;

namespace landerist_service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Config.SetToProduction();

            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<WorkerService>();
                })
                .UseWindowsService()
                .Build();

            host.Run();
        }
    }
}