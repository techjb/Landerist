using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using landerist_library.Configuration;
using landerist_library.Logs;

namespace landerist_library.Parse.ListingParser.VertexAI.Batch
{
    public class CloudStorage
    {
        public static string? UploadFile(string filePath)
        {
            string objectName = "input/" + Path.GetFileName(filePath);
            var storageClient = GetStorageClient();
            try
            {
                using FileStream fileStream = File.OpenRead(filePath);
                var dataObject = storageClient.UploadObject(PrivateConfig.GOOGLE_CLOUD_BUCKET_NAME, objectName, "text/html", fileStream);
                return dataObject.Name;
            }
            catch (Exception exception)
            {
                Log.WriteError("CloudStorage UploadFile", exception);
                return null;
            }
        }

        public static bool DownloadFile(string objectName, string localPath)
        {
            var storageClient = GetStorageClient();
            try
            {
                using var fileStream = File.OpenWrite(localPath);
                var dataObject = storageClient.DownloadObject(PrivateConfig.GOOGLE_CLOUD_BUCKET_NAME, objectName, fileStream);
                return true;
            }
            catch (Exception exception)
            {
                Log.WriteError("CloudStorage DownloadFile", exception);
                return false;
            }
        }

        public static void DeleteFiles(DateTime dateTime)
        {
            var storageClient = GetStorageClient();
            var objects = storageClient.ListObjects(PrivateConfig.GOOGLE_CLOUD_BUCKET_NAME);
            int total = objects.Count();
            int deleted = 0;
            int toDelete = 0;
            int errors = 0;             
            foreach(var obj in objects)
            {
                if (obj.TimeCreatedDateTimeOffset < dateTime)
                {
                    toDelete++;
                    try
                    {
                        storageClient.DeleteObject(PrivateConfig.GOOGLE_CLOUD_BUCKET_NAME, obj.Name);
                        deleted++;
                    }
                    catch
                    {
                        errors++;                        
                    }                    
                }
            };
            Log.WriteInfo("CloudStorage DeleteFiles", "Objects: " + total + " ToDelete: " + toDelete + " Deleted: " + deleted + " Errors: " + errors);
        }

        private static StorageClient GetStorageClient()
        {
            var credentials = GoogleCredential.FromJson(PrivateConfig.GOOGLE_CLOUD_VERTEX_AI_CREDENTIAL);
            return StorageClient.Create(credentials);
        }
    }
}
