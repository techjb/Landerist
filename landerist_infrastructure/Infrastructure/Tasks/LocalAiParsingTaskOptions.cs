namespace landerist_library.Infrastructure.Tasks;

public sealed record LocalAiParsingTaskOptions
{
    public const int DefaultMaxPagesPerTask = 100;
    public const int DefaultMaxConcurrentSequences = 4;
    public const int DefaultCompletionTokens = 7000;
    public const int DefaultModelMaxTokens = 30000;

    public LocalAiParsingTaskOptions(
        int maxPagesPerTask = DefaultMaxPagesPerTask,
        int maxConcurrentSequences = DefaultMaxConcurrentSequences,
        int completionTokens = DefaultCompletionTokens,
        int modelMaxTokens = DefaultModelMaxTokens,
        bool runSequentially = false,
        bool updateWaitingStatusOnStart = false)
    {
        if (maxPagesPerTask <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxPagesPerTask));
        if (maxConcurrentSequences <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxConcurrentSequences));
        if (completionTokens < 0)
            throw new ArgumentOutOfRangeException(nameof(completionTokens));
        if (modelMaxTokens <= completionTokens)
            throw new ArgumentOutOfRangeException(nameof(modelMaxTokens));

        MaxPagesPerTask = maxPagesPerTask;
        MaxConcurrentSequences = maxConcurrentSequences;
        CompletionTokens = completionTokens;
        ModelMaxTokens = modelMaxTokens;
        RunSequentially = runSequentially;
        UpdateWaitingStatusOnStart = updateWaitingStatusOnStart;
    }

    public int MaxPagesPerTask { get; }
    public int MaxConcurrentSequences { get; }
    public int CompletionTokens { get; }
    public int ModelMaxTokens { get; }
    public bool RunSequentially { get; }
    public bool UpdateWaitingStatusOnStart { get; }
}
