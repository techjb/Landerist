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

        const long MAX_FILE_SIZE_IN_BYTES = Config.MAX_BATCH_FILE_SIZE_MB * 1024 * 1024;

        private static List<Page> pages = [];

        private static List<string> UriHashes = [];


        public static void Start()
        {
            Clear();
            bool sucess = StartBatchUpload();
            if (pages.Count >= Config.MAX_PAGES_PER_BATCH && sucess)
            {
                Start();
            }
            Clear();
        }

        private static bool StartBatchUpload()
        {
            pages = Pages.SelectWaitingAIParsing();
            if (pages.Count < Config.MIN_PAGES_PER_BATCH)
            {
                return false;
            }
            var filePath = CreateFile();
            return true;
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

            SetWaitingAIResponse();
            if (!Batches.Insert(batchId))
            {
                Log.WriteError("BatchUpload StartUpload", "Error inserting batch. BatchId: " + batchId);
            }
            Log.WriteInfo("batch", $"Uploaded {pages.Count}");
            return true;
        }


        private static string? CreateFile()
        {
            var filePath = Config.BATCH_DIRECTORY + "batch_" +
                Config.LLM_PROVIDER.ToString().ToLower() + "_" +
                DateTime.Now.ToString("yyyyMMddHHmmss") + "_input.json";

            if (Config.IsConfigurationLocal())
            {
                filePath = Config.BATCH_DIRECTORY + "test.json";
            }

            File.Delete(filePath);

            UriHashes = [];
            var sync = new object();
            var errors = 0;
            var skipped = 0;

            Parallel.ForEach(pages,
                Config.PARALLELOPTIONS1INLOCAL,
                (page, state) =>
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
                if (Config.IsConfigurationLocal() && UriHashes.Count > 2)
                {
                    state.Stop();
                }
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
                return fileInfo.Length < MAX_FILE_SIZE_IN_BYTES;
            }
        }

        private static bool WriteToFile(Page page, string filePath)
        {
            try
            {
                var json = GetJson(page);
                if (string.IsNullOrEmpty(json))
                {
                    page.RemoveWaitingAIParsing();
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
                    return OpenAIBatchUpload.UploadFile(filePath);
                case LLMProvider.VertexAI:
                    //return VertexAIBatchClient.UploadFile(filePath);
                    return null;
                default:
                    return null;
            }
        }

        private static string? CreateBatch(string fileId)
        {
            switch (Config.LLM_PROVIDER)
            {
                case LLMProvider.OpenAI:
                    return OpenAIBatchUpload.CreateBatch(fileId);
                case LLMProvider.VertexAI:
                    //return VertexAIBatchClient.CreateBatch(fileId);
                    return null;
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


        static void SetWaitingAIResponse()
        {
            if (Config.IsConfigurationLocal())
            {
                return;
            }
            Parallel.ForEach(UriHashes,
                //new ParallelOptions(){MaxDegreeOfParallelism = 1},
                uriHash =>
            {
                Pages.UpdateWaitingAIParsing(uriHash, false);
            });
        }
    }
}
