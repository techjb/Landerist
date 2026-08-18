using landerist_library.Application.Parsing;

namespace landerist_library.Infrastructure.Tasks;

public sealed record BatchUploadOptions
{
    public BatchUploadOptions(
        BatchProvider provider,
        int maxPagesPerBatch,
        int minPagesPerBatch,
        int maxInputTokens,
        bool updateWaitingResponse,
        int statusUpdateParallelism = -1)
    {
        if (maxPagesPerBatch <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxPagesPerBatch));
        if (minPagesPerBatch <= 0 || minPagesPerBatch > maxPagesPerBatch)
            throw new ArgumentOutOfRangeException(nameof(minPagesPerBatch));
        if (maxInputTokens <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxInputTokens));
        if (statusUpdateParallelism is 0 or < -1)
            throw new ArgumentOutOfRangeException(nameof(statusUpdateParallelism));

        Provider = provider;
        MaxPagesPerBatch = maxPagesPerBatch;
        MinPagesPerBatch = minPagesPerBatch;
        MaxInputTokens = maxInputTokens;
        UpdateWaitingResponse = updateWaitingResponse;
        StatusUpdateParallelism = statusUpdateParallelism;
    }

    public BatchProvider Provider { get; }
    public int MaxPagesPerBatch { get; }
    public int MinPagesPerBatch { get; }
    public int MaxInputTokens { get; }
    public bool UpdateWaitingResponse { get; }
    public int StatusUpdateParallelism { get; }

    public ParallelOptions CreateStatusParallelOptions() =>
        new() { MaxDegreeOfParallelism = StatusUpdateParallelism };
}
