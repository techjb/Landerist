using Amazon.Auth.AccessControlPolicy.ActionIdentifiers;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using landerist_library.Configuration;

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
            catch (Exception e)
            {
                Logs.Log.WriteError("CloudStorage UploadFile", e);
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
            catch (Exception e)
            {
                Logs.Log.WriteError("CloudStorage DownloadFile", e);
                return false;
            }
        }

        private static StorageClient GetStorageClient()
        {
            var credentials = GoogleCredential.FromJson(PrivateConfig.GOOGLE_CLOUD_VERTEX_AI_CREDENTIAL);
            return StorageClient.Create(credentials);
        }
    }
}
