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
        private static readonly object Sync = new();

        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            WriteIndented = false
        };

        private static readonly OpenAIClient OpenAIClient = new(PrivateConfig.OPENAI_API_KEY);

        public static void Start()
        {
            var pages = Pages.SelectWaitingAIParsing();
            pages = Filter(pages);
            if (pages.Count < Config.MIN_PAGES_PER_BATCH)
            {
                return;
            }
            string? filePath = CreateFile(pages);
            if (string.IsNullOrEmpty(filePath))
            {
                Log.WriteLogErrors("BatchUpload Start", "Error creating file");
                return;
            }

            var fileResponse = UploadFile(filePath);
            File.Delete(filePath);
            if (fileResponse == null)
            {
                Log.WriteLogErrors("BatchUpload Start", "Error uploading file");
                return;
            }            

            var batchResponse = CreateBatch(fileResponse);
            if (batchResponse == null)
            {
                Log.WriteLogErrors("BatchUpload Start", "Error creating batch");
                return;
            }

            SetWaitingAIResponse(pages);
            Batches.Insert(batchResponse.Id);
        }

        private static List<Page> Filter(List<Page> pages)
        {
            if (pages.Count > Config.MAX_PAGES_PER_BATCH)
            {
                pages = pages.Take(Config.MAX_PAGES_PER_BATCH).ToList();
            }
            return pages;
        }

        private static string? CreateFile(List<Page> pages)
        {
            var filePath = Config.BATCH_DIRECTORY + "batch_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_input.json";
            File.Delete(filePath);

            var added = 0;
            var errors = 0;
            Parallel.ForEach(pages, new ParallelOptions()
            {
                MaxDegreeOfParallelism = Config.MAX_DEGREE_OF_PARALLELISM
            }, page =>
            {
                if (AddToBatch(page, filePath))
                {
                    Interlocked.Increment(ref added);
                }
                else
                {
                    Interlocked.Increment(ref errors);
                }
            });
            Log.WriteLogInfo("BatchUpload", $"{added}/{pages.Count} Errors: {errors}");
            if (errors.Equals(0))
            {
                return filePath;
            }
            return null;
        }

        private static bool AddToBatch(Page page, string filePath)
        {
            try
            {
                var json = GetJson(page);
                return WriteJson(json, filePath);
            }
            catch (Exception exception)
            {
                Log.WriteLogErrors("BatchUpload AddToBatch", exception.Message);
            }
            return false;
        }

        private static string? GetJson(Page page)
        {
            page.SetResponseBodyFromZipped();
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
                    temperature = OpenAIRequest.TEMPERATURE,
                    tools = new OpenAITools().GetTools(),
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

        private static bool WriteJson(string? json, string filePath)
        {
            if (string.IsNullOrEmpty(json))
            {
                return false;
            }
            lock (Sync)
            {
                File.AppendAllText(filePath, json + Environment.NewLine);
            }
            return true;
        }


        static FileResponse? UploadFile(string filePath)
        {
            try
            {
                return OpenAIClient.FilesEndpoint.UploadFileAsync(filePath, FilePurpose.Batch).GetAwaiter().GetResult();
            }
            catch (Exception exception)
            {
                Log.WriteLogErrors("BatchUpload UploadFile", exception);
            }
            return null;
        }

        static BatchResponse? CreateBatch(FileResponse fileResponse)
        {
            try
            {
                var batchRequest = new CreateBatchRequest(fileResponse.Id, Endpoint.ChatCompletions);
                return OpenAIClient.BatchEndpoint.CreateBatchAsync(batchRequest).Result;
            }
            catch (Exception exception)
            {
                Log.WriteLogErrors("BatchUpload CreateBatch", exception);
            }

            return null;
        }


        static void SetWaitingAIResponse(List<Page> pages)
        {
            Parallel.ForEach(pages,
                new ParallelOptions()
                {
                    MaxDegreeOfParallelism = Config.MAX_DEGREE_OF_PARALLELISM
                },
                page =>
            {
                page.SetWaitingAIParsingResponse();
                page.Update(false);
            });
        }
    }
}
