using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Logs;
using landerist_library.Websites;
using System.Text.Json;

namespace landerist_library.Parse.ListingParser.OpenAI.Batch
{
    public class BatchUpload: BatchClient
    {
        private static readonly object SyncWrite = new();

        const long MAX_FILE_SIZE_IN_BYTES = Config.MAX_BATCH_FILE_SIZE_MB * 1024 * 1024;

        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            WriteIndented = false
        };

        private static List<Page> pages = [];

        private static List<string> UriHashes = [];


        public static void Start()
        {
            Clear();
            bool sucess = StartUpload();
            if (pages.Count >= Config.MAX_PAGES_PER_BATCH && sucess)
            {
                Start();
            }
            Clear();
        }

        private static bool StartUpload()
        {
            pages = Pages.SelectWaitingAIParsing();
            if (pages.Count < Config.MIN_PAGES_PER_BATCH)
            {
                return false;
            }
            var filePath = CreateFile();
            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }
            var fileResponse = UploadFile(filePath);
            if (fileResponse == null || string.IsNullOrEmpty(fileResponse.Id))
            {
                return false;
            }

            var batchResponse = CreateBatch(fileResponse);
            if (batchResponse == null || string.IsNullOrEmpty(batchResponse.Id))
            {
                //DeleteFile(fileResponse.Id);
                return false;
            }            
            
            SetWaitingAIResponse();
            Batches.Insert(batchResponse.Id);
            Log.WriteInfo("batch", $"Uploaded {pages.Count}");
            return true;
        }

        private static void Clear()
        {
            Parallel.ForEach(pages, page => page.Dispose());
            pages.Clear();
            UriHashes.Clear();
        }

        private static string? CreateFile()
        {
            var filePath = Config.BATCH_DIRECTORY + "batch_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_input.json";
            File.Delete(filePath);

            UriHashes = [];
            var sync = new object();
            var errors = 0;
            var skipped = 0;

            Parallel.ForEach(pages, new ParallelOptions()
            {
                //MaxDegreeOfParallelism = Config.MAX_DEGREE_OF_PARALLELISM
                //MaxDegreeOfParallelism = 1
            },
            (page, state) =>
            {
                if (!CanWriteFile(filePath))
                {
                    Interlocked.Increment(ref skipped);
                    state.Stop();                    
                }
                else if (!AddToBatch(page, filePath))
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

        private static bool AddToBatch(Page page, string filePath)
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

        private static string? GetJson(Page page)
        {
            page.SetResponseBodyFromZipped();
            var userInput = UserInputText.GetText(page);
            page.RemoveResponseBody();

            if (string.IsNullOrEmpty(userInput))
            {
                Log.WriteError("BatchUpload GetJson", "Error getting user input. Page: " + page.UriHash);
                return null;
            }

            StructuredRequestData structuredRequestData = new()
            {
                custom_id = page.UriHash,
                method = "POST",
                url = "/v1/chat/completions",
                body = GetStructuredBody(userInput)
            };

            return JsonSerializer.Serialize(structuredRequestData, JsonSerializerOptions);
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

        static void SetWaitingAIResponse()
        {
            Parallel.ForEach(UriHashes,
                new ParallelOptions()
                {
                    //MaxDegreeOfParallelism = Config.MAX_DEGREE_OF_PARALLELISM
                    //MaxDegreeOfParallelism = 1
                },
                uriHash =>
            {
                Pages.UpdateWaitingAIParsing(uriHash, false);
            });
        }
    }
}
