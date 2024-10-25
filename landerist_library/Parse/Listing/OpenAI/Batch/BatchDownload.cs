﻿using landerist_library.Database;
using OpenAI.Batch;
using OpenAI;
using landerist_library.Configuration;
using System.Text.Json;
using landerist_library.Websites;
using System.Text.Json.Serialization;
using landerist_library.Scrape;
using landerist_library.Logs;


namespace landerist_library.Parse.Listing.OpenAI.Batch
{
    public class BatchDownload
    {
        private static global::OpenAI.Batch.BatchResponse? BatchResponse;

        private static readonly OpenAIClient OpenAIClient = new(PrivateConfig.OPENAI_API_KEY);

        private static string? DownloadedFilePath;

        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false)
            },
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };

        public static void Start()
        {
            var batchIds = PendingBatches.Select();
            foreach (var batchId in batchIds)
            {
                ProcessBatch(batchId);
            }
        }

        public static void ProcessBatch(string batchId)
        {
            try
            {
                BatchResponse = OpenAIClient.BatchEndpoint.RetrieveBatchAsync(batchId).Result;
                ProcessBatchResponse();
            }
            catch (Exception exception)
            {
                Log.WriteLogErrors("BatchDownload ProcessBatch", "batchId: " + batchId, exception);
            }
        }

        public static void ProcessBatchResponse()
        {
            if (BatchResponse == null || !BatchResponse.Status.Equals(BatchStatus.Completed))
            {
                return;
            }

            if (RetrieveBatchResponse())
            {
                if (ReadDownloadedFile())
                {
                    DeleteFiles();
                    DeletePendingBatch();
                }
                else
                {
                    Log.WriteLogErrors("BatchDownload ProcessBatchResponse", "Error downloading file in batchId: " + BatchResponse.Id);
                }
            }
            else
            {
                Log.WriteLogErrors("BatchDownload ProcessBatchResponse", "Error retrieving batchId: " + BatchResponse.Id);
            }
        }

        public static bool RetrieveBatchResponse()
        {
            if (BatchResponse == null || BatchResponse.OutputFileId == null)
            {
                return false;
            }
            try
            {
                DownloadedFilePath = OpenAIClient.FilesEndpoint.DownloadFileAsync(BatchResponse.OutputFileId, Config.BATCH_DIRECTORY).Result;

                return !string.IsNullOrEmpty(DownloadedFilePath);
            }
            catch (Exception exception)
            {
                Log.WriteLogErrors("BatchDownload RetrieveBatchResponse", exception);
                return false;
            }
        }

        public static bool ReadDownloadedFile()
        {
            if (string.IsNullOrEmpty(DownloadedFilePath))
            {
                return false;
            }

            try
            {
                var lines = File.ReadAllLines(DownloadedFilePath);
                ReadLines(lines);
                return true;
            }
            catch (Exception exception)
            {
                Log.WriteLogErrors("BatchDownload ReadDownloadedFile", exception);
                return false;
            }
        }

        private static void ReadLines(string[] lines)
        {
            int total = lines.Length;
            int readed = 0;
            int errors = 0;

            Parallel.ForEach(lines, new ParallelOptions()
            {
                MaxDegreeOfParallelism = Config.MAX_DEGREE_OF_PARALLELISM

            }, line =>
            {
                try
                {
                    if (ReadLine(line))
                    {
                        Interlocked.Increment(ref readed);
                    }
                    else
                    {
                        Interlocked.Increment(ref errors);
                        Log.WriteLogErrors("BatchDownload ReadLines. Error reading line: ", line);
                    }
                }
                catch (Exception exception)
                {
                    Log.WriteLogErrors("BatchDownload ReadLines", exception);
                }
            });

            Log.WriteLogInfo("BatchDownload", $"Total lines: {total}, Readed: {readed}, Errors: {errors}");
        }

        public static bool ReadLine(string line)
        {
            var batchResponse = JsonSerializer.Deserialize<BatchResponse?>(line, JsonSerializerOptions);

            if (batchResponse == null)
            {
                Log.WriteLogErrors("BatchDownload ReadLine", "batchResponse is null. Line: " + line);    
                return false;
            }

            if (!batchResponse.Response.StatusCode.Equals(200))
            {
                Log.WriteLogErrors("BatchDownload ReadLine", "Error StatusCode. CustomId: " + batchResponse.CustomId + " StatusCode: " + batchResponse.Response.StatusCode);
                return false;
            }

            var page = Pages.GetPage(batchResponse.CustomId);
            if (page == null)
            {
                Log.WriteLogErrors("BatchDownload ReadLine", "Page is null. CustomId: " + batchResponse.CustomId);
                return false;
            }

            page.SetResponseBodyFromZipped();
            page.RemoveWaitingAIParsing();
            page.RemoveResponseBodyZipped();

            var (pageType, listing, _) = ParseListing.ParseOpenAI(page, batchResponse.Response.Body);                        
            return new PageScraper(page).SetPageType(pageType, listing);
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

        public static bool DeletePendingBatch()
        {
            if (BatchResponse == null)
            {
                return false;
            }
            return PendingBatches.Delete(BatchResponse.Id);
        }
    }
}
