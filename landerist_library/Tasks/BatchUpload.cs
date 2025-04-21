using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Logs;
using landerist_library.Parse.ListingParser;
using landerist_library.Parse.ListingParser.OpenAI.Batch;
using landerist_library.Parse.ListingParser.VertexAI.Batch;
using landerist_library.Websites;

namespace landerist_library.Tasks
{
    public class BatchUpload
    {
        private static readonly object SyncWrite = new();

        private static readonly long MaxFileSizeInBytes = SetMaxFileSize();

        private static readonly int MaxPagesPerBatch = GetMaxPagesPerBatch();

        private static List<Page> pages = [];

        private static List<string> UriHashes = [];

        private static long SetMaxFileSize()
        {
            switch (Config.LLM_PROVIDER)
            {
                case LLMProvider.OpenAI:
                    return Config.MAX_BATCH_FILE_SIZE_OPEN_AI * 1024 * 1024;
                case LLMProvider.VertexAI:
                    return Config.MAX_BATCH_FILE_SIZE_VERTEX_AI * 1024 * 1024;
            }
            return 0;
        }

        private static int GetMaxPagesPerBatch()
        {
            switch (Config.LLM_PROVIDER)
            {
                case LLMProvider.OpenAI:
                    return Config.MAX_PAGES_PER_BATCH_OPEN_AI;
                case LLMProvider.VertexAI:
                    return Config.MAX_PAGES_PER_BATCH_VERTEX_AI;
            }
            return 0;
        }

        public static void Start()
        {
            Clear();
            bool sucess = StartBatchUpload();
            if (pages.Count >= MaxPagesPerBatch && sucess)
            {
                Start();
            }
            Clear();
        }

        private static bool StartBatchUpload()
        {
            pages = Pages.SelectWaitingStatus(MaxPagesPerBatch, WaitingStatus.waiting_ai_request);

            if (pages.Count < Config.MIN_PAGES_PER_BATCH)
            {
                return false;
            }
            var filePath = CreateFile();
            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }
            var fileId = UploadFile(filePath);
            if (string.IsNullOrEmpty(fileId))
            {
                return false;
            }

            var batchId = CreateBatch(fileId);
            if (string.IsNullOrEmpty(batchId))
            {
                return false;
            }

            Batches.Insert(batchId);
            SetWaitingStatusAIResponse();
            Log.WriteInfo("batch", $"Uploaded {UriHashes.Count}");
            return true;
        }

        private static string? CreateFile()
        {
            var filePath = Config.BATCH_DIRECTORY + "batch_" +
                Config.LLM_PROVIDER.ToString().ToLower() + "_" +
                DateTime.Now.ToString("yyyyMMddHHmmss") + "_input.json";

            File.Delete(filePath);

            UriHashes = [];
            var sync = new object();
            var errors = 0;
            var skipped = 0;

            Parallel.ForEach(pages, Config.PARALLELOPTIONS1INLOCAL, (page, state) =>
            {
                if (!CanWriteFile(filePath))
                {
                    Interlocked.Increment(ref skipped);
                    state.Stop();
                }
                else if (!WriteToFile(page, filePath))
                {
                    Interlocked.Increment(ref errors);
                }
                else
                {
                    lock (sync)
                    {
                        UriHashes.Add(page.UriHash);
                    }
                }
                page.Dispose();
            });

            if (errors > 0)
            {
                Log.WriteError("BatchUpload CreateFile", "Error creating file. Errors: " + errors);
            }
            return filePath;
        }

        private static bool CanWriteFile(string filePath)
        {
            lock (SyncWrite)
            {
                if (!File.Exists(filePath))
                {
                    return true;
                }

                FileInfo fileInfo = new(filePath);
                return fileInfo.Length < MaxFileSizeInBytes;
            }
        }

        private static bool WriteToFile(Page page, string filePath)
        {
            try
            {
                var json = GetJson(page);
                if (string.IsNullOrEmpty(json))
                {
                    page.RemoveWaitingStatus();
                    page.Update(false);
                    return false;
                }
                lock (SyncWrite)
                {
                    File.AppendAllText(filePath, json + Environment.NewLine);
                }
                return true;
            }
            catch (Exception exception)
            {
                Log.WriteError("BatchUpload AddToBatch", exception.Message);
            }
            return false;
        }


        private static string? GetJson(Page page)
        {
            page.SetResponseBodyFromZipped();
            var userInput = ParseListingUserInput.GetText(page);
            page.RemoveResponseBody();

            if (string.IsNullOrEmpty(userInput))
            {
                Log.WriteError("BatchUpload GetJson", "Error getting user input. Page: " + page.UriHash);
                return null;
            }

            switch (Config.LLM_PROVIDER)
            {
                case LLMProvider.OpenAI:
                    return OpenAIBatchUpload.GetJson(page, userInput);
                case LLMProvider.VertexAI:
                    return VertexAIBatchUpload.GetJson(page, userInput);
                default:
                    return null;
            }
        }

        private static string? UploadFile(string filePath)
        {
            switch (Config.LLM_PROVIDER)
            {
                case LLMProvider.OpenAI:
                    return OpenAIBatchClient.UploadFile(filePath);
                case LLMProvider.VertexAI:
                    return CloudStorage.UploadFile(filePath);
                default:
                    return null;
            }
        }

        private static string? CreateBatch(string fileId)
        {
            switch (Config.LLM_PROVIDER)
            {
                case LLMProvider.OpenAI:
                    return OpenAIBatchClient.CreateBatch(fileId);
                case LLMProvider.VertexAI:
                    return BatchPredictions.CreateBatch(fileId);
                default:
                    return null;
            }
        }

        private static void Clear()
        {
            Parallel.ForEach(pages, page => page.Dispose());
            pages.Clear();
            UriHashes.Clear();
        }


        static void SetWaitingStatusAIResponse()
        {
            if (Config.IsConfigurationLocal())
            {
                return;
            }
            Parallel.ForEach(UriHashes, Config.PARALLELOPTIONS1INLOCAL, uriHash =>
            {
                Pages.UpdateWaitingStatus(uriHash, WaitingStatus.waiting_ai_response);
            });
        }
    }
}
