namespace landerist_library.Infrastructure.Tasks;

public sealed record BatchCleanupOptions(string? LocalDirectory);

public sealed class TaskBatchCleaner
{
    private readonly IBatchStore _batches;
    private readonly BatchCleanupOptions _options;
    private readonly IBatchArtifactCleaner _remoteArtifacts;

    public TaskBatchCleaner(
        IBatchStore batches,
        BatchCleanupOptions options,
        IBatchArtifactCleaner remoteArtifacts)
    {
        ArgumentNullException.ThrowIfNull(batches);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(remoteArtifacts);
        _batches = batches;
        _options = options;
        _remoteArtifacts = remoteArtifacts;
    }

    public void Start()
    {
        DeleteDownloadedBatches();
        DeleteLocalFiles();
        _remoteArtifacts.Clean();
    }

    private void DeleteDownloadedBatches()
    {
        foreach (BatchRecord batch in _batches.Select(downloaded: true))
        {
            _batches.Delete(batch.Id);
        }
    }

    private void DeleteLocalFiles()
    {
        if (string.IsNullOrWhiteSpace(_options.LocalDirectory) ||
            !Directory.Exists(_options.LocalDirectory))
        {
            return;
        }

        foreach (string file in Directory.GetFiles(_options.LocalDirectory))
        {
            File.Delete(file);
        }
    }
}
