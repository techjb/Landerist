using landerist_library.Database;
using landerist_library.Infrastructure.Logging;

namespace landerist_unit_tests;

public sealed class SqlApplicationLoggerTests
{
    [Fact]
    public void WriteInfo_PersistsStructuredLogEntry()
    {
        RecordingDatabase database = new();
        SqlApplicationLogger logger = new(
            new StubDatabaseFactory(database),
            new ApplicationLoggerOptions(
                PersistenceEnabled: true,
                ErrorsInConsole: false,
                InformationInConsole: false,
                MachineName: "worker-01"),
            TimeProvider.System);

        logger.WriteInfo("scraper", "  completed  ");

        Assert.Contains("INSERT INTO [LOGS]", database.LastQuery);
        Assert.Equal("info", database.LastParameters!["LogKey"]);
        Assert.Equal("scraper", database.LastParameters["Source"]);
        Assert.Equal("completed", database.LastParameters["Text"]);
        Assert.Equal("worker-01", database.LastParameters["MachineName"]);
    }

    [Fact]
    public void WriteError_DoesNotOpenDatabaseWhenPersistenceIsDisabled()
    {
        RecordingDatabase database = new();
        StubDatabaseFactory databaseFactory = new(database);
        SqlApplicationLogger logger = new(
            databaseFactory,
            new ApplicationLoggerOptions(
                PersistenceEnabled: false,
                ErrorsInConsole: false,
                InformationInConsole: false,
                MachineName: "worker-01"),
            TimeProvider.System);

        logger.WriteError("scraper", "failed");

        Assert.Equal(0, databaseFactory.CreateCalls);
        Assert.Empty(database.Calls);
    }

    [Fact]
    public void Options_RejectMissingMachineName()
    {
        ApplicationLoggerOptions options = new(
            PersistenceEnabled: true,
            ErrorsInConsole: false,
            InformationInConsole: false,
            MachineName: string.Empty);

        Assert.Throws<ArgumentException>(options.Validate);
    }

    [Fact]
    public void WriteError_AlwaysWritesStructuredFallbackBeforePersistence()
    {
        List<string> fallback = [];
        SqlApplicationLogger logger = CreateLogger(
            new StubDatabaseFactory(new RecordingDatabase()),
            fallback.Add);

        logger.WriteError("DailyTask", "database failed");

        string entry = Assert.Single(fallback);
        Assert.Contains("level=error", entry);
        Assert.Contains("machine=worker-01", entry);
        Assert.Contains("source=DailyTask", entry);
        Assert.Contains("message=database failed", entry);
    }

    [Fact]
    public void WriteError_WhenPersistenceThrows_ReportsFailureAndDoesNotThrow()
    {
        List<string> fallback = [];
        SqlApplicationLogger logger = CreateLogger(
            new ThrowingDatabaseFactory(new InvalidOperationException("SQL unavailable")),
            fallback.Add);

        Exception? exception = Record.Exception(() =>
            logger.WriteError("ScrapeTask", "original failure"));

        Assert.Null(exception);
        Assert.Equal(2, fallback.Count);
        Assert.Contains("level=error", fallback[0]);
        Assert.Contains("original failure", fallback[0]);
        Assert.Contains("level=logging-persistence-failure", fallback[1]);
        Assert.Contains("SQL unavailable", fallback[1]);
    }

    private static SqlApplicationLogger CreateLogger(
        IDatabaseFactory databaseFactory,
        Action<string> fallback) => new(
            databaseFactory,
            new ApplicationLoggerOptions(true, false, false, "worker-01"),
            TimeProvider.System,
            fallback);

    private sealed class StubDatabaseFactory(IDatabase database)
        : IDatabaseFactory
    {
        public int CreateCalls { get; private set; }

        public IDatabase Create()
        {
            CreateCalls++;
            return database;
        }
    }

    private sealed class ThrowingDatabaseFactory(Exception exception)
        : IDatabaseFactory
    {
        public IDatabase Create() => throw exception;
    }
}
