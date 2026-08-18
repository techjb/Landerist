using landerist_library.Infrastructure.Runtime;

namespace landerist_library.Infrastructure.Administration;

internal sealed class UnpublishedListingRetentionPolicy(
    AdministrationOptions options,
    TimeProvider timeProvider)
{
    internal DateTime GetThreshold() =>
        timeProvider.GetLocalNow().DateTime
            .AddDays(-options.UnpublishedListingRetentionDays);
}
