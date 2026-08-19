using landerist_library.Application.Listings;
using landerist_library.Application.Logging;
using landerist_library.Application.Persistence;
using landerist_library.Application.Statistics;
using landerist_library.Application.Tasks;
using landerist_library.Application.Websites;
using landerist_library.Infrastructure.Listings;
using landerist_library.Infrastructure.PageServices;
using landerist_library.Infrastructure.Runtime;
using landerist_library.Infrastructure.Sql;
using landerist_library.Infrastructure.Sql.Statistics;
using landerist_library.Infrastructure.Tasks;
using landerist_library.Infrastructure.WebsiteServices;
using Microsoft.Extensions.DependencyInjection;

namespace landerist_console;

internal static class LanderistRecurringTaskServiceCollectionExtensions
{
    public static IServiceCollection AddLanderistRecurringTasks(
        this IServiceCollection services,
        LanderistRuntimeOptions runtimeOptions)
    {
        InMemoryTaskHealthRegistry taskHealth = new();
        services.AddSingleton<ITaskHealthRegistry>(taskHealth);
        services.AddSingleton(new HealthPublisherOptions(
            runtimeOptions.Health.FilePath,
            TimeSpan.FromSeconds(runtimeOptions.Health.IntervalSeconds)));
        services.AddHostedService<LanderistHealthWorker>();
        services.AddSingleton<LanderistDistributionComposition>();
        services.AddSingleton<HourlyTaskJob>(serviceProvider => new(
            new WebsiteRefreshService(
                serviceProvider.GetRequiredService<SqlWebsiteCatalog>(),
                serviceProvider.GetRequiredService<WebsitePersistenceService>(),
                serviceProvider.GetRequiredService<WebsiteNetworkService>(),
                serviceProvider.GetRequiredService<WebsiteSitemapService>()),
            serviceProvider.GetRequiredService<LanderistBatchTasks>().Cleaner));
        services.AddSingleton<DailyTaskJob>(serviceProvider =>
            serviceProvider.GetRequiredService<LanderistDistributionComposition>()
                .CreateDailyJob(
                    serviceProvider.GetRequiredService<SqlNotListingCacheService>(),
                    serviceProvider.GetRequiredService<GlobalStatistics>(),
                    serviceProvider.GetRequiredService<HostStatistics>(),
                    serviceProvider.GetRequiredService<PageStatisticsRepository>(),
                    serviceProvider.GetRequiredService<WebsiteMetricsService>(),
                    serviceProvider.GetRequiredService<SqlWebsiteCatalog>(),
                    serviceProvider.GetRequiredService<WebsiteQueryRepository>()));
        services.AddSingleton<TasksService>(serviceProvider => new(
            new TasksServiceOptions(GetExecutionMode(runtimeOptions.Role)),
            new SystemRecurringTaskScheduler(
                serviceProvider.GetRequiredService<IApplicationLogger>(),
                TimeProvider.System,
                serviceProvider.GetRequiredService<ITaskHealthRegistry>()),
            serviceProvider.GetRequiredService<IApplicationLogger>(),
            serviceProvider.GetRequiredService<ScrapeTaskJob>(),
            serviceProvider.GetRequiredService<LocalAiTaskJob>(),
            serviceProvider.GetRequiredService<LanderistBatchTasks>().TenMinute,
            serviceProvider.GetRequiredService<HourlyTaskJob>(),
            serviceProvider.GetRequiredService<DailyTaskJob>(),
            TimeProvider.System));
        return services;
    }

    private static TasksExecutionMode GetExecutionMode(
        LanderistExecutionRole role) => role switch
        {
            LanderistExecutionRole.LocalAi => TasksExecutionMode.LocalAi,
            LanderistExecutionRole.Principal => TasksExecutionMode.Principal,
            LanderistExecutionRole.Scraper => TasksExecutionMode.Scraper,
            _ => throw new ArgumentOutOfRangeException(
                nameof(role), role, "Unknown execution role.")
        };
}
