using landerist_library.Export;

namespace landerist_library.Infrastructure.Distribution.Cloud;

public sealed class S3WebsiteArtifactStorage : IWebsiteArtifactStorage
{
    private readonly S3 _s3 = new();

    public bool Upload(string filePath, string fileName, string subdirectory) =>
        _s3.UploadToWebsiteBucket(filePath, fileName, subdirectory);

    public (DateTime? LastModified, long? ContentLength) GetFileInfo(
        string bucketName,
        string objectKey) =>
        _s3.GetFileInfo(bucketName, objectKey);

    public string? GetMetadata(
        string bucketName,
        string objectKey,
        string metadataKey) =>
        _s3.GetMetadataValue(bucketName, objectKey, metadataKey);
}
