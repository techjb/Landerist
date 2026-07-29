using landerist_library.Configuration;
using landerist_library.Database;
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
                Config.LOGS_ENABLED,
                Config.LOGS_ERRORS_IN_CONSOLE,
                Config.LOGS_INFO_IN_CONSOLE,
                Config.MACHINE_NAME),
            TimeProvider.System);
        services.AddSingleton(databaseOptions);
        services.AddSingleton(databaseFactory);
        services.AddSingleton<IDatabaseFactory>(databaseFactory);
        services.AddSingleton<LanderistDatabaseAdapterFactory>();

        PageQueryOptions pageQueryOptions = new(
            runtimeOptions.Browser.IsLocal ? null : Config.MACHINE_NAME,
            Config.MAX_PAGES_PER_HOST_PER_SCRAPE);
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
        services.AddTransient(_ => new GlobalStatisticsRepository(databaseFactory.Create()));
        services.AddTransient(_ => new HostStatisticsRepository(databaseFactory.Create()));
        services.AddTransient(_ => new PageStatisticsRepository(databaseFactory.Create()));

        return services;
    }
}
