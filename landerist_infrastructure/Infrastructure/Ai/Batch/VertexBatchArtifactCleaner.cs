using landerist_library.Infrastructure.Tasks;

namespace landerist_library.Infrastructure.Ai.Batch;

public sealed class VertexBatchArtifactCleaner(
    VertexBatchJobClient jobs,
    VertexCloudStorageClient storage,
    int retentionDays,
    TimeProvider timeProvider) : IBatchArtifactCleaner
{
    public void Clean()
    {
        DateTime cutoff = timeProvider.GetLocalNow()
            .DateTime
            .AddDays(retentionDays);
        jobs.DeleteCompletedBefore(cutoff);
        storage.DeleteBefore(cutoff);
    }
}
