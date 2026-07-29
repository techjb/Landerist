using landerist_library.Application.Tasks;
using landerist_library.Infrastructure.Runtime;
using Microsoft.Extensions.DependencyInjection;

namespace landerist_console;

internal static class LanderistServiceCollectionExtensions
{
    public static IServiceCollection AddLanderist(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        LanderistRuntimeOptions runtimeOptions =
            LanderistRuntimeOptionsAdapter.FromLegacyConfiguration();

        return services.AddLanderist(runtimeOptions);
    }

    internal static IServiceCollection AddLanderist(
        this IServiceCollection services,
        LanderistRuntimeOptions runtimeOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(runtimeOptions);
        runtimeOptions.Validate();

        return services
            .AddLanderistRuntime(runtimeOptions)
            .AddLanderistPersistence(runtimeOptions)
            .AddLanderistScraping(runtimeOptions)
            .AddLanderistTasks();
    }

    private static IServiceCollection AddLanderistRuntime(
        this IServiceCollection services,
        LanderistRuntimeOptions runtimeOptions)
    {
        services.AddSingleton(runtimeOptions);
        services.AddSingleton(runtimeOptions.Ai);
        services.AddSingleton(runtimeOptions.Batch);
        services.AddSingleton(runtimeOptions.Scraping);
        services.AddSingleton(runtimeOptions.Integrations);
        services.AddSingleton(runtimeOptions.Execution);
        return services;
    }

    private static IServiceCollection AddLanderistTasks(
        this IServiceCollection services)
    {
        services.AddSingleton<TasksService>(serviceProvider =>
            LanderistServiceComposition.CreateTasksService(
                serviceProvider.GetRequiredService<LanderistRuntimeOptions>(),
                serviceProvider));
        return services;
    }
}