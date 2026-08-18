namespace landerist_library.Infrastructure.Distribution;

public interface IWebsiteArtifactStorage
{
    bool Upload(string filePath, string fileName, string subdirectory);

    (DateTime? LastModified, long? ContentLength) GetFileInfo(
        string bucketName,
        string objectKey);

    string? GetMetadata(string bucketName, string objectKey, string metadataKey);
}
