namespace landerist_library.Infrastructure.Distribution;

internal interface IDownloadsStorage
{
    bool Upload(
        string filePath,
        string fileName,
        string subdirectory,
        IReadOnlyList<(string Key, string Value)> metadata,
        string contentDisposition);

    string? GetMetadata(string objectKey, string metadataKey);
}
