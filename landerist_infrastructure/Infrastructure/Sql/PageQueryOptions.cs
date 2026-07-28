namespace landerist_library.Infrastructure.Sql;

public sealed record PageQueryOptions
{
    public static PageQueryOptions Default { get; } = new(null, int.MaxValue);

    public PageQueryOptions(string? lockedBy, int maxPagesPerHost)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxPagesPerHost);
        LockedBy = lockedBy;
        MaxPagesPerHost = maxPagesPerHost;
    }

    public string? LockedBy { get; }

    public int MaxPagesPerHost { get; }
}