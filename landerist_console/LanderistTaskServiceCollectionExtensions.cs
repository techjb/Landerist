using landerist_library.Infrastructure.Runtime;
using Microsoft.Extensions.DependencyInjection;

namespace landerist_console;

internal static class LanderistTaskServiceCollectionExtensions
{
    public static IServiceCollection AddLanderistTasks(
        this IServiceCollection services,
        LanderistRuntimeOptions runtimeOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(runtimeOptions);

        return services
            .AddLanderistParsingTasks()
            .AddLanderistScrapingTasks()
            .AddLanderistLocalAiTasks(runtimeOptions)
            .AddLanderistRecurringTasks(runtimeOptions);
    }
}