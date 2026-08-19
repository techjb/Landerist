namespace landerist_library.Infrastructure.Logging;

public sealed record LogRetentionOptions(
    int InformationRetentionDays,
    int ErrorRetentionDays,
    int BatchSize,
    int MaximumBatchesPerRun)
{
    public static LogRetentionOptions Default { get; } = new(90, 365, 1_000, 100);

    public void Validate()
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(InformationRetentionDays);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(ErrorRetentionDays);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(BatchSize);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(MaximumBatchesPerRun);
        if (ErrorRetentionDays < InformationRetentionDays)
        {
            throw new InvalidOperationException(
                "Error log retention cannot be shorter than information log retention.");
        }
    }
}
