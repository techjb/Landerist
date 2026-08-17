namespace landerist_library.Infrastructure.DatabaseMaintenance;

internal sealed record BackupObject(string Key, DateTime LastModified);

internal interface IBackupStorage
{
    bool Upload(string filePath, string fileName);
    IReadOnlyCollection<BackupObject> List();
    int Delete(IReadOnlyCollection<string> objectKeys);
}
