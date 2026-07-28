namespace landerist_library.Infrastructure.Tasks;

public sealed record BatchDownloadOptions
{
    public BatchDownloadOptions(int parallelism)
    {
        if (parallelism is 0 or < -1)
        {
            throw new ArgumentOutOfRangeException(nameof(parallelism));
        }

        ParallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = parallelism
        };
    }

    public ParallelOptions ParallelOptions { get; }
}
