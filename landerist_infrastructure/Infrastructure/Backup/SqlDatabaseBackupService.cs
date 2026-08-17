using landerist_library.Application.Logging;
using landerist_library.Application.Tasks;
using landerist_library.Database;
using landerist_library.Infrastructure.Runtime;
using landerist_library.Infrastructure.DatabaseMaintenance;
using System.Globalization;

namespace landerist_library.Infrastructure.Backup;

public sealed class SqlDatabaseBackupService : IDatabaseBackupService
{
    private readonly IDatabase _database;
    private readonly DatabaseBackupOptions _options;
    private readonly IBackupStorage _storage;
    private readonly IBackupFileSystem _files;
    private readonly TimeProvider _timeProvider;
    private readonly IApplicationLogger _logger;

    public SqlDatabaseBackupService(
        IDatabase database,
        DatabaseBackupOptions options,
        IApplicationLogger logger)
        : this(
            database,
            options,
            new S3BackupStorage(options.BucketName),
            new SystemBackupFileSystem(),
            TimeProvider.System,
            logger)
    {
    }

    internal SqlDatabaseBackupService(
        IDatabase database,
        DatabaseBackupOptions options,
        IBackupStorage storage,
        IBackupFileSystem files,
        TimeProvider timeProvider,
        IApplicationLogger logger)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(storage);
        ArgumentNullException.ThrowIfNull(files);
        ArgumentNullException.ThrowIfNull(timeProvider);
        ArgumentNullException.ThrowIfNull(logger);
        options.Validate();
        _database = database;
        _options = options;
        _storage = storage;
        _files = files;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public void Update()
    {
        CreateNewBackup();
        DeleteExpiredRemoteBackups();
        _files.DeleteAllFiles(_options.LocalDirectory);
    }

    private void CreateNewBackup()
    {
        string fileName = _options.DatabaseName +
            _timeProvider.GetLocalNow().ToString("yyyyMMdd", CultureInfo.InvariantCulture) +
            ".bak";
        string filePath = Path.Combine(_options.LocalDirectory, fileName);
        bool success = SaveBackup(filePath) && UploadBackup(fileName, filePath);
        _logger.WriteInfo("backup", fileName + " Success: " + success);
    }

    private bool SaveBackup(string filePath)
    {
        string escapedDatabaseName = _options.DatabaseName.Replace("]", "]]", StringComparison.Ordinal);
        string escapedFilePath = filePath.Replace("'", "''", StringComparison.Ordinal);
        string query =
            $"BACKUP DATABASE [{escapedDatabaseName}] TO " +
            $"DISK = N'{escapedFilePath}' WITH NOFORMAT, INIT, " +
            $"NAME = N'{escapedDatabaseName}-Full Database Backup', " +
            "SKIP, NOREWIND, NOUNLOAD, STATS = 10";

        _database.SetTimeout(60 * 10);
        return _database.Query(query);
    }

    private bool UploadBackup(string fileName, string filePath) =>
        _files.Exists(filePath) && _storage.Upload(filePath, fileName);

    private void DeleteExpiredRemoteBackups()
    {
        DateTime threshold = _timeProvider.GetLocalNow().DateTime
            .AddDays(-_options.RetentionDays);
        string[] expiredKeys = [.. _storage.List()
            .Where(item => item.LastModified < threshold)
            .Select(item => item.Key)];
        if (expiredKeys.Length == 0)
        {
            return;
        }

        int deleted = _storage.Delete(expiredKeys);
        _logger.WriteInfo("backup", "DeleteOldBackups Deleted: " + deleted);
    }
}
