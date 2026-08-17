using landerist_library.Export;

namespace landerist_library.Infrastructure.DatabaseMaintenance;

internal sealed class S3BackupStorage(string bucketName) : IBackupStorage
{
    private readonly S3 _s3 = new();

    public bool Upload(string filePath, string fileName) =>
        _s3.UploadFile(filePath, fileName, bucketName);

    public IReadOnlyCollection<BackupObject> List() =>
        [.. _s3.ListObjects(bucketName)
            .GetAwaiter()
            .GetResult()
            .Select(item => new BackupObject(item.Key, item.LastModified.GetValueOrDefault()))];

    public int Delete(IReadOnlyCollection<string> objectKeys) =>
        _s3.DeleteObjects(bucketName, [.. objectKeys])
            .GetAwaiter()
            .GetResult()
            .Count;
}
