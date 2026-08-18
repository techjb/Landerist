using landerist_library.Infrastructure.Administration;
using landerist_library.Infrastructure.Runtime;

namespace landerist_unit_tests;

public sealed class AdministrationOptionsTests
{
    [Fact]
    public void RetentionPolicy_UsesInjectedClockAndConfiguredDays()
    {
        UnpublishedListingRetentionPolicy policy = new(
            new AdministrationOptions(30, "HostMainUri.csv"),
            new FixedTimeProvider(
                new DateTimeOffset(2026, 8, 18, 12, 0, 0, TimeSpan.Zero)));

        DateTime threshold = policy.GetThreshold();

        Assert.Equal(new DateTime(2026, 7, 19, 12, 0, 0), threshold);
    }

    [Fact]
    public void CleanupReader_WhenFileDoesNotExist_ReturnsEmptyCollection()
    {
        WebsiteCleanupCsvReader reader = new();

        IReadOnlyCollection<string> hosts = reader.ReadHostsWithoutListingUrl(
            Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".csv"));

        Assert.Empty(hosts);
    }

    [Theory]
    [InlineData(0, "HostMainUri.csv")]
    [InlineData(30, "")]
    public void Validate_RejectsInvalidAdministrationOptions(
        int retentionDays,
        string filePath)
    {
        AdministrationOptions options = new(retentionDays, filePath);

        Assert.ThrowsAny<ArgumentException>(options.Validate);
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
    }
}
