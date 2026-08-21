using landerist_library.Infrastructure.Tasks;
using landerist_library.Application.Tasks;

namespace landerist_unit_tests;

public sealed class InMemoryTaskHealthRegistryTests
{
    [Fact]
    public void Snapshot_AfterSuccessfulExecution_IsHealthy()
    {
        InMemoryTaskHealthRegistry registry = new();
        DateTimeOffset now = new(2026, 8, 19, 12, 0, 0, TimeSpan.Zero);
        registry.Register("Daily", now, TimeSpan.FromHours(24));
        registry.Started("Daily", now);
        registry.Succeeded("Daily", now.AddMinutes(2), TimeSpan.FromMinutes(2));

        var snapshot = Assert.Single(registry.Snapshot(now.AddHours(1)));

        Assert.Equal("healthy", snapshot.Status);
        Assert.Equal(0, snapshot.ConsecutiveFailures);
        Assert.Equal(120_000, snapshot.LastDurationMilliseconds);
    }

    [Fact]
    public void Snapshot_AfterFailure_IsDegradedUntilNextSuccess()
    {
        InMemoryTaskHealthRegistry registry = new();
        DateTimeOffset now = new(2026, 8, 19, 12, 0, 0, TimeSpan.Zero);
        registry.Register("Hourly", now, TimeSpan.FromHours(1));
        registry.Started("Hourly", now);
        registry.Failed("Hourly", now.AddMinutes(1), TimeSpan.FromMinutes(1), "SQL failed");

        TaskHealthSnapshot failed = Assert.Single(registry.Snapshot(now.AddMinutes(2)));
        Assert.Equal("degraded", failed.Status);
        Assert.Equal(1, failed.ConsecutiveFailures);

        registry.Started("Hourly", now.AddMinutes(3));
        registry.Succeeded("Hourly", now.AddMinutes(4), TimeSpan.FromMinutes(1));
        TaskHealthSnapshot recovered = Assert.Single(registry.Snapshot(now.AddMinutes(5)));
        Assert.Equal("healthy", recovered.Status);
        Assert.Equal(0, recovered.ConsecutiveFailures);
    }

    [Fact]
    public void Snapshot_WhenExecutionExceedsTwoIntervals_IsDegradedAsStuck()
    {
        InMemoryTaskHealthRegistry registry = new();
        DateTimeOffset now = new(2026, 8, 19, 12, 0, 0, TimeSpan.Zero);
        registry.Register("Scrape", now, TimeSpan.FromMinutes(10));
        registry.Started("Scrape", now);

        TaskHealthSnapshot snapshot = Assert.Single(
            registry.Snapshot(now.AddMinutes(21)));

        Assert.Equal("degraded", snapshot.Status);
        Assert.Contains("stuck", snapshot.LastError);
    }

    [Fact]
    public void Snapshot_LongRunningExecutionWithRecentProgress_IsHealthy()
    {
        InMemoryTaskHealthRegistry registry = new();
        DateTimeOffset now = new(2026, 8, 19, 12, 0, 0, TimeSpan.Zero);
        registry.Register(
            "LocalAIParsing",
            now,
            TimeSpan.FromSeconds(2),
            TimeSpan.FromMinutes(15));
        registry.Started("LocalAIParsing", now);
        registry.Progress("LocalAIParsing", now.AddMinutes(14));

        TaskHealthSnapshot snapshot = Assert.Single(
            registry.Snapshot(now.AddMinutes(20)));

        Assert.Equal("healthy", snapshot.Status);
        Assert.Equal(now.AddMinutes(14), snapshot.LastProgressAt);
    }

    [Fact]
    public void Snapshot_LongRunningExecutionWithoutRecentProgress_IsDegraded()
    {
        InMemoryTaskHealthRegistry registry = new();
        DateTimeOffset now = new(2026, 8, 19, 12, 0, 0, TimeSpan.Zero);
        registry.Register(
            "UpdateAndScrape",
            now,
            TimeSpan.FromSeconds(3),
            TimeSpan.FromMinutes(10));
        registry.Started("UpdateAndScrape", now);
        registry.Progress("UpdateAndScrape", now.AddMinutes(4));

        TaskHealthSnapshot snapshot = Assert.Single(
            registry.Snapshot(now.AddMinutes(15)));

        Assert.Equal("degraded", snapshot.Status);
        Assert.Contains("stuck", snapshot.LastError);
    }
}
