using landerist_library.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace landerist_console;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        Config.SetToProduction();
        Console.Title = "Landerist Console " + Config.VERSION;

        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddSingleton(_ =>
            LanderistServiceComposition.CreateTasksService());
        builder.Services.AddHostedService<LanderistWorker>();

        using IHost host = builder.Build();
        await host.RunAsync();
    }
}