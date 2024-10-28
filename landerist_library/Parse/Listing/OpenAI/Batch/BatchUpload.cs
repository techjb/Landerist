﻿using landerist_library.Configuration;
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
                        Log.WriteLogErrors("BatchUpload Start", "Error creating batch");
                    }
                }
                else
                {
                    Log.WriteLogErrors("BatchUpload Start", "Error uploading file");
                }
            }
            else
            {
                Log.WriteLogErrors("BatchUpload Start", "Error creating file");
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
            var added = 0;
            var errors = 0;
            Parallel.ForEach(Pages, new ParallelOptions()
            {
                MaxDegreeOfParallelism = Config.MAX_DEGREE_OF_PARALLELISM
            }, page =>
            {
                if (AddToBatch(page))
                {
                    Interlocked.Increment(ref added);
                }
                else
                {
                    Interlocked.Increment(ref errors);
                }
            });
            Log.WriteLogInfo("BatchUpload", $"{added}/{totalPages} Errors: {errors}");
            return errors.Equals(0);
        }

        private static void InitFilePath()
        {
            FilePath = Config.BATCH_DIRECTORY +
                "batch_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_input.json";

            File.Delete(FilePath);
        }

        private static bool AddToBatch(Page page)
        {
            try
            {
                var json = GetJson(page);
                return WriteJson(json);
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
                Log.WriteLogErrors("BatchUpload UploadFile", exception);

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
            try
            {
                var batchRequest = new CreateBatchRequest(FileResponse.Id, Endpoint.ChatCompletions);
                BatchResponse = OpenAIClient.BatchEndpoint.CreateBatchAsync(batchRequest).Result;
            }
            catch (Exception exception)
            {
                Log.WriteLogErrors("BatchUpload CreateBatch", exception);
                return false;
            }

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
                page.SetWaitingAIParsingResponse();
                page.Update(false);
            });
        }
    }
}
