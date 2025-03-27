using landerist_library.Configuration;
using landerist_library.Logs;
using OpenAI;
using OpenAI.Batch;
using OpenAI.Files;

namespace landerist_library.Parse.ListingParser.OpenAI.Batch
{
    public class OpenAIBatchClient
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
                Log.WriteError("OpenAIBatchClient DeleteFile", exception);
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
                Log.WriteError("OpenAIBatchClient ExistsFile", exception);
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
                Log.WriteError("OpenAIBatchClient GetBatch", exception);
            }
            return null;
        }

        public static bool BatchIsCompleted(string bacthId)
        {
            var batch = GetBatch(bacthId);
            if (batch != null)
            {
                return BatchIsCompleted(batch);
            }
            return false;
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
                Log.WriteError("OpenAIBatchClient DownloadFile", exception);
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
                Log.WriteError("OpenAIBatchClient ListFiles", exception);
            }
            return null;
        }

        public static string? UploadFile(string filePath)
        {
            try
            {
                return OpenAIClient.FilesEndpoint.UploadFileAsync(filePath, FilePurpose.Batch).GetAwaiter().GetResult().Id;
            }
            catch (Exception exception)
            {
                Log.WriteError("OpenAIBatchClient UploadFile", exception);
            }
            return null;
        }

        public static string? CreateBatch(string id)
        {
            try
            {
                var batchRequest = new CreateBatchRequest(id, Endpoint.ChatCompletions);
                return OpenAIClient.BatchEndpoint.CreateBatchAsync(batchRequest).Result.Id;
            }
            catch (Exception exception)
            {
                Log.WriteError("OpenAIBatchClient CreateBatch", exception);
            }
            return null;
        }
    }
}
