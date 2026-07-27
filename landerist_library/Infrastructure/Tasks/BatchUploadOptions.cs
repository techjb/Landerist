using landerist_library.Parse.ListingParser;

namespace landerist_library.Infrastructure.Tasks;

public sealed record BatchUploadOptions
{
    public BatchUploadOptions(
        LLMProvider provider,
        string directory,
        long maxFileSizeInBytes,
        int maxPagesPerBatch,
        int minPagesPerBatch,
        int maxInputTokens,
        bool updateWaitingResponse,
        int statusUpdateParallelism = -1)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        if (maxFileSizeInBytes <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxFileSizeInBytes));
        if (maxPagesPerBatch <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxPagesPerBatch));
        if (minPagesPerBatch <= 0 || minPagesPerBatch > maxPagesPerBatch)
            throw new ArgumentOutOfRangeException(nameof(minPagesPerBatch));
        if (maxInputTokens <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxInputTokens));
        if (statusUpdateParallelism is 0 or < -1)
            throw new ArgumentOutOfRangeException(nameof(statusUpdateParallelism));

        Provider = provider;
        Directory = directory;
        MaxFileSizeInBytes = maxFileSizeInBytes;
        MaxPagesPerBatch = maxPagesPerBatch;
        MinPagesPerBatch = minPagesPerBatch;
        MaxInputTokens = maxInputTokens;
        UpdateWaitingResponse = updateWaitingResponse;
        StatusUpdateParallelism = statusUpdateParallelism;
    }

    public LLMProvider Provider { get; }
    public string Directory { get; }
    public long MaxFileSizeInBytes { get; }
    public int MaxPagesPerBatch { get; }
    public int MinPagesPerBatch { get; }
    public int MaxInputTokens { get; }
    public bool UpdateWaitingResponse { get; }
    public int StatusUpdateParallelism { get; }

    public ParallelOptions CreateStatusParallelOptions() =>
        new() { MaxDegreeOfParallelism = StatusUpdateParallelism };
}
