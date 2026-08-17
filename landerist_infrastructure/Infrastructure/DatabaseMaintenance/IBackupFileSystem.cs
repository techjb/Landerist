namespace landerist_library.Infrastructure.DatabaseMaintenance;

internal interface IBackupFileSystem
{
    bool Exists(string filePath);
    void DeleteAllFiles(string directory);
}

internal sealed class SystemBackupFileSystem : IBackupFileSystem
{
    public bool Exists(string filePath) => File.Exists(filePath);

    public void DeleteAllFiles(string directory)
    {
        foreach (string filePath in Directory.GetFiles(directory))
        {
            File.Delete(filePath);
        }
    }
}
