﻿using OpenAI;
using landerist_library.Configuration;
using System.Text.Json;
using landerist_library.Websites;
using System.Text.Json.Serialization;
using landerist_library.Scrape;
using landerist_library.Logs;
using landerist_library.Database;
using OpenAI.Batch;


namespace landerist_library.Parse.Listing.OpenAI.Batch
{
    public class BatchDownload
    {
        private static readonly OpenAIClient OpenAIClient = new(PrivateConfig.OPENAI_API_KEY);

        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false) },
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };

        public static void Start()
        {
            Download();
            Delete();
        }

        public static void Download()
        {
            var batchIds = Batches.SelectNonDownloaded();
            foreach (var batchId in batchIds)
            {
                var batchResponse = GetBatch(batchId);
                if (batchResponse == null || !BatchIsCompleted(batchResponse))
                {
                    continue;
                }
                Download(batchResponse);
            }
        }

        private static BatchResponse? GetBatch(string batchId)
        {
            try
            {
                return OpenAIClient.BatchEndpoint.RetrieveBatchAsync(batchId).Result;

            }
            catch (Exception exception)
            {
                Log.WriteError("BatchDownload GetBatch", exception);
            }
            return null;
        }

        private static void Download(BatchResponse batchResponse)
        {
            if (Download(batchResponse.OutputFileId) && Download(batchResponse.ErrorFileId))
            {
                Batches.UpdateToDownloaded(batchResponse.Id);
            }
        }

        private static bool Download(string? fileId)
        {
            if (string.IsNullOrEmpty(fileId))
            {
                return true;
            }
            var filePath = DownloadFile(fileId);
            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }
            bool sucess = ReadFile(filePath);
            File.Delete(filePath);
            return sucess;
        }

        private static bool BatchIsCompleted(BatchResponse batchResponse)
        {
            return batchResponse.Status.Equals(BatchStatus.Completed);
        }

        private static string? DownloadFile(string? fileId)
        {
            if (string.IsNullOrEmpty(fileId))
            {
                return null;
            }
            try
            {
                return OpenAIClient.FilesEndpoint.DownloadFileAsync(fileId, Config.BATCH_DIRECTORY).Result;
            }
            catch (Exception exception)
            {
                Log.WriteError("BatchDownload DownloadFile", exception);
            }
            return null;
        }

        private static bool ReadFile(string filePath)
        {
            try
            {
                var lines = File.ReadAllLines(filePath);
                ReadLines(lines);
                return true;
            }
            catch (Exception exception)
            {
                Log.WriteError("BatchDownload ReadFile", exception);
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
                        Log.WriteError("BatchDownload ReadLines. Error reading line: ", line);
                    }
                }
                catch (Exception exception)
                {
                    Log.WriteError("BatchDownload ReadLines", exception);
                }
            });

            Log.WriteInfo("BatchDownload", $"{readed}/{total}, Errors: {errors}");
        }

        private static bool ReadLine(string line)
        {
            var batchResponseLine = JsonSerializer.Deserialize<BatchLineResponse?>(line, JsonSerializerOptions);

            if (batchResponseLine == null)
            {
                Log.WriteError("BatchDownload ReadLine", "batchResponse is null. Line: " + line);
                return false;
            }

            var page = Pages.GetPage(batchResponseLine.CustomId);
            if (page == null)
            {
                Log.WriteError("BatchDownload ReadLine", "Page is null. CustomId: " + batchResponseLine.CustomId);
                return false;
            }

            if (!batchResponseLine.Response.StatusCode.Equals(200))
            {
                Log.WriteError("BatchDownload ReadLine", "Not 200 StatusCode. CustomId: " + batchResponseLine.CustomId);
                page.SetWaitingAIParsingRequest();
                return page.Update(false);
            }

            page.RemoveWaitingAIParsing();
            page.SetResponseBodyFromZipped();
            page.RemoveResponseBodyZipped();

            var (pageType, listing) = ParseListing.ParseOpenAI(page, batchResponseLine.Response.Body);
            return new PageScraper(page).SetPageType(pageType, listing);
        }

        public static void Delete()
        {
            var batchIds = Batches.SelectDownloaded();
            foreach (var batchId in batchIds)
            {
                var batchResponse = GetBatch(batchId);
                if (batchResponse == null || !BatchIsCompleted(batchResponse))
                {
                    continue;
                }
                Delete(batchResponse);
            }
        }

        private static void Delete(BatchResponse batchResponse)
        {
            if (DeleteFiles(batchResponse))
            {
                Batches.Delete(batchResponse.Id);
            }
        }


        public static bool DeleteFiles(BatchResponse batchResponse)
        {
            return
                DeleteFile(batchResponse.OutputFileId, true) &&
                DeleteFile(batchResponse.InputFileId, true) &&
                DeleteFile(batchResponse.ErrorFileId, true);
        }

        private static bool DeleteFile(string fileId, bool retry)
        {
            if (string.IsNullOrEmpty(fileId))
            {
                return true;
            }
            try
            {
                OpenAIClient.FilesEndpoint.DeleteFileAsync(fileId).Wait();
                return true;
            }
            catch (Exception exception)
            {
                Log.WriteError("BatchDownload DeleteFile", exception);
                if (retry)
                {
                    Thread.Sleep(5000);
                    return DeleteFile(fileId, false);
                }
            }
            return false;
        }
    }
}
