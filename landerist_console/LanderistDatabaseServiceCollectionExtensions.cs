using landerist_library.Database;
using landerist_library.Infrastructure.Logging;
using landerist_library.Infrastructure.Runtime;
using landerist_library.Infrastructure.Sql;
using landerist_library.Logs;
using Microsoft.Extensions.DependencyInjection;

namespace landerist_console;

internal static class LanderistDatabaseServiceCollectionExtensions
{
    public static IServiceCollection AddLanderistDatabase(
        this IServiceCollection services,
        LanderistRuntimeOptions runtimeOptions)
    {
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
        return services;
    }
}