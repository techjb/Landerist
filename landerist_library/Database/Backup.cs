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
            DeleteOldBackups();
        }

        private static void CreateNewBackup()
        {
            bool sucess = false;
            string fileName = Config.DATABASE_NAME + DateTime.Now.ToString("yyyyMMdd") + ".bak";
            if (SaveBackup(fileName))
            {
                sucess = UploadBackup(fileName);
            }
            Log.WriteLogInfo("backup", fileName + " Sucess: " + sucess.ToString());
        }

        private static bool SaveBackup(string fileName)
        {
            Console.WriteLine("Creating backup " + fileName + " ..");

            string filePath = LocalBakAbsolutePath(fileName);

            string query =
                "BACKUP DATABASE [" + Config.DATABASE_NAME + "] TO  " +
                "DISK = N'" + filePath + "' WITH NOFORMAT, INIT, " +
                "NAME = N'" + Config.DATABASE_NAME + "-Full Database Backup', SKIP, NOREWIND, NOUNLOAD,  STATS = 10";

            var dataBase = new DataBase();
            dataBase.SetTimeout(60 * 10); // 10 minutos
            return dataBase.Query(query);
        }

        private static bool UploadBackup(string fileName)
        {
            Console.WriteLine("Uploading backup " + fileName + "..");

            string filePath = LocalBakAbsolutePath(fileName);
            string keyName = fileName;

            if (!File.Exists(filePath))
            {
                return false;
            }
            return new S3().UploadFile(filePath, keyName, PrivateConfig.AWS_S3_BACKUPS_BUCKET);
        }

        private static async void DeleteOldBackups()
        {
            Console.WriteLine("Deletings old backups ..");
            var S3Objects = await new S3().ListObjects(PrivateConfig.AWS_S3_BACKUPS_BUCKET);
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
            var deletedObjects = await new S3().DeleteObjects(PrivateConfig.AWS_S3_BACKUPS_BUCKET, toDelete);
            Log.WriteLogInfo("backup", "DeleteOldBackups Deleted: " + deletedObjects.Count);
        }

        protected static string LocalBakAbsolutePath(string fileName)
        {
            return Config.BACKUPS_DIRECTORY + fileName;
        }
    }
}
