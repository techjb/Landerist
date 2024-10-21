using landerist_library.Configuration;
using landerist_library.Websites;
using landerist_library.Logs;
using System.Text.Json;
using OpenAI.Files;
using OpenAI;
using OpenAI.Batch;
using System.Threading.Tasks;
using landerist_library.Database;

namespace landerist_library.Parse.Listing.OpenAI
{
    public class OpenAIBatch
    {
        private static List<Page> Pages = [];

        private static readonly object Sync = new();

        private static string FilePath = string.Empty;

        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            WriteIndented = false
        };

        private static FileResponse? FileResponse;

        private static readonly OpenAIClient OpenAIClient = new(PrivateConfig.OPENAI_API_KEY);

        private static BatchResponse? BatchResponse;

        public static void Start()
        {
            Pages = Websites.Pages.SelectWaitingAIParsing();
            FilterMaxPagesPerBatch();
            if (CreateFile())
            {
                if (UploadFile())
                {
                    if (CreateBatch())
                    {
                        InsertBatchId();
                        //SetWaitingAIResponse();
                    }
                }
            }

        }

        private static void FilterMaxPagesPerBatch()
        {
            if (Pages.Count > Config.MAX_PAGES_PER_BATCH)
            {
                Pages = Pages.Take(Config.MAX_PAGES_PER_BATCH).ToList();
            }
        }

        private static bool CreateFile()
        {
            if (Pages.Count < Config.MIN_PAGES_PER_BATCH)
            {
                return false;
            }
            InitFilePath();
            var totalPages = Pages.Count;
            var counter = 0;
            var errors = 0;
            Parallel.ForEach(Pages, new ParallelOptions()
            {
                //MaxDegreeOfParallelism = Config.MAX_DEGREE_OF_PARALLELISM
                MaxDegreeOfParallelism = 1
            }, page =>
            {
                if (AddToBatch(page))
                {
                    Interlocked.Increment(ref counter);
                }
                else
                {
                    Interlocked.Increment(ref errors);
                }
            });
            Log.WriteLogInfo("OpenAIBatch", $"Total pages: {totalPages}, Added to batch: {counter}, Errors: {errors}");
            return errors.Equals(0);
        }

        private static void InitFilePath()
        {
            FilePath = Config.BATCH_DIRECTORY +
                "openai_batch_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".json";

            File.Delete(FilePath);
        }

        private static bool AddToBatch(Page page)
        {
            try
            {
                page.SetResponseBodyUnzipped();
                var json = GetBatchJson(page);
                return WriteJson(json);
            }
            catch (Exception exception)
            {
                Log.WriteLogErrors("OpenAIBatch AddToBatch", exception.Message);
            }
            return false;
        }

        private static string? GetBatchJson(Page page)
        {
            var userInput = UserTextInput.GetText(page);
            if (string.IsNullOrEmpty(userInput))
            {
                return null;
            }

            var requestData = new RequestData
            {
                custom_id = page.UriHash,
                method = "POST",
                url = "/v1/chat/completions",
                body = new Body
                {
                    model = OpenAIRequest.MODEL_NAME,
                    messages =
                [
                    new BatchMessage
                    {
                        role = "system",
                        content = OpenAIRequest.GetSystemPrompt()
                    },
                    new BatchMessage {
                        role = "user",
                        content = userInput
                    }
                ],
                }
            };

            return JsonSerializer.Serialize(requestData, JsonSerializerOptions);
        }

        private static bool WriteJson(string? json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return false;
            }
            lock (Sync)
            {
                File.AppendAllText(FilePath, json + Environment.NewLine);
            }
            return true;
        }


        static bool UploadFile()
        {
            bool sucess = false;
            try
            {

                FileResponse = OpenAIClient.FilesEndpoint.UploadFileAsync(FilePath, FilePurpose.Batch).GetAwaiter().GetResult();
                sucess = true;
            }
            catch (Exception exception)
            {
                Log.WriteLogErrors("OpenAIBatch UploadFile", exception.Message);

            }
            File.Delete(FilePath);
            return sucess;
        }

        static bool CreateBatch()
        {
            if (FileResponse == null)
            {
                return false;
            }

            var batchRequest = new CreateBatchRequest(FileResponse.Id, Endpoint.ChatCompletions);
            BatchResponse = OpenAIClient.BatchEndpoint.CreateBatchAsync(batchRequest).Result;
            return true;
        }

        static bool InsertBatchId()
        {
            if (BatchResponse == null)
            {
                return false;
            }

            return PendingBatches.Insert(BatchResponse.Id);
        }

        static void SetWaitingAIResponse()
        {
            Parallel.ForEach(Pages,
                new ParallelOptions()
                {
                    MaxDegreeOfParallelism = Config.MAX_DEGREE_OF_PARALLELISM
                },
                page =>
            {
                page.SetWaitingAIResponse();
            });
        }

        public static void End()
        {
            var batchIds = PendingBatches.Select();
            foreach(var batchId in batchIds)
            {
                BatchResponse = OpenAIClient.BatchEndpoint.RetrieveBatchAsync(batchId).Result;
                if (BatchResponse.Status.Equals(BatchStatus.Completed))
                {
                    RetrieveBatchResponse();
                }
            }
        }

        static void RetrieveBatchResponse()
        {
            if(BatchResponse == null)
            {
                return;
            }
            var fileId = BatchResponse.OutputFileId;
            var downloadedFilePath = OpenAIClient.FilesEndpoint.DownloadFileAsync(fileId, "path/to/your/save/directory").Result;
        }
    }

}
