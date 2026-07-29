using landerist_library.Application.Listings;
using landerist_library.Application.Logging;
using landerist_library.Application.Persistence;
using landerist_library.Application.Statistics;
using landerist_library.Infrastructure.Listings;
using landerist_library.Infrastructure.PageServices;
using landerist_library.Infrastructure.WebsiteServices;using landerist_library.Database;
using landerist_library.Infrastructure.Statistics;
using landerist_library.Infrastructure.Logging;
using landerist_library.Logs;
using landerist_library.Infrastructure.Runtime;
using landerist_library.Infrastructure.Sql;
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

        SqlDatabaseOptions databaseOptions = new(
            runtimeOptions.Database.DataSource,
            runtimeOptions.Database.UserId,
            runtimeOptions.Database.Password,
            runtimeOptions.Database.DatabaseName,
            runtimeOptions.Database.Encrypt,
            runtimeOptions.Database.TrustServerCertificate,
            runtimeOptions.Database.ConnectionTimeoutSeconds,
            runtimeOptions.Database.CommandTimeoutSeconds);
        SqlDatabaseFactory databaseFactory = new(databaseOptions);

        LegacyDatabase.Configure(databaseFactory);
        landerist_library.Infrastructure.Administration.CsvExportService.Configure(
            databaseFactory.Create);
        Log.Configure(
            databaseFactory,
            new LegacyLogOptions(
                runtimeOptions.Execution.LogsEnabled,
                runtimeOptions.Execution.LogErrorsToConsole,
                runtimeOptions.Execution.LogInformationToConsole,
                runtimeOptions.Execution.MachineName),
            TimeProvider.System);
        services.AddSingleton(databaseOptions);
        services.AddSingleton(databaseFactory);
        services.AddSingleton<IDatabaseFactory>(databaseFactory);
        services.AddSingleton<LanderistDatabaseAdapterFactory>();

        PageQueryOptions pageQueryOptions = new(
            runtimeOptions.Execution.IsLocal ? null : runtimeOptions.Execution.MachineName,
            runtimeOptions.Scraping.MaxPagesPerHostPerScrape);
        services.AddSingleton(pageQueryOptions);

        services.AddTransient(_ => new PageRepository(databaseFactory.Create()));
        services.AddTransient(_ => new PageQueryRepository(
            databaseFactory.Create(),
            pageQueryOptions));
        services.AddTransient(_ => new PageMaintenanceRepository(databaseFactory.Create()));
        services.AddTransient(_ => new WebsiteRepository(databaseFactory.Create()));
        services.AddTransient(_ => new WebsiteQueryRepository(databaseFactory.Create()));
        services.AddTransient(_ => new WebsitePageMetricsRepository(databaseFactory.Create()));
        services.AddTransient(_ => new ListingRepository(databaseFactory.Create()));
        services.AddTransient(_ => new ListingQueryRepository(databaseFactory.Create()));
        services.AddTransient(_ => new ListingStatisticsRepository(databaseFactory.Create()));
        services.AddTransient(_ => new MediaRepository(databaseFactory.Create()));
        services.AddTransient(_ => new SourceRepository(databaseFactory.Create()));
        services.AddTransient<IPageRepository>(serviceProvider =>
            serviceProvider.GetRequiredService<PageRepository>());
        services.AddTransient<IWebsiteRepository>(serviceProvider =>
            serviceProvider.GetRequiredService<WebsiteRepository>());
        services.AddTransient<IListingMediaRepository>(serviceProvider =>
            serviceProvider.GetRequiredService<MediaRepository>());
        services.AddTransient<IListingSourceRepository>(serviceProvider =>
            serviceProvider.GetRequiredService<SourceRepository>());
        services.AddTransient(_ => new GlobalStatisticsRepository(databaseFactory.Create()));
        services.AddTransient(_ => new HostStatisticsRepository(databaseFactory.Create()));
        services.AddTransient(_ => new PageStatisticsRepository(databaseFactory.Create()));

        services.AddSingleton<PagePersistenceService>();
        services.AddSingleton<WebsitePersistenceService>();
        services.AddSingleton<SqlListingQueryService>();
        services.AddSingleton<SqlPageCatalog>();
        services.AddSingleton<SqlPageWaitingStatusService>();
        services.AddSingleton<SqlWebsiteCatalog>();
        services.AddSingleton<SqlListingStore>(serviceProvider =>
            serviceProvider.GetRequiredService<LanderistDatabaseAdapterFactory>()
                .CreateListingStore(
                    serviceProvider.GetRequiredService<GlobalStatisticsRepository>(),
                    serviceProvider.GetRequiredService<IApplicationLogger>()));
        services.AddSingleton<SqlNotListingCacheService>(serviceProvider =>
            serviceProvider.GetRequiredService<LanderistDatabaseAdapterFactory>()
                .CreateNotListingCache(
                    runtimeOptions.Scraping.NotListingCacheEnabled));
        services.AddSingleton<WebsiteMetricsService>(serviceProvider => new(
            serviceProvider.GetRequiredService<WebsitePageMetricsRepository>(),
            serviceProvider.GetRequiredService<ListingStatisticsRepository>(),
            runtimeOptions.Scraping.MaxPagesPerWebsite));
        services.AddSingleton<GlobalStatistics>(serviceProvider => new(
            serviceProvider.GetRequiredService<GlobalStatisticsRepository>(),
            persistenceEnabled: !runtimeOptions.Execution.IsLocal));
        services.AddSingleton<HostStatistics>(serviceProvider => new(
            serviceProvider.GetRequiredService<HostStatisticsRepository>(),
            serviceProvider.GetRequiredService<SqlWebsiteCatalog>(),
            persistenceEnabled: !runtimeOptions.Execution.IsLocal));

        return services;
    }
}
