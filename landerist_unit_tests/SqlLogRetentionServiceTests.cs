using landerist_library.Database;
using landerist_library.Infrastructure.Logging;

namespace landerist_unit_tests;

public sealed class SqlLogRetentionServiceTests
{
    [Fact]
    public void Clean_UsesSeparateCutoffsForInformationAndErrors()
    {
        RecordingDatabase database = new() { QueryIntResult = 7 };
        SqlLogRetentionService service = new(
            new StubDatabaseFactory(database),
            new LogRetentionOptions(90, 365, 1_000, 100),
            new FixedTimeProvider(
                new DateTimeOffset(2026, 8, 19, 12, 0, 0, TimeSpan.Zero)));

        var result = service.Clean();

        Assert.Equal(14, result.TotalDeleted);
        Assert.Equal(2, database.Calls.Count);
        Assert.Contains("[LogKey] <> 'error'", database.Calls[0].Query);
        Assert.Equal(new DateTime(2026, 5, 21, 12, 0, 0), database.Calls[0].Parameters!["Cutoff"]);
        Assert.Contains("[LogKey] = 'error'", database.Calls[1].Query);
        Assert.Equal(new DateTime(2025, 8, 19, 12, 0, 0), database.Calls[1].Parameters!["Cutoff"]);
        Assert.Equal(1_000, database.Calls[0].Parameters!["BatchSize"]);
    }

    [Fact]
    public void Options_RejectErrorRetentionShorterThanInformationRetention()
    {
        LogRetentionOptions options = new(90, 30, 1_000, 100);

        Assert.Throws<InvalidOperationException>(options.Validate);
    }

    private sealed class StubDatabaseFactory(IDatabase database) : IDatabaseFactory
    {
        public IDatabase Create() => database;
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;

        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
    }
}
