using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace landerist_console;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        Console.Title = "Landerist Console";

        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddLanderist();
        builder.Services.AddHostedService<LanderistWorker>();

        using IHost host = builder.Build();
        await host.RunAsync();
    }
}