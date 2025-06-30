using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Logs;
using landerist_library.Parse.ListingParser;
using landerist_library.Parse.ListingParser.OpenAI.Batch;
using landerist_library.Parse.ListingParser.VertexAI.Batch;
using landerist_library.Websites;

namespace landerist_library.Tasks
{
    public class TaskBatchUpload
    {
        private static readonly long MaxFileSizeInBytes = SetMaxFileSize();

        private static readonly int MaxPagesPerBatch = GetMaxPagesPerBatch();

        private static List<Page> pages = [];

        private static HashSet<string> UriHashes = [];

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
            if (Config.IsConfigurationLocal())
            {
                return Config.MAX_PAGES_PER_BATCH_LOCAL;
            }
            switch (Config.LLM_PROVIDER)
            {
                case LLMProvider.OpenAI:
                    return Config.MAX_PAGES_PER_BATCH_OPEN_AI;
                case LLMProvider.VertexAI:
                    return Config.MAX_PAGES_PER_BATCH_VERTEX_AI;
            }
            return 0;
        }

        public static void Start(bool processAll = true)
        {
            Clear();
            bool sucess = BatchUpload();
            if (processAll && sucess && UriHashes.Count < pages.Count)
            {
                Start(processAll);
            }
            Clear();
        }

        private static bool BatchUpload()
        {
            pages = Pages.SelectWaitingStatusAiRequest(MaxPagesPerBatch);

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

            Batches.Insert(batchId, UriHashes);
            SetWaitingStatusAIResponse();
            return true;
        }

        private static string? CreateFile()
        {
            var filePath = Config.BATCH_DIRECTORY + "batch_" +
                Config.LLM_PROVIDER.ToString().ToLower() + "_" +
                DateTime.Now.ToString("yyyyMMddHHmmss") + "_input.json";

            Console.WriteLine("TaskBatchUpload " + filePath);
            File.Delete(filePath);

            UriHashes = [];
            var errors = 0;
            var skipped = 0;

            using StreamWriter writer = new(filePath, append: true)
            {
                AutoFlush = true
            };

            Parallel.ForEach(pages, Config.PARALLELOPTIONS1INLOCAL, (page, state) =>
            {
                if (!CanWriteFile(writer))
                {
                    Interlocked.Increment(ref skipped);
                    state.Stop();
                }
                else if (WriteToFile(page, writer))
                {
                    lock (UriHashes)
                    {
                        UriHashes.Add(page.UriHash);
                    }
                }
                else
                {
                    Interlocked.Increment(ref errors);
                }
                page.Dispose();
            });


            Log.WriteInfo("batch", $"CreateFile {UriHashes.Count}/{pages.Count} errors: {errors}");
            return filePath;
        }

        private static bool CanWriteFile(StreamWriter writer)
        {
            lock (writer)
            {
                writer.Flush();
                return writer.BaseStream.Length < MaxFileSizeInBytes;
            }
        }

        private static bool WriteToFile(Page page, StreamWriter writer)
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
                lock (writer)
                {
                    writer.WriteLine(json);
                    writer.Flush();
                }
                return true;
            }
            catch (Exception exception)
            {
                Log.WriteError("TaskBatchUpload AddToBatch", exception.Message);
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
                Log.WriteError("TaskBatchUpload GetJson", "Error getting user input. Page: " + page.UriHash);
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
            //Console.WriteLine("Uploading batch file: " + filePath);
            return Config.LLM_PROVIDER switch
            {
                LLMProvider.OpenAI => OpenAIBatchClient.UploadFile(filePath),
                LLMProvider.VertexAI => CloudStorage.UploadFile(filePath),
                _ => null,
            };
        }

        private static string? CreateBatch(string fileId)
        {
            return Config.LLM_PROVIDER switch
            {
                LLMProvider.OpenAI => OpenAIBatchClient.CreateBatch(fileId),
                LLMProvider.VertexAI => BatchPredictions.CreateBatch(fileId),
                _ => null,
            };
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

            Log.WriteInfo("TaskBatchUpload", "SetWaitingStatusAIResponse: " + UriHashes.Count);
            Parallel.ForEach(UriHashes, Config.PARALLELOPTIONS1INLOCAL, Pages.UpdateWaitingStatusAiResponse);
        }
    }
}
