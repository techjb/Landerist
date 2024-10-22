using landerist_library.Database;
using OpenAI.Batch;
using OpenAI;
using landerist_library.Configuration;
using System.Text.Json;


namespace landerist_library.Parse.Listing.OpenAI
{
    public class OpenAIBatchDownload
    {
        private static BatchResponse? BatchResponse;

        private static readonly OpenAIClient OpenAIClient = new(PrivateConfig.OPENAI_API_KEY);

        private static string? DownloadedFilePath;

        public static void Start()
        {
            var batchIds = PendingBatches.Select();
            foreach (var batchId in batchIds)
            {
                BatchResponse = OpenAIClient.BatchEndpoint.RetrieveBatchAsync(batchId).Result;
                if (BatchResponse.Status.Equals(BatchStatus.Completed))
                {
                    if (RetrieveBatchResponse())
                    {
                        if (ReadDownloadedFile())
                        {
                            DeleteFiles();
                            DeletePendingBatches(batchId);
                        }
                    }
                }
            }
        }

        public static bool RetrieveBatchResponse()
        {
            if (BatchResponse == null)
            {
                return false;
            }

            DownloadedFilePath = OpenAIClient.FilesEndpoint.DownloadFileAsync(BatchResponse.OutputFileId, Config.BATCH_DIRECTORY).Result;
            if (string.IsNullOrEmpty(DownloadedFilePath))
            {
                return false;
            }
            return true;
        }

        public static bool ReadDownloadedFile()
        {
            if (string.IsNullOrEmpty(DownloadedFilePath))
            {
                return false;
            }


            try
            {
                using var reader = new StreamReader(DownloadedFilePath);
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    ReadDownloadFileLine(line);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file: {ex.Message}");
                return false;
            }            

            return true;
        }

        public static void ReadDownloadFileLine(string line)
        {
            var batchResponse = JsonSerializer.Deserialize<BatchResponseJson?>(line);
            if (batchResponse == null)
            {
                return;
            }

            Console.WriteLine($"Id: {batchResponse.Id}");
            Console.WriteLine($"CustomId: {batchResponse.CustomId}");
            Console.WriteLine($"Response StatusCode: {batchResponse.Response.StatusCode}");
        }

        public static void DeleteFiles()
        {
            if (BatchResponse == null)
            {
                return;
            }
            OpenAIClient.FilesEndpoint.DeleteFileAsync(BatchResponse.OutputFileId).Wait();
            OpenAIClient.FilesEndpoint.DeleteFileAsync(BatchResponse.InputFileId).Wait();

            File.Delete(DownloadedFilePath!);
        }

        public static bool DeletePendingBatches(string batchId)
        {
            return PendingBatches.Delete(batchId);
        }   
    }
}
