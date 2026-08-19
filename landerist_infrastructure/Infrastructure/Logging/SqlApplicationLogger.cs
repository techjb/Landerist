using landerist_library.Application.Logging;
using landerist_library.Database;

namespace landerist_library.Infrastructure.Logging;

public sealed class SqlApplicationLogger : IApplicationLogger
{
    private const string LogsTable = "[LOGS]";
    private readonly IDatabaseFactory _databaseFactory;
    private readonly ApplicationLoggerOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly Action<string> _writeStandardError;

    public SqlApplicationLogger(
        IDatabaseFactory databaseFactory,
        ApplicationLoggerOptions options,
        TimeProvider timeProvider)
        : this(databaseFactory, options, timeProvider, Console.Error.WriteLine)
    {
    }

    internal SqlApplicationLogger(
        IDatabaseFactory databaseFactory,
        ApplicationLoggerOptions options,
        TimeProvider timeProvider,
        Action<string> writeStandardError)
    {
        ArgumentNullException.ThrowIfNull(databaseFactory);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(timeProvider);
        ArgumentNullException.ThrowIfNull(writeStandardError);
        options.Validate();

        _databaseFactory = databaseFactory;
        _options = options;
        _timeProvider = timeProvider;
        _writeStandardError = writeStandardError;
    }

    public void WriteError(string source, string message)
    {
        WriteStandardError("error", source, message);
        if (_options.ErrorsInConsole)
        {
            WriteConsole(source, message);
        }

        TryWrite("error", source, message);
    }

    public void WriteInfo(string source, string message)
    {
        if (_options.InformationInConsole)
        {
            WriteConsole(source, message);
        }

        TryWrite("info", source, message);
    }

    private void TryWrite(string logKey, string source, string message)
    {
        try
        {
            Write(logKey, source, message);
        }
        catch (Exception exception)
        {
            WriteStandardError(
                "logging-persistence-failure",
                source,
                $"Original level: {logKey}. Persistence error: {exception}");
        }
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

    private void WriteStandardError(string level, string source, string message)
    {
        try
        {
            DateTimeOffset now = _timeProvider.GetLocalNow();
            _writeStandardError(
                $"{now:O} level={level} machine={_options.MachineName} " +
                $"source={source ?? string.Empty} message={message ?? string.Empty}");
        }
        catch
        {
            // The emergency sink must never replace the original application failure.
        }
    }
}
