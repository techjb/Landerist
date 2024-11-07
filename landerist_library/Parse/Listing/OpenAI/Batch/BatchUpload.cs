using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Logs;
using landerist_library.Websites;
using OpenAI;
using OpenAI.Batch;
using OpenAI.Files;
using System.Text.Json;

namespace landerist_library.Parse.Listing.OpenAI.Batch
{
    public class BatchUpload
    {
        private static readonly object SyncWrite = new();

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
            (var filePath, pages) = CreateFile(pages);
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            var fileResponse = UploadFile(filePath);
            File.Delete(filePath);
            if (fileResponse == null || string.IsNullOrEmpty(fileResponse.Id))
            {
                return;
            }

            var batchResponse = CreateBatch(fileResponse);
            if (batchResponse == null)
            {
                OpenAIClient.FilesEndpoint.DeleteFileAsync(fileResponse.Id).Wait();
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

        private static (string? filePath, List<Page> added) CreateFile(List<Page> pages)
        {
            var filePath = Config.BATCH_DIRECTORY + "batch_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_input.json";
            File.Delete(filePath);

            List<Page> added = [];
            var sync = new object();
            var errors = 0;
            var skipped = 0;

            Parallel.ForEach(pages, new ParallelOptions()
            {
                MaxDegreeOfParallelism = Config.MAX_DEGREE_OF_PARALLELISM
            }, page =>
            {
                if (!CanWriteFile(filePath))
                {
                    Interlocked.Increment(ref skipped);
                    return;
                }
                if (!AddToBatch(page, filePath))
                {
                    Interlocked.Increment(ref errors);
                    return;
                }
                lock (sync)
                {
                    added.Add(page);
                }
            });
            //Log.WriteInfo("BatchUpload", $"{added.Count}/{pages.Count} Errors: {errors} Skipped: {skipped}");
            Log.WriteInfo("BatchUpload", $"Added {added.Count}");
            if (errors > 0)
            {
                filePath = null;
                Log.WriteError("BatchUpload CreateFile", "Error creating file. Errors: " + errors);
            }
            return (filePath, added);
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
                Log.WriteError("BatchUpload AddToBatch", exception.Message);
            }
            return false;
        }

        private static bool CanWriteFile(string filePath)
        {
            lock (SyncWrite)
            {
                if (!File.Exists(filePath))
                {
                    return true;
                }
                const long sizeInByes = Config.MAX_BATCH_FILE_SIZE_MB * 1024 * 1024;
                FileInfo fileInfo = new(filePath);
                return fileInfo.Length < sizeInByes;
            }
        }

        private static string? GetJson(Page page)
        {
            page.SetResponseBodyFromZipped();
            var userInput = UserTextInput.GetText(page);
            if (string.IsNullOrEmpty(userInput))
            {
                return null;
            }

            if (Config.STRUCTURED_OUTPUT)
            {
                StructuredRequestData structuredRequestData = new()
                {
                    custom_id = page.UriHash,
                    method = "POST",
                    url = "/v1/chat/completions",
                    body = GetStructuredBody(userInput)
                };

                return JsonSerializer.Serialize(structuredRequestData, JsonSerializerOptions);
            }

            NonStructuredRequestData nonStructuredRequestData = new()
            {
                custom_id = page.UriHash,
                method = "POST",
                url = "/v1/chat/completions",
                body = GetNonStructuredBody(userInput)
            };

            return JsonSerializer.Serialize(nonStructuredRequestData, JsonSerializerOptions);
        }

        private static NonStructuredBody GetNonStructuredBody(string userInput)
        {
            return new NonStructuredBody
            {
                model = OpenAIRequest.MODEL_NAME,
                temperature = OpenAIRequest.TEMPERATURE,
                messages = GetBatchMessages(userInput),
                response_format = new NonStructuredResponseFormat
                {
                    type = "json_object"
                },
                tools = new OpenAITools().GetTools(),
                tool_choice = OpenAIRequest.TOOL_CHOICE
            };
        }

        private static StructuredBody GetStructuredBody(string userInput)
        {
            return new StructuredBody
            {
                model = OpenAIRequest.MODEL_NAME,
                temperature = OpenAIRequest.TEMPERATURE,
                messages = GetBatchMessages(userInput),
                response_format = new StructuredResponseFormat
                {
                    type = "json_schema",
                    json_schema = OpenAIRequest.GetOpenAIJsonSchema()
                },
            };
        }

        private static List<BatchMessage> GetBatchMessages(string userInput)
        {
            return [
                    new BatchMessage
                    {
                        role = "system",
                        content = ParseListingRequest.GetSystemPrompt()
                    },
                    new BatchMessage {
                        role = "user",
                        content = userInput
                    }
                ];
        }

        private static bool WriteJson(string? json, string filePath)
        {
            if (string.IsNullOrEmpty(json))
            {
                return false;
            }
            lock (SyncWrite)
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
                Log.WriteError("BatchUpload UploadFile", exception);
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
                Log.WriteError("BatchUpload CreateBatch", exception);
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
