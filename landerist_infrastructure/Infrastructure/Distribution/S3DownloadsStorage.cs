using landerist_library.Configuration;
using landerist_library.Export;

namespace landerist_library.Infrastructure.Distribution;

internal sealed class S3DownloadsStorage : IDownloadsStorage
{
    private readonly S3 _s3 = new();

    public bool Upload(
        string filePath,
        string fileName,
        string subdirectory,
        IReadOnlyList<(string Key, string Value)> metadata,
        string contentDisposition) =>
        _s3.UploadToDownloadsBucket(
            filePath,
            fileName,
            subdirectory.Replace("\\", "/"),
            metadata.ToList(),
            contentDisposition);

    public string? GetMetadata(string objectKey, string metadataKey) =>
        _s3.GetMetadataValue(
            AppConfig.AWS_S3_DOWNLOADS_BUCKET,
            objectKey,
            metadataKey);
}
