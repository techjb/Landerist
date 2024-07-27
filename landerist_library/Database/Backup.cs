using Amazon.S3.Model;
using landerist_library.Configuration;
using landerist_library.Export;
using landerist_library.Logs;

namespace landerist_library.Database
{
    public class Backup
    {
        public static void Update()
        {
            CreateNewBackup();
            DeleteRemoteOldBackups();
            DeleteLocalFiles();
        }

        private static void CreateNewBackup()
        {
            bool sucess = false;
            string fileName = Config.DATABASE_NAME + DateTime.Now.ToString("yyyyMMdd") + ".bak";
            string filePath = LocalBakAbsolutePath(fileName);

            if (SaveBackup(filePath))
            {
                sucess = UploadBackup(fileName, filePath);
            }
            Log.WriteLogInfo("backup", fileName + " Sucess: " + sucess.ToString());
        }

        private static bool SaveBackup(string filePath)
        {
            Console.WriteLine("Creating backup " + filePath + " ..");

            string query =
                "BACKUP DATABASE [" + Config.DATABASE_NAME + "] TO  " +
                "DISK = N'" + filePath + "' WITH NOFORMAT, INIT, " +
                "NAME = N'" + Config.DATABASE_NAME + "-Full Database Backup', SKIP, NOREWIND, NOUNLOAD,  STATS = 10";

            var dataBase = new DataBase();
            dataBase.SetTimeout(60 * 10); // 10 minutos
            return dataBase.Query(query);
        }

        private static bool UploadBackup(string fileName, string filePath)
        {
            Console.WriteLine("Uploading backup " + fileName + "..");
            if (!File.Exists(filePath))
            {
                return false;
            }
            return new S3().UploadFile(filePath, fileName, PrivateConfig.AWS_S3_BACKUPS_BUCKET);
        }

        public static void DeleteRemoteOldBackups()
        {
            Console.WriteLine("Deletings old backups ..");
            var S3Objects = new S3().ListObjects(PrivateConfig.AWS_S3_BACKUPS_BUCKET).Result;
            List<string> toDelete = [];
            DateTime dateToDelete = DateTime.Now.AddDays(-Config.DAYS_TO_DELETE_BACKUP);
            foreach (var S3Object in S3Objects)
            {
                if (S3Object.LastModified < dateToDelete)
                {
                    toDelete.Add(S3Object.Key);
                }
            }
            if (toDelete.Count.Equals(0))
            {
                return;
            }
            var deletedObjects = new S3().DeleteObjects(PrivateConfig.AWS_S3_BACKUPS_BUCKET, toDelete).Result;
            Log.WriteLogInfo("backup", "DeleteOldBackups Deleted: " + deletedObjects.Count);
        }

        protected static string LocalBakAbsolutePath(string fileName)
        {
            return Config.BACKUPS_DIRECTORY + fileName;
        }

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
}
