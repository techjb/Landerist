using landerist_library.Application.Logging;
using landerist_library.Database;

namespace landerist_library.Infrastructure.Logging;

public sealed class SqlApplicationLogger : IApplicationLogger
{
    private const string LogsTable = "[LOGS]";
    private readonly IDatabaseFactory _databaseFactory;
    private readonly ApplicationLoggerOptions _options;
    private readonly TimeProvider _timeProvider;

    public SqlApplicationLogger(
        IDatabaseFactory databaseFactory,
        ApplicationLoggerOptions options,
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

    public void WriteError(string source, string message)
    {
        if (_options.ErrorsInConsole)
        {
            WriteConsole(source, message);
        }

        Write("error", source, message);
    }

    public void WriteInfo(string source, string message)
    {
        if (_options.InformationInConsole)
        {
            WriteConsole(source, message);
        }

        Write("info", source, message);
    }

    private void Write(string logKey, string source, string message)
    {
        if (!_options.PersistenceEnabled || string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        const string query =
            "INSERT INTO " + LogsTable +
            " ([Date], [MachineName], [LogKey], [Source], [Text]) " +
            "VALUES(@Date, @MachineName, @LogKey, @Source, @Text)";
        _databaseFactory.Create().Query(
            query,
            new Dictionary<string, object?>
            {
                ["Date"] = _timeProvider.GetLocalNow().DateTime,
                ["MachineName"] = _options.MachineName,
                ["LogKey"] = logKey,
                ["Source"] = source ?? string.Empty,
                ["Text"] = message.Trim()
            });
    }

    private void WriteConsole(string source, string message)
    {
        DateTimeOffset now = _timeProvider.GetLocalNow();
        Console.WriteLine($"{now:HH\\:mm\\:ss} {source} {message}");
    }
}
