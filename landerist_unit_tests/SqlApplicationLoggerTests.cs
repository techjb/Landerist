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
}
