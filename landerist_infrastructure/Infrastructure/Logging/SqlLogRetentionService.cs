using landerist_library.Application.Logging;
using landerist_library.Database;

namespace landerist_library.Infrastructure.Logging;

public sealed class SqlLogRetentionService : ILogRetentionService
{
    private readonly IDatabaseFactory _databaseFactory;
    private readonly LogRetentionOptions _options;
    private readonly TimeProvider _timeProvider;

    public SqlLogRetentionService(
        IDatabaseFactory databaseFactory,
        LogRetentionOptions options,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(databaseFactory);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(timeProvider);
        options.Validate();
        _databaseFactory = databaseFactory;
        _options = options;
        _timeProvider = timeProvider;
    }

    public LogRetentionResult Clean()
    {
        DateTime now = _timeProvider.GetLocalNow().DateTime;
        IDatabase database = _databaseFactory.Create();
        int informationDeleted = DeleteInBatches(
            database,
            "[LogKey] <> 'error' AND [Date] < @Cutoff",
            now.AddDays(-_options.InformationRetentionDays));
        int errorsDeleted = DeleteInBatches(
            database,
            "[LogKey] = 'error' AND [Date] < @Cutoff",
            now.AddDays(-_options.ErrorRetentionDays));
        return new LogRetentionResult(informationDeleted, errorsDeleted);
    }

    private int DeleteInBatches(IDatabase database, string predicate, DateTime cutoff)
    {
        int total = 0;
        for (int batch = 0; batch < _options.MaximumBatchesPerRun; batch++)
        {
            string query =
                "DELETE TOP (@BatchSize) FROM [LOGS] WHERE " + predicate + "; " +
                "SELECT @@ROWCOUNT;";
            int deleted = database.QueryInt(query, new Dictionary<string, object?>
            {
                ["BatchSize"] = _options.BatchSize,
                ["Cutoff"] = cutoff
            });
            total = checked(total + deleted);
            if (deleted < _options.BatchSize)
            {
                break;
            }
        }

        return total;
    }
}
