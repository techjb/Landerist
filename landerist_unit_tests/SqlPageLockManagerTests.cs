using landerist_library.Infrastructure.Scraping;

namespace landerist_unit_tests;

public sealed class SqlPageLockManagerTests
{
    [Fact]
    public async Task CleanPageLocksAsync_UsesMachineNameParameter()
    {
        RecordingDatabase database = new() { QueryResult = true };
        SqlPageLockManager manager = new(database, "worker-01");

        await manager.CleanPageLocksAsync(CancellationToken.None);

        Assert.Contains("SET [LockedBy] = NULL", database.LastQuery);
        Assert.Equal("worker-01", database.LastParameters!["LockedBy"]);
    }

    [Fact]
    public async Task CleanPageLocksAsync_PropagatesCancellation()
    {
        RecordingDatabase database = new() { QueryResult = true };
        SqlPageLockManager manager = new(database, "worker-01");
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            manager.CleanPageLocksAsync(cancellation.Token));

        Assert.Empty(database.Calls);
    }
}