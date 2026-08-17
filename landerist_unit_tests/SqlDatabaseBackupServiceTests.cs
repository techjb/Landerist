using landerist_library.Application.Logging;
using landerist_library.Infrastructure.Backup;
using landerist_library.Infrastructure.DatabaseMaintenance;
using landerist_library.Infrastructure.Runtime;

namespace landerist_unit_tests;

public sealed class SqlDatabaseBackupServiceTests
{
    private static readonly DatabaseBackupOptions Options = new(
        "Landerist",
        "backups",
        "backup-bucket",
        RetentionDays: 60);

    [Fact]
    public void Update_UploadsBackupDeletesExpiredObjectsAndCleansLocalFiles()
    {
        RecordingDatabase database = new() { QueryResult = true };
        RecordingBackupStorage storage = new();
        storage.Objects.Add(new("expired.bak", new DateTime(2026, 6, 1)));
        storage.Objects.Add(new("current.bak", new DateTime(2026, 7, 1)));
        RecordingBackupFileSystem files = new() { FileExists = true };
        SqlDatabaseBackupService service = CreateService(database, storage, files);

        service.Update();

        Assert.Equal(600, database.TimeoutSeconds);
        Assert.Contains("BACKUP DATABASE [Landerist]", database.LastQuery);
        Assert.Contains(Path.Combine("backups", "Landerist20260817.bak"), database.LastQuery);
        Assert.Equal(
            (Path.Combine("backups", "Landerist20260817.bak"), "Landerist20260817.bak"),
            storage.Uploaded);
        Assert.Equal(["expired.bak"], storage.DeletedKeys);
        Assert.Equal("backups", files.DeletedDirectory);
    }

    [Fact]
    public void Update_WhenDatabaseBackupFails_DoesNotUploadButStillCleans()
    {
        RecordingDatabase database = new() { QueryResult = false };
        RecordingBackupStorage storage = new();
        RecordingBackupFileSystem files = new() { FileExists = true };
        SqlDatabaseBackupService service = CreateService(database, storage, files);

        service.Update();

        Assert.Null(storage.Uploaded);
        Assert.Equal("backups", files.DeletedDirectory);
    }

    private static SqlDatabaseBackupService CreateService(
        RecordingDatabase database,
        RecordingBackupStorage storage,
        RecordingBackupFileSystem files) =>
        new(
            database,
            Options,
            storage,
            files,
            new FixedTimeProvider(new DateTimeOffset(2026, 8, 17, 12, 0, 0, TimeSpan.Zero)),
            new NullLogger());

    private sealed class RecordingBackupStorage : IBackupStorage
    {
        public List<BackupObject> Objects { get; } = [];
        public (string Path, string Name)? Uploaded { get; private set; }
        public IReadOnlyCollection<string> DeletedKeys { get; private set; } = [];

        public bool Upload(string filePath, string fileName)
        {
            Uploaded = (filePath, fileName);
            return true;
        }

        public IReadOnlyCollection<BackupObject> List() => Objects;

        public int Delete(IReadOnlyCollection<string> objectKeys)
        {
            DeletedKeys = objectKeys;
            return objectKeys.Count;
        }
    }

    private sealed class RecordingBackupFileSystem : IBackupFileSystem
    {
        public bool FileExists { get; init; }
        public string? DeletedDirectory { get; private set; }
        public bool Exists(string filePath) => FileExists;
        public void DeleteAllFiles(string directory) => DeletedDirectory = directory;
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
    }

    private sealed class NullLogger : IApplicationLogger
    {
        public void WriteError(string source, string message) { }
        public void WriteInfo(string source, string message) { }
    }
}
