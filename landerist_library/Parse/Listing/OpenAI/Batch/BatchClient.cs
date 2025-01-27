using landerist_library.Configuration;
using landerist_library.Logs;
using OpenAI;
using OpenAI.Batch;
using OpenAI.Files;

namespace landerist_library.Parse.Listing.OpenAI.Batch
{
    public class BatchClient
    {
        private static readonly OpenAIClient OpenAIClient = new(PrivateConfig.OPENAI_API_KEY);

        public static bool DeleteFile(string fileId)
        {
            if (string.IsNullOrEmpty(fileId))
            {
                return true;
            }
            if (!ExistsFile(fileId))
            {
                return true;
            }
            try
            {
                OpenAIClient.FilesEndpoint.DeleteFileAsync(fileId);
                return true;
            }
            catch (Exception exception)
            {
                Log.WriteError("BatchClient DeleteFile", exception);
            }
            return false;
        }

        protected static bool ExistsFile(string fileId)
        {
            try
            {
                FileResponse fileResponse = OpenAIClient.FilesEndpoint.GetFileInfoAsync(fileId).Result;
                return true;
            }
            catch (Exception exception)
            {
                Log.WriteError("BatchClient ExistsFile", exception);
            }
            return false;
        }

        protected static BatchResponse? GetBatch(string batchId)
        {
            try
            {
                return OpenAIClient.BatchEndpoint.RetrieveBatchAsync(batchId).Result;

            }
            catch (Exception exception)
            {
                Log.WriteError("BatchClient GetBatch", exception);
            }
            return null;
        }

        protected static bool BatchIsCompleted(BatchResponse batchResponse)
        {
            return batchResponse.Status.Equals(BatchStatus.Completed);
        }

        protected static string? DownloadFile(string? fileId)
        {
            if (string.IsNullOrEmpty(fileId))
            {
                return null;
            }
            try
            {
                return OpenAIClient.FilesEndpoint.DownloadFileAsync(fileId, Config.BATCH_DIRECTORY).Result;
            }
            catch (Exception exception)
            {
                Log.WriteError("BatchClient DownloadFile", exception);
            }
            return null;
        }

        protected static List<FileResponse>? ListFiles()
        {
            try
            {
                return [.. OpenAIClient.FilesEndpoint.ListFilesAsync().Result];
            }
            catch (Exception exception)
            {
                Log.WriteError("BatchClient ListFiles", exception);
            }
            return null;
        }

        protected static FileResponse? UploadFile(string filePath)
        {
            try
            {
                return OpenAIClient.FilesEndpoint.UploadFileAsync(filePath, FilePurpose.Batch).GetAwaiter().GetResult();
            }
            catch (Exception exception)
            {
                Log.WriteError("BatchClient UploadFile", exception);
            }
            return null;
        }

        protected static BatchResponse? CreateBatch(FileResponse fileResponse)
        {
            try
            {
                var batchRequest = new CreateBatchRequest(fileResponse.Id, Endpoint.ChatCompletions);
                return OpenAIClient.BatchEndpoint.CreateBatchAsync(batchRequest).Result;
            }
            catch (Exception exception)
            {
                Log.WriteError("BatchClient CreateBatch", exception);
            }
            return null;
        }
    }
}
