using landerist_library.Application.Tasks;
using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Export;
using landerist_library.Logs;

namespace landerist_library.Infrastructure.Backup;

public sealed class SqlDatabaseBackupService : IDatabaseBackupService
{
    private readonly IDatabase _database;

    public SqlDatabaseBackupService(IDatabase database)
    {
        ArgumentNullException.ThrowIfNull(database);
        _database = database;
    }

    public void Update()
    {
        CreateNewBackup();
        DeleteRemoteOldBackups();
        DeleteLocalFiles();
    }

    private void CreateNewBackup()
    {
        bool success = false;
        string fileName = Config.DATABASE_NAME + DateTime.Now.ToString("yyyyMMdd") + ".bak";
        string filePath = LocalBakAbsolutePath(fileName);

        if (SaveBackup(filePath))
        {
            success = UploadBackup(fileName, filePath);
        }
        Log.WriteInfo("backup", fileName + " Success: " + success);
    }

    private bool SaveBackup(string filePath)
    {
        Console.WriteLine("Creating backup " + filePath + " ..");

        string query =
            "BACKUP DATABASE [" + Config.DATABASE_NAME + "] TO " +
            "DISK = N'" + filePath + "' WITH NOFORMAT, INIT, " +
            "NAME = N'" + Config.DATABASE_NAME + "-Full Database Backup', SKIP, NOREWIND, NOUNLOAD, STATS = 10";

        _database.SetTimeout(60 * 10);
        return _database.Query(query);
    }

    private static bool UploadBackup(string fileName, string filePath)
    {
        Console.WriteLine("Uploading backup " + fileName + "..");
        if (!File.Exists(filePath))
        {
            return false;
        }
        return new S3().UploadFile(filePath, fileName, AppConfig.AWS_S3_BACKUPS_BUCKET);
    }

    private static void DeleteRemoteOldBackups()
    {
        Console.WriteLine("Deleting old backups ..");
        var s3Objects = new S3().ListObjects(AppConfig.AWS_S3_BACKUPS_BUCKET).Result;
        List<string> toDelete = [];
        DateTime dateToDelete = DateTime.Now.AddDays(-Config.DAYS_TO_DELETE_BACKUP);
        foreach (var s3Object in s3Objects)
        {
            if (s3Object.LastModified < dateToDelete)
            {
                toDelete.Add(s3Object.Key);
            }
        }
        if (toDelete.Count == 0)
        {
            return;
        }
        var deletedObjects = new S3().DeleteObjects(AppConfig.AWS_S3_BACKUPS_BUCKET, toDelete).Result;
        Log.WriteInfo("backup", "DeleteOldBackups Deleted: " + deletedObjects.Count);
    }

    private static string LocalBakAbsolutePath(string fileName) =>
        Config.BACKUPS_DIRECTORY + fileName;

    private static void DeleteLocalFiles()
    {
        if (Config.BACKUPS_DIRECTORY is null)
        {
            return;
        }

        DirectoryInfo directoryInfo = new(Config.BACKUPS_DIRECTORY);
        foreach (FileInfo fileInfo in directoryInfo.GetFiles())
        {
            fileInfo.Delete();
        }
    }
}