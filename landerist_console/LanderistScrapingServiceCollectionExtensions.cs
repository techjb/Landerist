using landerist_library.Infrastructure.Runtime;
using Microsoft.Extensions.DependencyInjection;

namespace landerist_console;

internal static class LanderistScrapingServiceCollectionExtensions
{
    public static IServiceCollection AddLanderistScraping(
        this IServiceCollection services,
        LanderistRuntimeOptions runtimeOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(runtimeOptions);
        runtimeOptions.Proxy.Validate();
        runtimeOptions.Browser.Validate();

        return services
            .AddLanderistScrapingInfrastructure(runtimeOptions)
            .AddLanderistWebsiteScraping(runtimeOptions)
            .AddLanderistListingScraping(runtimeOptions);
    }
}