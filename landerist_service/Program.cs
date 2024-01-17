using landerist_library.Configuration;


namespace landerist_service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // todo: uncomment
            //Config.SetToProduction();

            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                })
                //UseWindowsService()
                .Build();            

            host.Run();
        }
    }
}