namespace landerist_library.Infrastructure.Location.Providers.Goolzoom;

public sealed record GoolzoomOptions(
    string? ApiKey,
    TimeSpan Timeout,
    int MaxRetryAttempts)
{
    public GoolzoomOptions Validate()
    {
        if (Timeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(Timeout),
                "Goolzoom timeout must be positive.");
        }

        if (MaxRetryAttempts <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(MaxRetryAttempts),
                "Goolzoom retry attempts must be positive.");
        }

        return this;
    }
}
