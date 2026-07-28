using landerist_library.Infrastructure.Tasks;
using landerist_library.Parse.ListingParser.VertexAI.Batch;

namespace landerist_library.Infrastructure.Tasks;

public sealed class LegacyVertexAiBatchArtifactCleaner : IBatchArtifactCleaner
{
    public void Clean() => VertexAIBatchCleaner.RemoveFiles();
}
