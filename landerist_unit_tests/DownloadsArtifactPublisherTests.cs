using landerist_library.Infrastructure.Distribution;
using landerist_library.Websites;

namespace landerist_unit_tests;

public sealed class DownloadsArtifactPublisherTests
{
    private static readonly DateOnly Yesterday = new(2026, 8, 16);

    [Fact]
    public void UploadListings_PublishesCurrentAndDatedHistoricArtifact()
    {
        RecordingStorage storage = new();
        RecordingFileSystem files = new();
        DownloadsArtifactPublisher publisher = new(storage, files, () => Yesterday);

        bool result = publisher.UploadListings(
            "current.json",
            CountryCode.ES,
            ExportType.Published,
            42,
            null,
            null);

        Assert.True(result);
        Assert.Equal(2, storage.Uploads.Count);
        Assert.Equal("es-listings-published.json", storage.Uploads[0].FileName);
        Assert.Equal(
            "es-listings-published-2026-08-16.json",
            storage.Uploads[1].FileName);
        Assert.Equal("attachment; filename=\"es-listings-published.json\"", storage.Uploads[0].Disposition);
        Assert.Contains((DownloadsUpdater.METADATA_KEY_COUNTER, "42"), storage.Uploads[0].Metadata);
        Assert.Single(files.Copies);
        Assert.Single(files.Deletes);
    }

    [Fact]
    public void UploadListings_WhenCurrentUploadFails_DoesNotCreateHistoricArtifact()
    {
        RecordingStorage storage = new() { UploadResults = new Queue<bool>([false]) };
        RecordingFileSystem files = new();
        DownloadsArtifactPublisher publisher = new(storage, files, () => Yesterday);

        bool result = publisher.UploadListings(
            "current.json",
            CountryCode.ES,
            ExportType.Published,
            1,
            null,
            null);

        Assert.False(result);
        Assert.Single(storage.Uploads);
        Assert.Empty(files.Copies);
        Assert.Empty(files.Deletes);
    }

    [Fact]
    public void UploadListings_WhenHistoricUploadFails_StillDeletesTemporaryCopy()
    {
        RecordingStorage storage = new()
        {
            UploadResults = new Queue<bool>([true, false])
        };
        RecordingFileSystem files = new();
        DownloadsArtifactPublisher publisher = new(storage, files, () => Yesterday);

        bool result = publisher.UploadListings(
            "current.json",
            CountryCode.ES,
            ExportType.PublishedUpdates,
            3,
            new DateOnly(2026, 8, 10),
            new DateOnly(2026, 8, 15));

        Assert.False(result);
        Assert.Equal(
            "es-listings-published-updates-2026-08-10-to-2026-08-15.json",
            storage.Uploads[1].FileName);
        Assert.Single(files.Deletes);
    }

    [Fact]
    public void GetDateFrom_UsesOldestValidContinuationAcrossExports()
    {
        RecordingStorage storage = new();
        storage.MetadataByObjectKey[
            "ES/PublishedUpdates/es-listings-published-updates.json"] = "2026-08-12";
        storage.MetadataByObjectKey[
            "ES/UnpublishedUpdates/es-listings-unpublished-updates.json"] = "2026-08-09";
        DownloadsArtifactPublisher publisher = new(
            storage,
            new RecordingFileSystem(),
            () => Yesterday);

        DateOnly result = publisher.GetDateFrom(
            ExportType.PublishedUpdates,
            ExportType.UnpublishedUpdates);

        Assert.Equal(new DateOnly(2026, 8, 10), result);
    }

    private sealed class RecordingStorage : IDownloadsStorage
    {
        public Queue<bool> UploadResults { get; set; } = [];

        public List<UploadRecord> Uploads { get; } = [];

        public Dictionary<string, string> MetadataByObjectKey { get; } = [];

        public bool Upload(
            string filePath,
            string fileName,
            string subdirectory,
            IReadOnlyList<(string Key, string Value)> metadata,
            string contentDisposition)
        {
            Uploads.Add(new UploadRecord(
                filePath,
                fileName,
                subdirectory,
                metadata,
                contentDisposition));
            return UploadResults.Count == 0 || UploadResults.Dequeue();
        }

        public string? GetMetadata(string objectKey, string metadataKey) =>
            MetadataByObjectKey.GetValueOrDefault(objectKey);
    }

    private sealed class RecordingFileSystem : IDistributionFileSystem
    {
        private readonly HashSet<string> _existing = [];

        public List<(string Source, string Destination)> Copies { get; } = [];

        public List<string> Deletes { get; } = [];

        public void Copy(string sourcePath, string destinationPath, bool overwrite)
        {
            Copies.Add((sourcePath, destinationPath));
            _existing.Add(destinationPath);
        }

        public bool Exists(string path) => _existing.Contains(path);

        public void Delete(string path)
        {
            Deletes.Add(path);
            _existing.Remove(path);
        }
    }

    private sealed record UploadRecord(
        string FilePath,
        string FileName,
        string Subdirectory,
        IReadOnlyList<(string Key, string Value)> Metadata,
        string Disposition);
}
