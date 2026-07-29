using landerist_library.Infrastructure.Runtime;
using Microsoft.Extensions.DependencyInjection;

namespace landerist_console;

internal static class LanderistPersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddLanderistPersistence(
        this IServiceCollection services,
        LanderistRuntimeOptions runtimeOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(runtimeOptions);
        runtimeOptions.Database.Validate();

        return services
            .AddLanderistDatabase(runtimeOptions)
            .AddLanderistRepositories(runtimeOptions)
            .AddLanderistPersistenceServices(runtimeOptions);
    }
}