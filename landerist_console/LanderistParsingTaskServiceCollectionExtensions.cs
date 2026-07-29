using landerist_library.Infrastructure.Parsing;
using Microsoft.Extensions.DependencyInjection;

namespace landerist_console;

internal static class LanderistParsingTaskServiceCollectionExtensions
{
    public static IServiceCollection AddLanderistParsingTasks(
        this IServiceCollection services)
    {
        services.AddSingleton<LanderistListingParserProviderComposition>();
        services.AddSingleton<LanderistAiComposition>();
        services.AddSingleton<ParseListing>(serviceProvider =>
            serviceProvider.GetRequiredService<LanderistAiComposition>()
                .CreateListingParser());
        return services;
    }
}