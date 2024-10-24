using landerist_library.Configuration;
using landerist_library.Websites;
using landerist_library.Logs;
using System.Text.Json;
using OpenAI.Files;
using OpenAI;
using OpenAI.Batch;
using landerist_library.Database;

namespace landerist_library.Parse.Listing.OpenAI.Batch
{
    public class BatchUpload
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

        private static global::OpenAI.Batch.BatchResponse? BatchResponse;

        public static void Start()
        {
            Pages = Websites.Pages.SelectWaitingAIParsing();
            FilterMaxPagesPerBatch();
            if (Pages.Count < Config.MIN_PAGES_PER_BATCH)
            {
                return;
            }
            if (CreateFile())
            {
                if (UploadFile())
                {
                    if (CreateBatch())
                    {
                        InsertPendingBatch();
                        SetWaitingAIResponse();
                    }
                    else
                    {
                        Log.WriteLogErrors("OpenAIBatch Start", "Error creating batch");
                    }
                }
                else
                {
                    Log.WriteLogErrors("OpenAIBatch Start", "Error uploading file");
                }                
            }
            else
            {
                Log.WriteLogErrors("OpenAIBatch Start", "Error creating file");
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
            InitFilePath();
            var totalPages = Pages.Count;
            var counter = 0;
            var errors = 0;
            Parallel.ForEach(Pages, new ParallelOptions()
            {
                MaxDegreeOfParallelism = Config.MAX_DEGREE_OF_PARALLELISM
            }, page =>
            {
                if (AddToBatch(page))
                {
                    page.Update(false);
                    Interlocked.Increment(ref counter);
                }
                else
                {
                    Interlocked.Increment(ref errors);
                }
            });
            Log.WriteLogInfo("OpenAIBatch", $"Upload. Total pages: {totalPages}, Added to batch: {counter}, Errors: {errors}");
            return errors.Equals(0);
        }

        private static void InitFilePath()
        {
            FilePath = Config.BATCH_DIRECTORY +
                "openai_batch_upload_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".json";

            File.Delete(FilePath);
        }

        private static bool AddToBatch(Page page)
        {
            try
            {
                page.SetWaitingAIResponse();
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

            var tools = new OpenAITools().GetTools();

            var requestData = new RequestData
            {
                custom_id = page.UriHash,
                method = "POST",
                url = "/v1/chat/completions",
                body = new Body
                {
                    model = OpenAIRequest.MODEL_NAME,
                    temperature = OpenAIRequest.TEMPERATURE,
                    tools = tools,
                    tool_choice = OpenAIRequest.TOOL_CHOICE,
                    response_format = new ResponseFormat
                    {
                        type = "json_object"
                    },
                    messages =
                    [
                        new BatchMessage
                        {
                            role = "system",
                            content = ParseListingRequest.GetSystemPrompt()
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

        static bool InsertPendingBatch()
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
    }
}
