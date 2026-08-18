using landerist_library.Application.Listings;
using landerist_library.Application.Persistence;
using landerist_library.Database;
using landerist_library.Infrastructure.Runtime;
using landerist_library.Infrastructure.Sql.Statistics;
using landerist_library.Infrastructure.Sql;
using Microsoft.Extensions.DependencyInjection;

namespace landerist_console;

internal static class LanderistRepositoryServiceCollectionExtensions
{
    public static IServiceCollection AddLanderistRepositories(
        this IServiceCollection services,
        LanderistRuntimeOptions runtimeOptions)
    {
        PageQueryOptions pageQueryOptions = new(
            runtimeOptions.Execution.IsLocal
                ? null
                : runtimeOptions.Execution.MachineName,
            runtimeOptions.Scraping.MaxPagesPerHostPerScrape);
        services.AddSingleton(pageQueryOptions);

        services.AddTransient(serviceProvider => new PageRepository(
            CreateDatabase(serviceProvider)));
        services.AddTransient(serviceProvider => new PageQueryRepository(
            CreateDatabase(serviceProvider),
            pageQueryOptions));
        services.AddTransient(serviceProvider => new PageMaintenanceRepository(
            CreateDatabase(serviceProvider)));
        services.AddTransient(serviceProvider => new WebsiteRepository(
            CreateDatabase(serviceProvider)));
        services.AddTransient(serviceProvider => new WebsiteQueryRepository(
            CreateDatabase(serviceProvider)));
        services.AddTransient(serviceProvider => new WebsitePageMetricsRepository(
            CreateDatabase(serviceProvider)));
        services.AddTransient(serviceProvider => new ListingRepository(
            CreateDatabase(serviceProvider)));
        services.AddTransient(serviceProvider => new ListingQueryRepository(
            CreateDatabase(serviceProvider)));
        services.AddTransient(serviceProvider => new ListingStatisticsRepository(
            CreateDatabase(serviceProvider)));
        services.AddTransient(serviceProvider => new MediaRepository(
            CreateDatabase(serviceProvider)));
        services.AddTransient(serviceProvider => new SourceRepository(
            CreateDatabase(serviceProvider)));
        services.AddTransient(serviceProvider => new GlobalStatisticsRepository(
            CreateDatabase(serviceProvider)));
        services.AddTransient(serviceProvider => new HostStatisticsRepository(
            CreateDatabase(serviceProvider)));
        services.AddTransient(serviceProvider => new PageStatisticsRepository(
            CreateDatabase(serviceProvider)));

        services.AddTransient<IPageRepository>(serviceProvider =>
            serviceProvider.GetRequiredService<PageRepository>());
        services.AddTransient<IWebsiteRepository>(serviceProvider =>
            serviceProvider.GetRequiredService<WebsiteRepository>());
        services.AddTransient<IListingRecordRepository>(serviceProvider =>
            serviceProvider.GetRequiredService<ListingRepository>());
        services.AddTransient<IListingMediaRepository>(serviceProvider =>
            serviceProvider.GetRequiredService<MediaRepository>());
        services.AddTransient<IListingSourceRepository>(serviceProvider =>
            serviceProvider.GetRequiredService<SourceRepository>());
        return services;
    }

    private static IDatabase CreateDatabase(IServiceProvider serviceProvider) =>
        serviceProvider.GetRequiredService<IDatabaseFactory>().Create();
}
