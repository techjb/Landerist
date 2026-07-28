using landerist_library.Application.Tasks;
using landerist_library.Configuration;
using landerist_library.Infrastructure.Runtime;
using Microsoft.Extensions.DependencyInjection;

namespace landerist_console;

internal static class LanderistServiceCollectionExtensions
{
    public static IServiceCollection AddLanderist(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        Config.SetToProduction();
        LanderistRuntimeOptions runtimeOptions =
            LanderistRuntimeOptionsAdapter.FromLegacyConfiguration();

        services.AddSingleton(runtimeOptions);
        services.AddLanderistPersistence(runtimeOptions);
        services.AddSingleton<TasksService>(serviceProvider =>
            LanderistServiceComposition.CreateTasksService(
                serviceProvider.GetRequiredService<LanderistRuntimeOptions>(),
                serviceProvider));

        return services;
    }
}
