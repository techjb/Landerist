using landerist_library.Infrastructure.Tasks;

namespace landerist_library.Infrastructure.Ai.Batch;

public sealed class VertexBatchArtifactCleaner(
    VertexBatchJobClient jobs,
    VertexCloudStorageClient storage,
    int retentionDays,
    TimeProvider timeProvider) : IBatchArtifactCleaner
{
    private readonly int _retentionDays = retentionDays > 0
        ? retentionDays
        : throw new ArgumentOutOfRangeException(nameof(retentionDays));

    public void Clean()
    {
        DateTime cutoff = timeProvider.GetLocalNow()
            .DateTime
            .AddDays(-_retentionDays);
        jobs.DeleteCompletedBefore(cutoff);
        storage.DeleteBefore(cutoff);
    }
}
